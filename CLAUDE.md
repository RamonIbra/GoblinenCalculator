# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

This is a freshly scaffolded ASP.NET Core Razor Pages project (`dotnet new webapp` output, untouched). There is no application logic yet — `Pages/Index.cshtml` still has the default "Welcome" template. The repository has no commits yet. When adding features, expect to be building the "Goblinen Calculator" domain logic from scratch inside this scaffold rather than modifying existing business code.

## Commands

Run all commands from the repository root (`GoblinenCalculator.slnx` is here; the actual project is in `GoblinenCalculator/`).

```
dotnet build                          # build the solution
dotnet run --project GoblinenCalculator   # run the app (http://localhost:5017, https://localhost:7078)
dotnet watch --project GoblinenCalculator run   # run with hot reload
```

There are no test projects in the solution yet. If tests are added, they'll need a new project referenced from `GoblinenCalculator.slnx`.

No linting is configured beyond the default .NET/Roslyn analyzers that ship with the SDK.

## Architecture

- **Framework**: ASP.NET Core Razor Pages, targeting `net10.0`, nullable reference types and implicit usings enabled (`GoblinenCalculator/GoblinenCalculator.csproj`).
- **Entry point**: `GoblinenCalculator/Program.cs` — minimal hosting model. Adds Razor Pages services, configures the standard middleware pipeline (exception handler + HSTS in non-dev, HTTPS redirection, routing, authorization, static assets, Razor Pages).
- **Pages**: `GoblinenCalculator/Pages/*.cshtml` + matching `*.cshtml.cs` code-behind `PageModel` classes, following standard Razor Pages conventions (page routing is inferred from file path via `@page`).
- **Shared layout**: `GoblinenCalculator/Pages/Shared/_Layout.cshtml` is the master layout; `_ViewStart.cshtml` wires it up; `_ViewImports.cshtml` sets the default namespace (`GoblinenCalculator.Pages`) and global tag helpers.
- **Client-side libraries**: Bootstrap, jQuery, and jQuery Validation are vendored under `GoblinenCalculator/wwwroot/lib/` (likely managed via LibMan — check for `libman.json` before manually editing vendored files). Site-specific CSS/JS live in `GoblinenCalculator/wwwroot/css/site.css` and `GoblinenCalculator/wwwroot/js/site.js`.
- **Config**: `appsettings.json` / `appsettings.Development.json` for app configuration; `Properties/launchSettings.json` for local run profiles (ports, environment).
