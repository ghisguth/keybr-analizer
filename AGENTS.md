# Project Overview

This is a .NET console application designed to analyze and report on typing data exported from Keybr.com. The main project is `KeybrAnalyzer`, and it has a corresponding test project `KeybrAnalyzer.Tests`.

## Main Technologies

* **.NET 10.0** and **C#**
* **xUnit** for testing
* **Shouldly**, **NSubstitute**, and **AutoFixture** for fluent assertions, mocking, and test data generation
* **System.Text.Json** for processing Keybr data
* **Microsoft.Extensions.Hosting** for application setup (logging, configuration, DI)
* **Analyzers**: StyleCop, AsyncFixer, and .NET Code Quality Analyzers for code style and quality enforcement

## Architecture

* The main application logic resides in the `KeybrAnalyzer` project.
* `KeybrAnalyzerService` is a `BackgroundService` that handles the file discovery, data deserialization, and report generation.
* The reporting logic is decoupled into several specialized services:
    * `SummaryReportingService`: Handles high-level statistics and milestones.
    * `PerformanceReportingService`: Detailed key-by-key analysis and target identification.
    * `ProgressReportingService`: Historical progress tracking and improvement analysis.
    * `KeyboardLayoutReportingService`: Visualizes training progress on a keyboard map.
    * `FatigueReportingService`: Monitors for signs of fatigue and diminishing returns.
    * `ReportOrchestrator`: Coordinates the overall report generation process.
* `KeybrAnalysisService` provides the core analytical logic for sessions and histograms.
* The project uses central package management via `Directory.Packages.props`.

-----

# Guiding Principles

*   **Source of Truth:** If there is a discrepancy between this document and the existing codebase, **always ask for clarification** before proceeding. Do not assume one is more correct than the other. The user will specify whether the code should be updated to match the document or vice-versa.

-----

# Building and Running

## Building the project

```bash
dotnet build
```

## Running the project

```bash
dotnet run --project src/KeybrAnalyzer/KeybrAnalyzer.csproj
```

## Running the tests

This project uses **Microsoft.Testing.Platform** (MTP) as the test runner, configured via `global.json`.

```bash
dotnet test
```

### Common Arguments

Since this project uses MTP directly, standard VSTest arguments (like `--filter` or `--logger`) **do not work**. Use the following MTP-specific arguments instead:

*   **List Tests:** `dotnet test --list-tests`
*   **Filtering (xUnit):** Use specific filter flags instead of generic expressions.
    *   By Class: `dotnet test --filter-class "Namespace.ClassName"`
    *   By Method: `dotnet test --filter-method "Namespace.ClassName.MethodName"`
    *   By Namespace: `dotnet test --filter-namespace "Namespace"`
*   **Reporting:** Use `--report-*` flags.
    *   TRX Report: `dotnet test --report-trx` (Output is standard TRX format)
*   **Help:** `dotnet test --help` (Shows all available platform and extension options)

-----

# Pre-Commit Checklist

Before every commit, ensure you have completed the following steps:

*   [ ] Does the code build successfully (`dotnet build`)?
*   [ ] Do all tests pass (`dotnet test`)?
*   [ ] Has the code been formatted (`dotnet format KeybrAnalyzer.slnx`)?
*   [ ] Have you reviewed the `git diff --staged` for accuracy?
*   [ ] Does your commit message conform to the project's style (check `git log -n 5`)?

-----

# Development Conventions

The project uses `.editorconfig` to define and maintain consistent coding styles. All code changes **must** adhere to these styles.

## Code Formatting

To ensure consistent code style, reformat your code before each commit.

1.  Run `dotnet format` against the solution file:

    ```bash
    dotnet format KeybrAnalyzer.slnx
    ```

If you see any StyleCop (SA*) or other analyzer errors after formatting, the agent should first try to fix them by using `dotnet format`. If that doesn't work, please fix them manually.

## Commit Messages

This project follows the conventions established in its commit history. Before committing, review the output of `git log -n 5` to match the style (e.g., tense, subject line format, use of prefixes).

## Tool Usage Guidelines

