#!/usr/bin/env pwsh
# Starts the shared SQL Server container used by every example.
# Idempotent: a running container is left alone, a stopped one is started, a missing one is created.
$ErrorActionPreference = 'Stop'

$Container = if ($env:CHINOOK_CONTAINER) { $env:CHINOOK_CONTAINER } else { 'chinook-mssql' }
$Image = 'mcr.microsoft.com/mssql/server:2022-latest'
$SaPassword = if ($env:CHINOOK_SA_PASSWORD) { $env:CHINOOK_SA_PASSWORD } else { 'Chinook!Passw0rd' }
$Port = if ($env:CHINOOK_PORT) { $env:CHINOOK_PORT } else { '1433' }
$TimeoutSeconds = 120

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw 'docker is not on PATH.' }

$state = docker inspect -f '{{.State.Status}}' $Container 2>$null
if ($LASTEXITCODE -ne 0) { $state = '' }

switch ($state) {
    'running' {
        Write-Host "Container $Container is already running."
        exit 0
    }
    '' {
        Write-Host "Creating container $Container from $Image on port $Port..."
        docker pull --platform linux/amd64 $Image
        docker run -d --name $Container `
            -e 'ACCEPT_EULA=Y' `
            -e 'MSSQL_PID=Developer' `
            -e "MSSQL_SA_PASSWORD=$SaPassword" `
            -p "${Port}:1433" `
            --platform linux/amd64 `
            $Image | Out-Null
        if ($LASTEXITCODE -ne 0) { throw "docker run failed." }
    }
    default {
        Write-Host "Container $Container exists ($state); starting it..."
        docker start $Container | Out-Null
    }
}

function Get-SqlCmd {
    docker exec $Container test -x /opt/mssql-tools18/bin/sqlcmd 2>$null
    if ($LASTEXITCODE -eq 0) { return @('/opt/mssql-tools18/bin/sqlcmd', '-C') }
    docker exec $Container test -x /opt/mssql-tools/bin/sqlcmd 2>$null
    if ($LASTEXITCODE -eq 0) { return @('/opt/mssql-tools/bin/sqlcmd') }
    return $null
}

Write-Host -NoNewline 'Waiting for SQL Server to answer SELECT 1'
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
while ((Get-Date) -lt $deadline) {
    $sqlcmd = Get-SqlCmd
    if ($sqlcmd) {
        docker exec $Container @sqlcmd -S localhost -U sa -P $SaPassword -Q 'SELECT 1' -h -1 *> $null
        if ($LASTEXITCODE -eq 0) {
            Write-Host ''
            Write-Host "SQL Server is ready on localhost,$Port."
            exit 0
        }
    }
    Write-Host -NoNewline '.'
    Start-Sleep -Seconds 2
}

Write-Host ''
Write-Error "SQL Server did not answer within ${TimeoutSeconds}s. Check: docker logs $Container"
exit 1
