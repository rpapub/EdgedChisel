<#
.SYNOPSIS
    Copies all shared .cs files into UiPath project Code folders.

.DESCRIPTION
    This script recursively scans predefined source folders (e.g., ./src/Mail) 
    and copies all .cs files into the corresponding UiPath project folders 
    (e.g., ./src/UiPath/MailWrapperDemo/Code), overwriting existing files.

    Intended for manual or semi-automated syncing of shared logic across projects 
    without requiring submodules or package distribution.

.NOTES
    - Assumes script is run from the repo root.
    - Customize source and target mappings inside the script body.
    - All copied files are treated as part of the destination repo (not linked).

#>
