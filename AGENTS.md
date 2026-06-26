# AGENTS.md

## Cursor Cloud specific instructions

This repo is a **.NET 10 + .NET MAUI** personal finance app. Cloud Agents run on **Linux**, which constrains what can be built/run here.

### What runs on Linux (Cloud Agent)
The `net10.0` class libraries and the test project build and run on Linux. This is exactly the CI "quick path" (`.github/workflows/ci-split.yml` → `quick-checks`):

- `Finanzuebersicht.Core`, `Finanzuebersicht.Application`, `Finanzuebersicht.Infrastructure`, `Finanzuebersicht.Presentation` (all `net10.0`)
- `Finanzuebersicht.Tests` (xUnit, `net10.0`) — exercises use cases, repositories, JSON stores, DKB CSV import, etc.

Standard commands (see `README.md` / `.github/copilot-instructions.md`):
- Restore: `dotnet restore Finanzuebersicht.Tests/Finanzuebersicht.Tests.csproj`
- Build + test: `dotnet test Finanzuebersicht.Tests/Finanzuebersicht.Tests.csproj -c Release`

There is no separate lint step: `TreatWarningsAsErrors=true` + `AnalysisLevel=latest` (`Directory.Build.props`) make the build itself the analyzer/lint gate.

### What does NOT run on Linux
The MAUI app project `Finanzuebersicht/Finanzuebersicht.csproj` targets `net10.0-ios`, `net10.0-maccatalyst`, and Windows only. **Do not** try to `dotnet build`/`dotnet run` the MAUI app or the full solution (`Finanzuebersicht.slnx`) on Linux — it requires macOS (Xcode) or Windows plus the `maui` workloads. The GUI app cannot be launched in this environment; the full Mac Catalyst build runs only on macOS CI for PRs to `main`.

### Environment notes
- The .NET 10 SDK is installed at `~/.dotnet` and added to `PATH` via `~/.bashrc`. The update script self-heals by reinstalling it if missing.
- `Nerdbank.GitVersioning` computes the version from git history; the full clone is present, so no extra steps are needed.
