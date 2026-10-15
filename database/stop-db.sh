#!/usr/bin/env bash
# Stops and removes the chinook-mssql container. The data goes with it; run start-db.sh and init-db.sh to get it back.
set -euo pipefail
CONTAINER="${CHINOOK_CONTAINER:-chinook-mssql}"
if docker inspect "$CONTAINER" >/dev/null 2>&1; then
  docker rm -f "$CONTAINER" >/dev/null
  echo "Removed container $CONTAINER."
else
  echo "Container $CONTAINER does not exist; nothing to do."
fi
