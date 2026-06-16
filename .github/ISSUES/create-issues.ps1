#!/usr/bin/env pwsh
# Run: .\create-issues.ps1 -Token "ghp_yourtoken"
# Requires a GitHub PAT with 'repo' scope.

param(
    [Parameter(Mandatory)]
    [string]$Token
)

$Repo    = "itzikyis/taskflow-ai"
$Headers = @{
    Authorization  = "Bearer $Token"
    Accept         = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

$Issues = @(
    @{
        title  = "feat(boards): implement boards module"
        body   = Get-Content "$PSScriptRoot\3-boards.md" -Raw
        labels = @("feature", "backend", "frontend")
    },
    @{
        title  = "feat(comments): implement comments module"
        body   = Get-Content "$PSScriptRoot\4-comments.md" -Raw
        labels = @("feature", "backend", "frontend")
    },
    @{
        title  = "feat(attachments): implement attachments module"
        body   = Get-Content "$PSScriptRoot\5-attachments.md" -Raw
        labels = @("feature", "backend", "frontend", "infrastructure")
    },
    @{
        title  = "feat(ai): implement AI assistant module"
        body   = Get-Content "$PSScriptRoot\6-ai-assistant.md" -Raw
        labels = @("feature", "backend", "frontend", "ai")
    }
)

foreach ($issue in $Issues) {
    $Payload = @{
        title  = $issue.title
        body   = $issue.body
        labels = $issue.labels
    } | ConvertTo-Json -Depth 5

    try {
        $Response = Invoke-RestMethod `
            -Uri     "https://api.github.com/repos/$Repo/issues" `
            -Method  POST `
            -Headers $Headers `
            -Body    $Payload `
            -ContentType "application/json"

        Write-Host "Created: #$($Response.number) — $($Response.title)" -ForegroundColor Green
        Write-Host "  $($Response.html_url)"
    }
    catch {
        Write-Host "Failed: $($issue.title)" -ForegroundColor Red
        Write-Host "  $_"
    }
}
