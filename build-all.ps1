#!/usr/bin/env pwsh
# Builds every example solution under 01- through 05- in Release and prints a pass/fail table.
# A folder containing a .skip-build marker is skipped, along with any solution inside it.
# Projects inside a solution can also carry a .skip-build marker; those projects are left out.
$ErrorActionPreference = 'Continue'
$root = $PSScriptRoot
Set-Location $root

function Test-SkipMarker([string]$dir) {
    while ($dir -and $dir -ne $root) {
        if (Test-Path (Join-Path $dir '.skip-build')) { return $true }
        $dir = Split-Path $dir -Parent
    }
    return $false
}

$results = @()
$failed = $false

$solutions = Get-ChildItem -Path $root -Directory -Filter '0[1-5]-*' |
    ForEach-Object { Get-ChildItem -Path $_.FullName -Recurse -Filter '*.sln' } |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
    Sort-Object FullName

foreach ($sln in $solutions) {
    $dir = $sln.DirectoryName
    $name = [IO.Path]::GetRelativePath($root, $sln.FullName)
    if (Test-SkipMarker $dir) {
        $results += [pscustomobject]@{ Result = 'SKIP'; Solution = $name }
        continue
    }

    $skipped = Get-ChildItem -Path $dir -Recurse -Force -Filter '.skip-build' |
        Where-Object { $_.DirectoryName -ne $dir -and $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } |
        ForEach-Object { $_.DirectoryName }

    if (-not $skipped) {
        Write-Host ">> $name"
        dotnet build $sln.FullName -c Release --nologo -v quiet --disable-build-servers
        $ok = $LASTEXITCODE -eq 0
        $label = $name
    }
    else {
        $ok = $true
        $projects = Get-ChildItem -Path $dir -Recurse -Filter '*.csproj' |
            Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } | Sort-Object FullName
        foreach ($proj in $projects) {
            $inSkipped = $skipped | Where-Object { $proj.DirectoryName -eq $_ -or $proj.DirectoryName.StartsWith($_ + [IO.Path]::DirectorySeparatorChar) }
            if ($inSkipped) { continue }
            Write-Host ">> $($proj.FullName)"
            dotnet build $proj.FullName -c Release --nologo -v quiet --disable-build-servers
            if ($LASTEXITCODE -ne 0) { $ok = $false }
        }
        $label = "$name (partial: $(@($skipped).Count) project(s) skipped)"
    }

    if ($ok) {
        $results += [pscustomobject]@{ Result = 'PASS'; Solution = $label }
    }
    else {
        $results += [pscustomobject]@{ Result = 'FAIL'; Solution = $label }
        $failed = $true
    }
}

$results | Format-Table -AutoSize
if ($failed) { exit 1 } else { exit 0 }
