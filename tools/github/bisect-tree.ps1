# Bisect which tree entry makes GitHub's create-tree API reject the request.
# Writes JSON with no BOM (PowerShell's Set-Content -Encoding UTF8 adds one,
# and GitHub answers "Problems parsing JSON" to it).
$ErrorActionPreference = 'Stop'

$token = $env:GITHUB_TOKEN
if (-not $token) { throw 'set GITHUB_TOKEN' }

$root = 'D:\Documents\deepseek-harness\default-workspace\projects\pcl-max-src'
$api = 'https://api.github.com/repos/aaron7357357/PCL-Max'

$exclDirs = @('bin', 'obj', '.git', '.vs', '.idea', 'publish', 'publish-x64',
    'release', 'release-assets', 'SRC-Backups', '.build-out', 'node_modules',
    'TestResults', '.github-release-cache')
$exclExt = @('.mp4', '.pdb', '.user', '.bak', '.prev', '.log', '.tmp', '.cache')

$files = Get-ChildItem $root -Recurse -File -ErrorAction SilentlyContinue | Where-Object {
    $rel = $_.FullName.Substring($root.Length).TrimStart('\')
    $bad = $false
    foreach ($d in $exclDirs) { if (($rel -split '\\') -contains $d) { $bad = $true } }
    (-not $bad) -and ($exclExt -notcontains $_.Extension) -and $_.Length -lt 95MB
}

Write-Output "files: $($files.Count)"

$sha1 = [Security.Cryptography.SHA1]::Create()
$entries = New-Object System.Collections.ArrayList

foreach ($f in $files) {
    $bytes = [IO.File]::ReadAllBytes($f.FullName)
    $hdr = [Text.Encoding]::ASCII.GetBytes("blob $($bytes.Length)`0")
    $buf = New-Object byte[] ($hdr.Length + $bytes.Length)
    [Array]::Copy($hdr, 0, $buf, 0, $hdr.Length)
    [Array]::Copy($bytes, 0, $buf, $hdr.Length, $bytes.Length)
    $h = ($sha1.ComputeHash($buf) | ForEach-Object { $_.ToString('x2') }) -join ''
    [void]$entries.Add([pscustomobject]@{
        path = $f.FullName.Substring($root.Length).TrimStart('\').Replace('\', '/')
        sha  = $h
    })
}

$utf8NoBom = New-Object Text.UTF8Encoding($false)

function Test-Tree {
    param([object[]]$items)
    $arr = foreach ($it in $items) {
        '{"path":"' + ($it.path -replace '\\', '\\\\' -replace '"', '\"') + '","mode":"100644","type":"blob","sha":"' + $it.sha + '"}'
    }
    $body = '{"tree":[' + ($arr -join ',') + ']}'
    $tmp = Join-Path $env:TEMP ('tree-' + [guid]::NewGuid().ToString('N').Substring(0, 8) + '.json')
    [IO.File]::WriteAllText($tmp, $body, $utf8NoBom)
    $resp = & curl.exe -s -o "$tmp.out" -w '%{http_code}' -X POST `
        -H "Authorization: token $token" -H 'User-Agent: p' -H 'Content-Type: application/json' `
        --data-binary "@$tmp" "$api/git/trees"
    $out = Get-Content "$tmp.out" -Raw -ErrorAction SilentlyContinue
    Remove-Item $tmp, "$tmp.out" -Force -ErrorAction SilentlyContinue
    return [pscustomobject]@{ code = $resp; body = $out }
}

$all = Test-Tree -items $entries.ToArray()
Write-Output ("all-at-once: HTTP " + $all.code)
if ($all.code -ne '201') {
    Write-Output ('  body: ' + $all.body.Substring(0, [Math]::Min(400, $all.body.Length)))

    # Bisect: repeatedly split the failing set in half
    $bad = $entries.ToArray()
    while ($bad.Count -gt 1) {
        $mid = [int]($bad.Count / 2)
        $left = $bad[0..($mid - 1)]
        $r = Test-Tree -items $left
        if ($r.code -eq '201') {
            $bad = $bad[$mid..($bad.Count - 1)]
            Write-Output ("  left $($left.Count) ok -> suspect right $($bad.Count)")
        }
        else {
            $bad = $left
            Write-Output ("  left $($left.Count) FAILS -> suspect left $($bad.Count)")
        }
    }

    Write-Output '=== culprit ==='
    $bad | Format-List
}
