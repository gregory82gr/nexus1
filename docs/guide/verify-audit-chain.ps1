<#
.SYNOPSIS
  Independently verify RootCause's tamper-evident audit chain (H10, ADR-032).

.DESCRIPTION
  Reads every row of RootCause.Audit in order and checks, with no project code involved:
    1. each row's PrevHash equals the previous row's Hash (the first row's PrevHash is the
       64-zero genesis hash), and
    2. each row's Hash equals lowercase-hex SHA-256( UTF-8( PrevHash + Payload ) ) --
       recomputed here from the raw stored text, the same formula as
       Sha256AuditChainWriter.ComputeHash.
  Optionally compares the newest row's Hash with an auditHash you copied from a
  diagnosis HTTP response (-ExpectedLatestHash).

  Uses only Windows PowerShell 5.1's built-in System.Data.SqlClient and your own Windows
  login (sysadmin on LocalDB) -- no modules, no project assemblies, no SQL-login secrets.

.EXAMPLE
  .\docs\guide\verify-audit-chain.ps1
.EXAMPLE
  .\docs\guide\verify-audit-chain.ps1 -ExpectedLatestHash 3f0986425052060bcd0b8b3f45094295cc7ec7802e5b65fea399a6261b9c2a81
#>
param(
    [string]$Server = "(localdb)\mssqllocaldb",
    [string]$Database = "RootCauseDb",
    [string]$ExpectedLatestHash
)

$genesis = "0" * 64
$sha = [System.Security.Cryptography.SHA256]::Create()
function Get-ChainHash([string]$prev, [string]$payload) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($prev + $payload)
    ($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString("x2") }) -join ""
}

$connection = New-Object System.Data.SqlClient.SqlConnection("Server=$Server;Database=$Database;Integrated Security=True;")
$connection.Open()
$command = $connection.CreateCommand()
$command.CommandText = "SELECT Seq, TimestampUtc, PrevHash, Hash, Payload FROM RootCause.Audit ORDER BY Seq"
$reader = $command.ExecuteReader()

$expectedPrev = $genesis
$rows = 0; $broken = 0; $last = $null
while ($reader.Read()) {
    $rows++
    $seq = $reader.GetInt64(0); $prev = $reader.GetString(2).Trim(); $hash = $reader.GetString(3).Trim(); $payload = $reader.GetString(4)
    $recomputed = Get-ChainHash $prev $payload
    $linkOk = $prev -eq $expectedPrev
    $hashOk = $recomputed -eq $hash
    if (-not ($linkOk -and $hashOk)) {
        $broken++
        Write-Host ("  BROKEN at Seq {0}: link {1}, hash {2}" -f $seq, $(if ($linkOk) { "ok" } else { "MISMATCH" }), $(if ($hashOk) { "ok" } else { "MISMATCH (stored $hash, recomputed $recomputed)" })) -ForegroundColor Red
    }
    $expectedPrev = $hash
    $last = [pscustomobject]@{ Seq = $seq; TimestampUtc = $reader.GetDateTime(1); Hash = $hash; Recomputed = $recomputed; Payload = $payload }
}
$reader.Close(); $connection.Close()

if ($rows -eq 0) { Write-Host "RootCause.Audit is empty -- run a diagnosis first."; exit 1 }

Write-Host ("Rows checked: {0}   broken links/hashes: {1}" -f $rows, $broken)
Write-Host ("Newest row:   Seq {0} at {1:u}" -f $last.Seq, $last.TimestampUtc)
Write-Host ("  stored     Hash = {0}" -f $last.Hash)
Write-Host ("  recomputed Hash = {0}" -f $last.Recomputed)
Write-Host ("  payload (first 160 chars): {0}" -f $last.Payload.Substring(0, [Math]::Min(160, $last.Payload.Length)))
if ($ExpectedLatestHash) {
    $match = $ExpectedLatestHash.Trim().ToLowerInvariant() -eq $last.Hash
    Write-Host ("HTTP auditHash matches newest row: {0}" -f $(if ($match) { "YES" } else { "NO" })) -ForegroundColor $(if ($match) { "Green" } else { "Red" })
    if (-not $match) { exit 2 }
}
if ($broken -gt 0) { Write-Host "CHAIN BROKEN" -ForegroundColor Red; exit 3 }
Write-Host "CHAIN INTACT" -ForegroundColor Green
