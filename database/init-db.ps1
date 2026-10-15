#!/usr/bin/env pwsh
# Loads chinook.docker.sql into the running chinook-mssql container.
#
# Usage: ./init-db.ps1 [-Tenants US,EU] [-Force]
#   -Tenants  suffixes; for each one a copy of the script with every [Chinook] replaced by
#             [Chinook_<suffix>] is loaded as well (multi-tenant-saas needs US,EU)
#   -Force    drop and recreate databases that already exist (default: skip them)
param(
    [string[]]$Tenants = @(),
    [switch]$Force
)
$ErrorActionPreference = 'Stop'

$Container = if ($env:CHINOOK_CONTAINER) { $env:CHINOOK_CONTAINER } else { 'chinook-mssql' }
$SaPassword = if ($env:CHINOOK_SA_PASSWORD) { $env:CHINOOK_SA_PASSWORD } else { 'Chinook!Passw0rd' }
$Script = Join-Path $PSScriptRoot 'chinook.docker.sql'

# Accept both -Tenants US,EU and -Tenants "US,EU".
$Tenants = $Tenants | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ }

if (-not (Test-Path $Script)) { throw "$Script not found. Run ./generate-docker-sql.sh first." }
$running = docker inspect -f '{{.State.Running}}' $Container 2>$null
if ($running -ne 'true') { throw "Container $Container is not running. Run ./start-db.ps1 first." }

docker exec $Container test -x /opt/mssql-tools18/bin/sqlcmd 2>$null
if ($LASTEXITCODE -eq 0) {
    $SqlCmd = @('/opt/mssql-tools18/bin/sqlcmd', '-C')
}
else {
    docker exec $Container test -x /opt/mssql-tools/bin/sqlcmd 2>$null
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd not found in $Container under /opt/mssql-tools18 or /opt/mssql-tools." }
    $SqlCmd = @('/opt/mssql-tools/bin/sqlcmd')
}

function Invoke-Sql([string[]]$Arguments) {
    docker exec $Container @SqlCmd -S localhost -U sa -P $SaPassword @Arguments
}

function Test-Database([string]$Name) {
    $result = Invoke-Sql @('-h', '-1', '-W', '-Q', "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$Name') IS NULL THEN 0 ELSE 1 END")
    return (($result | Out-String).Trim() -eq '1')
}

$work = Join-Path ([IO.Path]::GetTempPath()) ([Guid]::NewGuid())
New-Item -ItemType Directory -Path $work | Out-Null

function Import-Database([string]$Name, [string]$File) {
    if (Test-Database $Name) {
        if (-not $Force) {
            Write-Host "Database $Name already exists; skipping (use -Force to recreate)."
            return
        }
        Write-Host "Dropping $Name (-Force)..."
        Invoke-Sql @('-b', '-Q', "ALTER DATABASE [$Name] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$Name];") | Out-Null
    }
    Write-Host "Loading $Name..."
    docker cp $File "${Container}:/tmp/$Name.sql" | Out-Null
    # The script is UTF-16 with a BOM, which sqlcmd detects on its own.
    $log = Invoke-Sql @('-i', "/tmp/$Name.sql") 2>&1 | Out-String
    $exit = $LASTEXITCODE
    docker exec -u 0 $Container rm -f "/tmp/$Name.sql" | Out-Null
    if ($exit -ne 0 -or $log -match '(?m)^Msg ') {
        ($log -split "`n" | Select-String -Pattern '^Msg ' -Context 0, 1 | Select-Object -First 10) | Out-String | Write-Host
        throw "Loading $Name reported errors."
    }
    $count = (Invoke-Sql @('-d', $Name, '-h', '-1', '-W', '-Q', 'SET NOCOUNT ON; SELECT COUNT(*) FROM dbo.Artist') | Out-String).Trim()
    Write-Host "Loaded ${Name}: $count artists."
}

try {
    Import-Database 'Chinook' $Script

    foreach ($suffix in $Tenants) {
        if ($suffix -notmatch '^[A-Za-z0-9_]+$') { throw "Invalid tenant suffix '$suffix'." }
        $name = "Chinook_$suffix"
        $copy = Join-Path $work "$name.sql"
        $text = [IO.File]::ReadAllText($Script, [Text.Encoding]::Unicode)
        $text = $text.Replace('[Chinook]', "[$name]").Replace("N'Chinook'", "N'$name'")
        [IO.File]::WriteAllText($copy, $text, [Text.UnicodeEncoding]::new($false, $true))
        Import-Database $name $copy
    }
}
finally {
    Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}
