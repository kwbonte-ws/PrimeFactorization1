# PrimeFactorization

An interactive .NET 10 application with Clean Architecture dependencies:
Console → Application → Domain. Tests use xUnit and AwesomeAssertions.

## Build, test, and run

Install the .NET 10 SDK, then run from the repository root:

```powershell
dotnet build PrimeFactorization.slnx
dotnet test PrimeFactorization.slnx
dotnet run --project src/PrimeFactorization.Console/PrimeFactorization.Console.csproj
```

Enter positive 32-bit integers to calculate their prime factors. Enter `exit` to
quit; redirected input also ends the session at EOF.

## Code coverage

Run with Windows PowerShell 5.1 or PowerShell 7+ and the .NET 10 SDK:

```powershell
./scripts/coverage.ps1
```

To open the generated HTML report in your browser:

```powershell
./scripts/coverage.ps1 -OpenReport
```

To collect Release coverage, including on a future CI agent:

```powershell
pwsh -NoProfile -File ./scripts/coverage.ps1 -Configuration Release
```

The script resolves paths relative to itself, so it can be invoked from any
working directory. It restores the pinned repository-local ReportGenerator tool,
replaces only `artifacts/coverage`, builds and runs every solution test project
with coverage enabled, and merges the current run's reports. Tool restore,
build/test, missing coverage, and report-generation failures stop the script with
an error. Reporting does not proceed after failed tests. No browser is opened
unless `-OpenReport` is supplied.

Generated files:

- `artifacts/coverage/report/index.html`: combined, browsable production coverage.
- `artifacts/coverage/report/Cobertura.xml`: combined coverage for CI publishing.
- `artifacts/coverage/report/Summary.json` and `Summary.txt`: machine-readable and text summaries, including line and branch coverage.
- `artifacts/coverage/results/`: individual Cobertura reports and TRX test results.

The standard root `.gitignore` already excludes `artifacts/`, `bin/`, `obj/`, and
test results. Keep the tool manifest and coverage settings in version control;
keep generated reports out of it. No minimum coverage percentage is enforced.

### Runner and exclusions

These projects use **VSTest**, through `Microsoft.NET.Test.Sdk` and
`xunit.runner.visualstudio`, rather than Microsoft.Testing.Platform.
`coverlet.collector` 10.1.0 is installed privately in all three test projects.
Its documented VSTest integration requires Test SDK 18.4.0 or newer, so the test
projects use 18.11.0. Production projects have no coverage dependencies.

`coverage.runsettings` collects only `PrimeFactorization.*` production assemblies
and excludes test assemblies, files under `obj`, common generated C# filenames,
and code marked `GeneratedCode` or `ExcludeFromCodeCoverage`. Handwritten
`Program.cs` remains eligible: the Console session tests do not execute its
composition-root entry point, so its uncovered lines are reported honestly.

ReportGenerator merges overlapping coverage from Domain, Application, and Console
tests. Review line and branch coverage separately for each production assembly;
a method without branches has no applicable branch coverage, rather than a
missing test result.

### Free tooling and future Azure DevOps integration

The workflow uses [Coverlet (MIT)](https://github.com/coverlet-coverage/coverlet)
and the free [ReportGenerator (Apache-2.0)](https://github.com/danielpalme/ReportGenerator)
HTML, Cobertura, JSON summary, and text summary report formats. ReportGenerator
5.5.11 is pinned in `.config/dotnet-tools.json` and restored automatically;
no global tool installation, license key, PRO feature, or paid service is used.
The tool manifest permits runtime roll-forward so a .NET 10 installation can run
the tool without requiring a separate older runtime.

A future Azure DevOps job can run the same script without `-OpenReport`, publish
`artifacts/coverage/results/**/*.trx` as test results, publish
`artifacts/coverage/report/Cobertura.xml` as coverage, and retain the HTML report
directory as a build artifact. No pipeline or coverage threshold is imposed yet.
