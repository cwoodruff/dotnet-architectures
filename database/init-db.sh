#!/usr/bin/env bash
# Loads chinook.docker.sql into the running chinook-mssql container.
#
# Usage: ./init-db.sh [--tenants US,EU] [--force]
#   --tenants  comma-separated suffixes; for each one a copy of the script with every [Chinook]
#              replaced by [Chinook_<suffix>] is loaded as well (multi-tenant-saas needs US,EU)
#   --force    drop and recreate databases that already exist (default: skip them)
set -euo pipefail

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONTAINER="${CHINOOK_CONTAINER:-chinook-mssql}"
SA_PASSWORD="${CHINOOK_SA_PASSWORD:-Chinook!Passw0rd}"
SCRIPT="$here/chinook.docker.sql"

tenants=""
force=0
while [[ $# -gt 0 ]]; do
  case "$1" in
    --tenants) tenants="${2:?--tenants needs a value such as US,EU}"; shift 2 ;;
    --tenants=*) tenants="${1#*=}"; shift ;;
    --force) force=1; shift ;;
    -h|--help) sed -n '2,8p' "$0"; exit 0 ;;
    *) echo "Unknown argument: $1" >&2; exit 1 ;;
  esac
done

[[ -f "$SCRIPT" ]] || { echo "ERROR: $SCRIPT not found. Run ./generate-docker-sql.sh first." >&2; exit 1; }
[[ "$(docker inspect -f '{{.State.Running}}' "$CONTAINER" 2>/dev/null || true)" == "true" ]] \
  || { echo "ERROR: container $CONTAINER is not running. Run ./start-db.sh first." >&2; exit 1; }

if docker exec "$CONTAINER" test -x /opt/mssql-tools18/bin/sqlcmd 2>/dev/null; then
  SQLCMD=(/opt/mssql-tools18/bin/sqlcmd -C)
elif docker exec "$CONTAINER" test -x /opt/mssql-tools/bin/sqlcmd 2>/dev/null; then
  SQLCMD=(/opt/mssql-tools/bin/sqlcmd)
else
  echo "ERROR: sqlcmd not found in $CONTAINER under /opt/mssql-tools18 or /opt/mssql-tools." >&2
  exit 1
fi

run_sql() {
  docker exec "$CONTAINER" "${SQLCMD[@]}" -S localhost -U sa -P "$SA_PASSWORD" "$@"
}

db_exists() {
  [[ "$(run_sql -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$1') IS NULL THEN 0 ELSE 1 END" | tr -d '[:space:]')" == "1" ]]
}

work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT

load() {
  local db="$1" file="$2"
  if db_exists "$db"; then
    if [[ $force -eq 0 ]]; then
      echo "Database $db already exists; skipping (use --force to recreate)."
      return
    fi
    echo "Dropping $db (--force)..."
    run_sql -b -Q "ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$db];" >/dev/null
  fi
  echo "Loading $db..."
  docker cp "$file" "$CONTAINER:/tmp/$db.sql" >/dev/null
  # The script is UTF-16 with a BOM, which sqlcmd detects on its own.
  if ! run_sql -i "/tmp/$db.sql" > "$work/$db.log" 2>&1 || grep -q '^Msg ' "$work/$db.log"; then
    echo "ERROR: loading $db reported errors:" >&2
    grep -A2 '^Msg ' "$work/$db.log" | head -20 >&2
    docker exec -u 0 "$CONTAINER" rm -f "/tmp/$db.sql"
    exit 1
  fi
  docker exec -u 0 "$CONTAINER" rm -f "/tmp/$db.sql"
  echo "Loaded $db: $(run_sql -d "$db" -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM dbo.Artist" | tr -d '[:space:]') artists."
}

load Chinook "$SCRIPT"

if [[ -n "$tenants" ]]; then
  IFS=',' read -ra suffixes <<< "$tenants"
  for suffix in "${suffixes[@]}"; do
    suffix="$(echo "$suffix" | tr -d '[:space:]')"
    [[ "$suffix" =~ ^[A-Za-z0-9_]+$ ]] || { echo "ERROR: invalid tenant suffix '$suffix'." >&2; exit 1; }
    db="Chinook_$suffix"
    python3 - "$SCRIPT" "$work/$db.sql" "$db" <<'PY'
import sys
src, dst, db = sys.argv[1:4]
text = open(src, "rb").read().decode("utf-16")
text = text.replace("[Chinook]", f"[{db}]").replace("N'Chinook'", f"N'{db}'")
open(dst, "wb").write(text.encode("utf-16"))
PY
    load "$db" "$work/$db.sql"
  done
fi
