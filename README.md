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

## Mutation testing

Stryker.NET 5.0.0 is pinned in the existing local tool manifest. It targets .NET
10 and is [Apache-2.0 licensed](https://github.com/stryker-mutator/stryker-net/blob/master/LICENSE).
This workflow uses only local execution and local reports: no global tool,
dashboard upload, paid service, or production package reference is required.

From the repository root, run:

```powershell
./scripts/mutation-test.ps1
./scripts/mutation-test.ps1 -OpenReport
```

To run one layer or invoke it from a CI job:

```powershell
pwsh -NoProfile -File ./scripts/mutation-test.ps1 -Layer Domain
pwsh -NoProfile -File ./scripts/mutation-test.ps1 -Layer Application
```

Windows PowerShell 5.1 is also supported; substitute `powershell` for `pwsh`.
Paths resolve relative to the script, so an absolute script path works from any
working directory. The script restores and checks the local tool, runs the
selected layers sequentially, and replaces only their generated output folders.
It prints each Stryker exit code, continues to collect results for the other
selected layer, and returns the first nonzero Stryker exit code if a run fails.
Tool availability, missing-report, and rebuild failures are also reported as
failures. A final clean rebuild restores ordinary test outputs, including after
Stryker encounters a temporary file lock while restoring a mutated DLL.

Open the generated HTML files directly in your browser:

- `artifacts/mutation/domain/reports/mutation-report.html`
- `artifacts/mutation/application/reports/mutation-report.html`

Each report also has a sibling `mutation-report.json` containing the precise
mutant statuses and source locations. `-OpenReport` opens successful runs after
completion. Without that switch the workflow is noninteractive. For Azure
DevOps, retain `artifacts/mutation/` as a build artifact and use the script's exit
code; no pipeline is configured yet.

### Scope and interpretation

Each layer's test project contains a `stryker-config.json` selecting its direct
production project reference. Only Domain tests evaluate Domain mutations, and
only Application tests evaluate Application mutations; scores are separate, not
combined. The Console project and test source files are outside those production
targets. Build outputs and common generated C# filenames are explicitly excluded.
No handwritten production source or mutation operator is excluded to raise scores.

The configuration selects VSTest, the `Complete` mutation level, two concurrent
test sessions, and local HTML, JSON, and console reporters. These settings were
checked against the installed tool's help and configuration parser.
`--skip-version-check` and `--output` are command-line options in this version,
not JSON configuration keys. Existing Coverlet/ReportGenerator settings are
unchanged. Stryker's default score-break threshold is zero, so low scores do not
fail this workflow; default report colors are informational.

Ordinary coverage measures which code executes. Mutation testing deliberately
changes compiled production code and checks whether tests notice the changes.
High coverage alone does not demonstrate effective assertions, and a high
mutation score is evidence for the generated mutations rather than proof of
correctness for every possible defect.

- **Killed:** tests detected the mutation by failing.
- **Survived:** covered mutation completed without a failing test; investigate missing assertions or cases, and consider whether it is behaviorally equivalent.
- **Timeout:** the mutated test run exceeded Stryker's time allowance; investigate loops and slow mutants separately from assertion failures.
- **CompileError:** the mutated program could not compile; excluded from the score.
- **NoCoverage:** no selected test executed the mutation; counted as undetected.
- **Ignored:** Stryker excluded the mutant from evaluation; inspect its reason. The project's configuration does not disable mutation operators.

Stryker's score is `(Killed + Timeout) / (Killed + Timeout + Survived + NoCoverage)`.
Ignored and compilation-error mutants are excluded. Always inspect the counts
alongside the percentage, especially when there are few mutants or many timeouts.
Use the JSON statuses for exact counts; the installed version's console table
can group ignored mutants into its surviving column.

Initial analysis generated 17 Domain mutants: 10 killed, 3 timed out, and 4
ignored by Stryker's built-in block-already-covered filter. Application generated
one mutant, removing the `Execute` body, which its tests killed. Neither layer
had surviving, uncovered, or compilation-error mutants. Both scores were 100%,
with the qualification that Domain includes timeout detections and Application
has a very small mutation surface. The timeout mutations affect the Domain's
factor-search division, divisor increment, and factor-reduction assignment.
They suggest reviewing termination behavior rather than inventing assertion gaps
from nonexistent survivors. No production logic or test assertions were changed.

Mutation testing rebuilds projects and repeatedly runs tests, so it is slower
than ordinary tests. Initial local runs took roughly 34 seconds for Domain and
11 seconds for Application, plus tool restore and final rebuild time. Execution
time varies with hardware, package caches, test suite growth, and timeout
mutations. Use it after significant logic changes or in a dedicated CI job,
rather than after every small Red → Green → Refactor cycle.
