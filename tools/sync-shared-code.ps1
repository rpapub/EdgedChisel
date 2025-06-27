<#
.SYNOPSIS
    Syncs all .cs files from shared source folders into UiPath project Code folders.

.DESCRIPTION
    - Finds all ./src/* folders that are NOT ./src/UiPath and contain one or more .cs files.
    - Finds all ./src/UiPath/* folders that contain a project.json.
    - Copies all .cs files from each source into the Code/ folder of each UiPath project.
    - Prompts before overwriting unless -Force is used.
    - At the end, lists extra .cs files in Code/ folders that were not part of the sync.

.PARAMETER Force
    If specified, all .cs files will be overwritten without prompting.

.EXAMPLE
    ./tools/sync-shared-code.ps1 -Force
#>

param (
    [switch]$Force
)

$repoRoot = Resolve-Path "$PSScriptRoot\.."
$srcRoot = Join-Path $repoRoot 'src'
$uiPathRoot = Join-Path $srcRoot 'UiPath'

Write-Host "🔍 Searching for source folders with .cs files..."

$sourceFolders = Get-ChildItem -Path $srcRoot -Directory | Where-Object {
    $_.FullName -notlike "$uiPathRoot*" -and
    (Get-ChildItem -Path $_.FullName -Recurse -Filter *.cs | Measure-Object).Count -gt 0
}

if (-not $sourceFolders) {
    Write-Warning "No source folders with .cs files found."
    exit 1
}

Write-Host "✅ Found source folders:"
$sourceFolders | ForEach-Object { Write-Host "  - $($_.FullName)" }

Write-Host "`n🔍 Searching for UiPath projects..."

$targetProjects = Get-ChildItem -Path $uiPathRoot -Recurse -Filter project.json |
    Where-Object { Test-Path $_.DirectoryName } |
    Select-Object -ExpandProperty DirectoryName -Unique

if (-not $targetProjects) {
    Write-Warning "No UiPath projects found."
    exit 1
}

Write-Host "✅ Found UiPath projects:"
$targetProjects | ForEach-Object { Write-Host "  - $_" }

# Map of files synced (to detect leftovers later)
$filesCopied = @{}

foreach ($target in $targetProjects) {
    $codeFolder = Join-Path $target 'Code'
    if (-not (Test-Path $codeFolder)) {
        New-Item -ItemType Directory -Path $codeFolder | Out-Null
    }

    foreach ($source in $sourceFolders) {
        $csFiles = Get-ChildItem -Path $source.FullName -Recurse -Filter *.cs

        foreach ($file in $csFiles) {
            $destPath = Join-Path $codeFolder $file.Name
            $filesCopied[$destPath] = $true

            $shouldCopy = $true
            if ((Test-Path $destPath) -and -not $Force) {
                $response = Read-Host "❓ $($file.Name) exists in $($codeFolder). Overwrite? [y/N]"
                if ($response -ne 'y' -and $response -ne 'Y') {
                    $shouldCopy = $false
                }
            }

            if ($shouldCopy) {
                Copy-Item -Path $file.FullName -Destination $destPath -Force
                Write-Host "📄 Copied: $($file.Name) → $codeFolder"
            } else {
                Write-Host "⚠️  Skipped: $($file.Name)"
            }
        }
    }
}

Write-Host "`n🔎 Checking for untracked .cs files in Code folders..."

foreach ($target in $targetProjects) {
    $codeFolder = Join-Path $target 'Code'
    if (-not (Test-Path $codeFolder)) { continue }

    $allFiles = Get-ChildItem -Path $codeFolder -Filter *.cs
    $leftovers = $allFiles | Where-Object { -not $filesCopied[$_.FullName] }

    if ($leftovers) {
        Write-Host "`n⚠️  Untracked .cs files in ${codeFolder}:"
        $leftovers | ForEach-Object { Write-Host "  - $($_.Name)" }
    }
}

Write-Host "`n✅ Sync complete."
