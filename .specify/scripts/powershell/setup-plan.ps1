#!/usr/bin/env pwsh
# Setup implementation plan for a feature

[CmdletBinding()]
param(
    [switch]$Json,
    [switch]$Help,
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$RemainingArgs
)

$ErrorActionPreference = 'Stop'

if ($Help) {
    Write-Output "Usage: ./setup-plan.ps1 [-Json] [-Help]"
    exit 0
}

if ($RemainingArgs.Count -gt 0) {
    [Console]::Error.WriteLine("ERROR: Unknown option '$($RemainingArgs[0])'")
    exit 1
}

. "$PSScriptRoot/common.ps1"

$paths = Get-FeaturePathsEnv -ReturnNullOnError
if (-not $paths) {
    [Console]::Error.WriteLine("ERROR: Failed to resolve feature paths")
    exit 1
}

New-Item -ItemType Directory -Path $paths.FEATURE_DIR -Force | Out-Null

if (Test-Path $paths.IMPL_PLAN -PathType Leaf) {
    if ($Json) {
        [Console]::Error.WriteLine("Plan already exists at $($paths.IMPL_PLAN), skipping template copy")
    } else {
        Write-Output "Plan already exists at $($paths.IMPL_PLAN), skipping template copy"
    }
} else {
    $content = Resolve-TemplateContent -TemplateName 'plan-template' -RepoRoot $paths.REPO_ROOT
    if ($null -ne $content) {
        $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
        [System.IO.File]::WriteAllText($paths.IMPL_PLAN, $content, $utf8NoBom)
        if ($Json) {
            [Console]::Error.WriteLine("Copied plan template to $($paths.IMPL_PLAN)")
        } else {
            Write-Output "Copied plan template to $($paths.IMPL_PLAN)"
        }
    } else {
        if ($Json) {
            [Console]::Error.WriteLine("Warning: Plan template not found")
        } else {
            Write-Output "Warning: Plan template not found"
        }
        New-Item -ItemType File -Path $paths.IMPL_PLAN -Force | Out-Null
    }
}

if ($Json) {
    [PSCustomObject]@{
        FEATURE_SPEC = $paths.FEATURE_SPEC
        IMPL_PLAN    = $paths.IMPL_PLAN
        FEATURE_DIR  = $paths.FEATURE_DIR
        BRANCH       = $paths.CURRENT_BRANCH
    } | ConvertTo-Json -Compress
} else {
    Write-Output "FEATURE_SPEC: $($paths.FEATURE_SPEC)"
    Write-Output "IMPL_PLAN: $($paths.IMPL_PLAN)"
    Write-Output "FEATURE_DIR: $($paths.FEATURE_DIR)"
    Write-Output "BRANCH: $($paths.CURRENT_BRANCH)"
}
