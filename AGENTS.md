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
