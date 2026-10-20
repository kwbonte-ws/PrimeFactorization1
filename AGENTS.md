# Repository engineering standards

These standards apply to all work in this repository.

## Architecture

- Follow Robert C. Martin's Clean Architecture and SOLID principles.
- Dependencies must point inward: Console → Application → Domain.
- Keep Domain independent of UI, infrastructure, and external services.
- Favor composition over inheritance.
- Introduce interfaces only when they serve a meaningful architectural or testing purpose.
- Avoid unnecessary abstraction, frameworks, and speculative generalization.

## Development methodology

- Follow strict Red → Green → Refactor TDD for business logic.
- Begin each behavior change with a failing test.
- Execute tests to verify the Red phase and confirm failure for the intended reason.
- Implement the minimum production code necessary to pass.
- Execute tests to verify the Green phase.
- Refactor only while tests are green.
- Execute tests again after refactoring.
- Never modify a test simply to conceal incorrect production behavior.

## Testing conventions

- Use xUnit for test discovery and execution, and prefer AwesomeAssertions for all future test assertions.
- Use AwesomeAssertions instead of FluentAssertions. Prefer the latest stable compatible NuGet package versions, with explicit versions recorded in project files and upgrades validated by builds and tests.
- Prefer collection assertions such as `actual.Should().Equal(expected)` and `actual.Should().BeEmpty()`.
- Capture exception-producing calls in an `Action` and assert with `action.Should().Throw<TException>()`; use `ThrowExactly<TException>()` and `WithParameterName(...)` when exact type and parameter name are part of the test's intent.
- Preserve strict Red → Green → Refactor for behavior changes. Assertion readability refactoring does not require new failing tests; run the complete suite before and after refactoring.
- When validating meaningful new business logic, run `./scripts/coverage.ps1` after the TDD cycles. Review production line and branch coverage separately, report gaps, and keep generated reports out of Git. No minimum coverage percentage is imposed.
- Mutation testing with `./scripts/mutation-test.ps1` is recommended for significant Domain or Application logic changes, after completing TDD cycles; it is not required after every small cycle. Review killed, surviving, timed-out, compile-error, uncovered, and ignored mutants separately. Do not change tests or exclude production mutations just to improve a score, and do not impose a mutation-score gate without explicit approval.

## C# and .NET standards

- Target .NET 10.
- Use modern C# features where they improve readability.
- Enable nullable reference types and implicit usings.
- Treat compiler warnings as errors.
- Prefer expressive naming, focused methods, and readable implementations.
- Avoid unnecessary comments and magic numbers.
- Follow standard .NET formatting conventions.

## Repository hygiene

- Every .NET solution must contain a root .gitignore using the standard dotnet template (`dotnet new gitignore`).
- Exclude bin, obj, and other generated artifacts.
- Keep build and test artifacts out of version control.
- Never commit or push unless explicitly requested.
- Avoid unrelated changes.

## Agent operating rules

- Inspect existing code and project structure before making changes.
- Make only changes relevant to the current assignment.
- Explain significant architectural decisions.
- Report commands executed and actual test/build results.
- Never claim a test passed unless it was executed successfully.
- Stop at explicitly requested checkpoints.
