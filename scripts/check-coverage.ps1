param(
    [Parameter(Mandatory)][string]$ResultsDirectory,
    [double]$MinimumLineCoverage = 65,
    [double]$MinimumBranchCoverage = 30
)
$ErrorActionPreference = 'Stop'
$reports = @(Get-ChildItem -LiteralPath $ResultsDirectory -Recurse -Filter coverage.cobertura.xml)
# VSTest's TRX logger may copy the collector attachment into an additional In/ directory.
# Deduplicate identical bytes only; never silently choose between different test runs.
$reports = @($reports | Group-Object { (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash } | ForEach-Object { $_.Group[0] })
if ($reports.Count -ne 1) { throw "Expected one distinct coverage report, found $($reports.Count). Use a fresh results directory." }
[xml]$report = Get-Content -LiteralPath $reports[0].FullName -Raw
$culture = [Globalization.CultureInfo]::InvariantCulture
if ([int]$report.coverage.'lines-valid' -le 0) { throw 'Coverage report contains no instrumented lines.' }
$lines = [double]::Parse($report.coverage.'line-rate', $culture) * 100
$branches = [double]::Parse($report.coverage.'branch-rate', $culture) * 100
$summary = "Line coverage: $($lines.ToString('F2', $culture))% (minimum $MinimumLineCoverage%). Branch coverage: $($branches.ToString('F2', $culture))% (minimum $MinimumBranchCoverage%)."
Write-Output $summary
if ($env:GITHUB_STEP_SUMMARY) { Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Value $summary }
if ($lines -lt $MinimumLineCoverage -or $branches -lt $MinimumBranchCoverage) { throw 'Coverage fell below the required minimum.' }
