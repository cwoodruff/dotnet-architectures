#!/usr/bin/env bash
# Starts the shared SQL Server container used by every example.
# Idempotent: a running container is left alone, a stopped one is started, a missing one is created.
set -euo pipefail

CONTAINER="${CHINOOK_CONTAINER:-chinook-mssql}"
IMAGE="mcr.microsoft.com/mssql/server:2022-latest"
SA_PASSWORD="${CHINOOK_SA_PASSWORD:-Chinook!Passw0rd}"
PORT="${CHINOOK_PORT:-1433}"
TIMEOUT_SECONDS=120

command -v docker >/dev/null || { echo "ERROR: docker is not on PATH." >&2; exit 1; }

state="$(docker inspect -f '{{.State.Status}}' "$CONTAINER" 2>/dev/null || true)"

case "$state" in
  running)
    echo "Container $CONTAINER is already running."
    exit 0
    ;;
  "")
    echo "Creating container $CONTAINER from $IMAGE on port $PORT..."
    docker pull --platform linux/amd64 "$IMAGE"
    docker run -d \
      --name "$CONTAINER" \
      -e "ACCEPT_EULA=Y" \
      -e "MSSQL_PID=Developer" \
      -e "MSSQL_SA_PASSWORD=$SA_PASSWORD" \
      -p "$PORT:1433" \
      --platform linux/amd64 \
      "$IMAGE" >/dev/null
    ;;
  *)
    echo "Container $CONTAINER exists ($state); starting it..."
    docker start "$CONTAINER" >/dev/null
    ;;
esac

sqlcmd_path() {
  if docker exec "$CONTAINER" test -x /opt/mssql-tools18/bin/sqlcmd 2>/dev/null; then
    echo "/opt/mssql-tools18/bin/sqlcmd -C"
  elif docker exec "$CONTAINER" test -x /opt/mssql-tools/bin/sqlcmd 2>/dev/null; then
    echo "/opt/mssql-tools/bin/sqlcmd"
  fi
}

echo -n "Waiting for SQL Server to answer SELECT 1"
deadline=$((SECONDS + TIMEOUT_SECONDS))
while (( SECONDS < deadline )); do
  sqlcmd="$(sqlcmd_path)"
  if [[ -n "$sqlcmd" ]] && docker exec "$CONTAINER" $sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" -h -1 >/dev/null 2>&1; then
    echo
    echo "SQL Server is ready on localhost,$PORT."
    exit 0
  fi
  echo -n "."
  sleep 2
done

echo
echo "ERROR: SQL Server did not answer within ${TIMEOUT_SECONDS}s. Check: docker logs $CONTAINER" >&2
exit 1
