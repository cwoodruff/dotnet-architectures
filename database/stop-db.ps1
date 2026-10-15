#!/usr/bin/env pwsh
# Stops and removes the chinook-mssql container. The data goes with it; run start-db.ps1 and init-db.ps1 to get it back.
$Container = if ($env:CHINOOK_CONTAINER) { $env:CHINOOK_CONTAINER } else { 'chinook-mssql' }
docker inspect $Container *> $null
if ($LASTEXITCODE -eq 0) {
    docker rm -f $Container | Out-Null
    Write-Host "Removed container $Container."
}
else {
    Write-Host "Container $Container does not exist; nothing to do."
}
