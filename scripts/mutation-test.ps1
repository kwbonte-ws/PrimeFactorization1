[CmdletBinding()]
param(
    [ValidateSet('All', 'Domain', 'Application')]
    [string]$Layer = 'All',

    [switch]$OpenReport
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (Test-Path variable:PSNativeCommandUseErrorActionPreference) {
    $PSNativeCommandUseErrorActionPreference = $false
}

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolManifest = Join-Path $repositoryRoot '.config/dotnet-tools.json'
$solution = Join-Path $repositoryRoot 'PrimeFactorization.slnx'
$outputRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts/mutation'))
$layers = if ($Layer -eq 'All') { @('Domain', 'Application') } else { @($Layer) }
$exitCode = 0
$mutationStarted = $false
$reports = @()

Push-Location $repositoryRoot
try {
    dotnet tool restore --tool-manifest $toolManifest
    $toolExitCode = $LASTEXITCODE
    if ($toolExitCode -ne 0) {
        Write-Error "Local tool restore failed with exit code $toolExitCode." -ErrorAction Continue
        exit $toolExitCode
    }

    dotnet tool run dotnet-stryker -- --help | Out-Null
    $toolExitCode = $LASTEXITCODE
    if ($toolExitCode -ne 0) {
        Write-Error "The local Stryker.NET tool is unavailable (exit code $toolExitCode)." -ErrorAction Continue
        exit $toolExitCode
    }

    foreach ($currentLayer in $layers) {
        $testDirectory = Join-Path $repositoryRoot "tests/PrimeFactorization.$currentLayer.Tests"
        $outputDirectory = [System.IO.Path]::GetFullPath((Join-Path $outputRoot $currentLayer.ToLowerInvariant()))
        $outputPrefix = $outputRoot + [System.IO.Path]::DirectorySeparatorChar
        if (-not $outputDirectory.StartsWith($outputPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            throw 'Mutation output must be inside artifacts/mutation.'
        }
        if (Test-Path -LiteralPath $outputDirectory) {
            Remove-Item -LiteralPath $outputDirectory -Recurse -Force
        }

        Write-Host "Mutation testing: $currentLayer"
        $mutationStarted = $true
        Push-Location $testDirectory
        try {
            dotnet tool run dotnet-stryker -- --config-file stryker-config.json `
                --skip-version-check --output $outputDirectory
            $strykerExitCode = $LASTEXITCODE
        }
        finally {
            Pop-Location
        }

        Write-Host "Stryker $currentLayer exit code: $strykerExitCode"
        if ($strykerExitCode -ne 0) {
            Write-Error "Stryker failed for $currentLayer with exit code $strykerExitCode." -ErrorAction Continue
            if ($exitCode -eq 0) { $exitCode = $strykerExitCode }
        }

        $reportPath = Join-Path $outputDirectory 'reports/mutation-report.html'
        if (Test-Path -LiteralPath $reportPath -PathType Leaf) {
            $reports += $reportPath
        }
        elseif ($strykerExitCode -eq 0) {
            Write-Error "Stryker did not generate an HTML report for $currentLayer." -ErrorAction Continue
            if ($exitCode -eq 0) { $exitCode = 1 }
        }
    }
}
finally {
    try {
        if ($mutationStarted) {
            # Timed-out test hosts can briefly lock mutated test-output DLLs during Stryker cleanup.
            dotnet build $solution --no-incremental --disable-build-servers
            $buildExitCode = $LASTEXITCODE
            if ($buildExitCode -ne 0) {
                Write-Error "Restoring normal build outputs failed with exit code $buildExitCode." -ErrorAction Continue
                if ($exitCode -eq 0) { $exitCode = $buildExitCode }
            }
        }
    }
    finally {
        Pop-Location
    }
}

foreach ($report in $reports) {
    Write-Host "Mutation report: $report"
    if ($OpenReport -and $exitCode -eq 0) {
        if ([System.Environment]::OSVersion.Platform -eq [System.PlatformID]::Win32NT) {
            Start-Process -FilePath $report -WindowStyle Hidden
        }
        else {
            Start-Process -FilePath $report
        }
    }
}

exit $exitCode
