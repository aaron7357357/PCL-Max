# Retry pushing source to GitHub until the secondary rate limit clears.
# ASCII-only on purpose: PowerShell 5.1 misparses non-BOM UTF-8 scripts.
# The token is read from the environment: embedding it here trips GitHub's
# secret scanning, which rejects the push with a 422 repository-rule violation.
$ErrorActionPreference = 'Continue'

$exe = 'D:\Documents\deepseek-harness\default-workspace\projects\pcl-max-src\tools\github\GitPush\bin\Release\net10.0\GitPush.exe'
$src = 'D:\Documents\deepseek-harness\default-workspace\projects\pcl-max-src'

if (-not $env:GITHUB_TOKEN) {
    Write-Output 'ERROR: set $env:GITHUB_TOKEN before running this script'
    exit 2
}

$msg = "src: migrate to PCL_Plus 4.0.1 source base (v0.3.0-Beta1)`n`n" +
       "- remove Lunar Client / GameManage / About / Feedback / Log`n" +
       "- home page slimmed to performance card`n" +
       "- new top-level Link page (wifi icon + Terracotta panel)`n" +
       "- WebP detection via WIC / libwebp.dll`n" +
       "- own M icon`n" +
       "- fix update source: ProjectLinks -> aaron7357357/PCL-Max"

for ($round = 1; $round -le 40; $round++) {
    Write-Output ''
    Write-Output ('=== attempt ' + $round + ' at ' + (Get-Date -Format 'HH:mm:ss') + ' ===')

    $out = & $exe --src $src --repo 'aaron7357357/PCL-Max' --branch main --message $msg --token $env:GITHUB_TOKEN 2>&1
    $code = $LASTEXITCODE
    $out | ForEach-Object { '    ' + $_ }

    if ($code -eq 0) {
        Write-Output 'DONE: push succeeded'
        exit 0
    }

    $joined = ($out | Out-String)
    if ($joined -notmatch 'rate limit|403|Forbidden') {
        Write-Output 'STOP: not a rate-limit failure'
        exit 1
    }

    Write-Output '    rate limited, retrying in 180s ...'
    Start-Sleep -Seconds 180
}

Write-Output 'GAVE UP after 40 attempts'
exit 1