* **`replace` Tool:** When using the `replace` tool, if an edit command fails multiple times even after re-reading the file, try to make multiple smaller, atomic changes instead of one large change. This increases the likelihood of successful replacement.

## General File Rules

* **Line Endings:** Use Linux-style line endings (`lf`) for C# files.
* **Final Newline:** Every file must end with exactly one newline character. This prevents StyleCop error `SA1518`.
* **Trailing Whitespace:** Trailing whitespace must be trimmed from all lines.
* **Max Line Length:** The maximum line length is **240 characters**.
* **Indentation:** Use **Tabs** for indentation, with a tab width and indent size of **4**. For YAML, JSON, and XML files, use 2 spaces.

## Naming Conventions

All naming rules are enforced as `suggestion` severity in the editor.

| Entity | Style | Example | `.editorconfig` Rule |
| :--- | :--- | :--- | :--- |
| **General** (Classes, Methods, Enums, etc.) | **PascalCase** | `public class KeybrAnalyzerService` | `default_should_be_pascal_case`  |
| **Interfaces** | **IPascalCase** | `public interface IService` | `interfaces_should_start_with_i`  |
| **Type Parameters** | **TPascalCase** | `public class Cache<TItem>` | `type_parameters_should_start_with_t`  |
| **Properties** | **PascalCase** | `public string FullName { get; set; }` | `properties_should_be_pascal_case`  |
| **Public/Internal Fields** | **PascalCase** | `public string PublicData;` | `public_and_internal_fields_should_be_pascal_case`  |
| **Static Fields** | **PascalCase** | `public static int MaxRetries;` | `static_fields_should_be_pascal_case_style`  |
| **Constant Fields** | **PascalCase** | `public const int DefaultTimeout = 5;` | `constant_fields_should_be_pascal_case`  |
| **Private/Protected Fields** | **_camelCase** | `private readonly int _retries;` | `private_and_protected_fields_should_be_camel_case_underscore`  |
| **Locals and Parameters** | **camelCase** | `void Method(string parameterName)` | `locals_and_parameters_should_be_camel_case`  |

## C# Language and Syntax Style

### Modern C# Features

* **File-Scoped Namespaces:** Use file-scoped namespaces to reduce nesting (`IDE0160`).
* **Target-Typed `new` Expressions:** Use `new()` when the type is apparent (`csharp_style_implicit_object_creation_when_type_is_apparent`).
* **Null Argument Checking:** Use `ArgumentNullException.ThrowIfNull()` for guard clauses.
* **Switch Expressions:** Prefer switch expressions over switch statements where possible (`IDE0066`).
* **Primary Constructors:** Prefer primary constructors for dependency injection and initialization to reduce boilerplate code.

### Code Structure and Clarity

* **`using` Directives:** Must be placed **outside the namespace** declaration (`IDE0065`).
* **Accessibility Modifiers:** **Always** specify accessibility modifiers explicitly (`IDE0040`).
* **Braces:** Braces `{}` are **required** for all blocks, even single-line `if` statements (`csharp_prefer_braces`). The opening brace should be on a new line (`csharp_new_line_before_open_brace`).
* **Keywords vs. BCL Types:** Prefer language keywords over BCL types (e.g., use `int` instead of `System.Int32`) (`IDE0049`).
* **Regions:** **Do not use regions** (`#region`). This is enforced by StyleCop rule `SA1124`.

## Testing Conventions

### Naming and Async Conventions

* **Async Methods:** All asynchronous test methods **must** be suffixed with `Async`.
* **Test Method Naming:** Test names **must not** contain underscores (`_`). Use PascalCase to separate the method under test from the condition and expected outcome.
* **`async` Keyword:** In tests, it's better to use `async`/`await` for readability, even if the method body doesn't strictly require it.
* **Synchronous Waits:** Avoid synchronous waits like `Task.Wait()` in tests.

### xUnit Conventions

* **DO NOT** use `Xunit.Abstractions`. This assembly is for xUnit v2. For xUnit v3, simply use `using Xunit;`.
* If you see `using Xunit.Abstractions;` in any code, remove it.
