[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [switch]$OpenReport
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$solution = Join-Path $repositoryRoot 'PrimeFactorization.slnx'
$settings = Join-Path $repositoryRoot 'coverage.runsettings'
$toolManifest = Join-Path $repositoryRoot '.config/dotnet-tools.json'
$coverageDirectory = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts/coverage'))
$resultsDirectory = Join-Path $coverageDirectory 'results'
$reportDirectory = Join-Path $coverageDirectory 'report'
$reportPath = Join-Path $reportDirectory 'index.html'

Push-Location $repositoryRoot
try {
    dotnet tool restore --tool-manifest $toolManifest
    if ($LASTEXITCODE -ne 0) {
        throw "Local tool restore failed with exit code $LASTEXITCODE."
    }

    $repositoryPrefix = $repositoryRoot + [System.IO.Path]::DirectorySeparatorChar
    if (-not $coverageDirectory.StartsWith($repositoryPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw 'The coverage output directory must be inside the repository.'
    }

    if (Test-Path -LiteralPath $coverageDirectory) {
        Remove-Item -LiteralPath $coverageDirectory -Recurse -Force
    }
    New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null

    dotnet test $solution --configuration $Configuration --disable-build-servers `
        --collect 'XPlat Code Coverage' --settings $settings `
        --results-directory $resultsDirectory --logger trx
    if ($LASTEXITCODE -ne 0) {
        throw "Tests or coverage collection failed with exit code $LASTEXITCODE. Report generation stopped."
    }

    # Original collector attachments are one directory deep; TRX may also copy them under In/.
    $coverageReports = @(Get-ChildItem -LiteralPath $resultsDirectory -Directory | ForEach-Object {
        Get-ChildItem -LiteralPath $_.FullName -File -Filter 'coverage.cobertura.xml'
    })
    if ($coverageReports.Count -eq 0) {
        throw 'No Coverlet coverage reports were generated.'
    }

    [xml]$solutionXml = Get-Content -LiteralPath $solution -Raw
    $expectedReportCount = $solutionXml.SelectNodes('//Project[starts-with(@Path, "tests/")]').Count
    if ($coverageReports.Count -ne $expectedReportCount) {
        throw "Expected coverage from $expectedReportCount test projects, but found $($coverageReports.Count) reports."
    }

    $reportInputs = $coverageReports.FullName -join ';'
    dotnet tool run reportgenerator -- "-reports:$reportInputs" "-targetdir:$reportDirectory" `
        '-reporttypes:Html;Cobertura;JsonSummary;TextSummary' '-assemblyfilters:+PrimeFactorization.*;-*.Tests'
    if ($LASTEXITCODE -ne 0) {
        throw "ReportGenerator failed with exit code $LASTEXITCODE."
    }
    if (-not (Test-Path -LiteralPath $reportPath -PathType Leaf)) {
        throw 'ReportGenerator did not create the HTML report.'
    }

    Write-Host "Coverage report: $reportPath"
    Write-Host "Combined Cobertura: $(Join-Path $reportDirectory 'Cobertura.xml')"

    if ($OpenReport) {
        if ([System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT) {
            Start-Process -FilePath $reportPath -WindowStyle Hidden
        }
        else {
            Start-Process -FilePath $reportPath
        }
    }
}
finally {
    Pop-Location
}
