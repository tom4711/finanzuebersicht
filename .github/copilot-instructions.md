# Copilot Instructions – Finanzübersicht

## Project Overview

A personal finance overview app built with **.NET 10 and .NET MAUI**, targeting **iOS and macOS** (Mac Catalyst). Data is persisted locally via JSON files (`LocalDataService`); CloudKit support requires a paid Apple Developer account. Architecture follows **MVVM**. Language: German only.

## Build & Run

```bash
dotnet restore

# Build for macOS (Mac Catalyst)
dotnet build -f net10.0-maccatalyst

# Build for iOS
dotnet build -f net10.0-ios

# Run tests
dotnet test Finanzuebersicht.Tests
```

> Xcode version validation is disabled in the `.csproj` (`ValidateXcodeVersion=false`) due to SDK/Xcode version mismatch.

### Running on macOS (Mac Catalyst)

`dotnet run` and `-t:Run` fail due to macOS sandboxing. To launch the app, copy it to `/Applications` first:

```bash
cp -R Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/Finanzübersicht.app /Applications/
open /Applications/Finanzübersicht.app
```

For a one-liner after every build:

```bash
dotnet build -f net10.0-maccatalyst && cp -R Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/Finanzübersicht.app /Applications/ && open /Applications/Finanzübersicht.app
```

## Architecture

**MVVM** using `CommunityToolkit.Mvvm` source generators:

- `Finanzuebersicht.Core/` – Shared .NET 10 class library (models, services, interfaces)
- `Finanzuebersicht/` – .NET MAUI app (ViewModels, Views, Converters, platform code)
- `Finanzuebersicht.Tests/` – xUnit test project (references Core only)

Navigation: Shell with 5 tabs (Dashboard, Transaktionen, Daueraufträge, Kategorien, Einstellungen) + 3 detail pages via Shell routing.

## Data Persistence

- All CRUD goes through `IDataService` → `LocalDataService` (Singleton, JSON files)
- Storage path configurable via `SettingsService` ("DataPath" key), default: `LocalApplicationData/Finanzuebersicht`
- CloudKit code exists (`CloudKitDataService`) but is disabled (requires paid Apple Developer account)
- Recurring transaction auto-generation runs on `App.OnStart()` and `Window.Resumed`

## Key Conventions

- `CommunityToolkit.Mvvm` source generators everywhere — no manual `INotifyPropertyChanged`
- All services and ViewModels registered in `MauiProgram.cs` via DI (`AddSingleton` / `AddTransient`)
- Pages receive their ViewModel via constructor injection
- `OnAppearing()` triggers data loading via command execution
- Monetary values: `decimal` in C#, formatted with `CultureInfo.CurrentCulture`
- Use `Border` with `StrokeShape="RoundRectangle"` instead of deprecated `Frame`
- Colors defined in `Resources/Styles/Colors.xaml` (Apple System Colors palette)
- Light/Dark mode via `AppThemeBinding` — avoid hardcoded color values in XAML
- German-only UI — no localization infrastructure

## Versioning

Automatic **SemVer** via **Nerdbank.GitVersioning** (`version.json` in repo root):

- Version = `<major>.<minor>.<git-height>` (e.g., `0.1.5` = 5 commits since 0.1.0)
- MAUI `ApplicationDisplayVersion` and `ApplicationVersion` are set automatically at build time
- **Bump version:** edit `version.json` or run `nbgv set-version <new-version>`
- **Current version:** run `nbgv get-version`
- `publicReleaseRefSpec`: `main` and `release/v*` branches produce stable versions
- Cloud build numbers enabled for CI pipelines

## Git Branching Strategy

**`main` is protected** — no direct commits to `main`.

All changes go through feature/fix branches and are merged via Pull Request:

```
main              ← stable, release-ready
├── feature/*     ← new features (e.g., feature/cloud-sync)
├── fix/*         ← bug fixes (e.g., fix/dark-mode-contrast)
├── chore/*       ← tooling, deps, config (e.g., chore/update-packages)
└── release/v*    ← release preparation (e.g., release/v1.0)
```

**Workflow:**
1. Create branch from `main`: `git checkout -b feature/<name>`
2. Make changes, commit following conventions below
3. Push branch and create Pull Request
4. Merge to `main` (squash or merge commit)

**Branch naming:** `<type>/<short-description>` in kebab-case (English)

## Project Structure

```
Finanzuebersicht.slnx              ← Solution file
version.json                       ← Nerdbank.GitVersioning config
Directory.Build.props              ← Shared MSBuild properties (nbgv package)

Finanzuebersicht.Core/             ← Shared library (net10.0)
├── Models/                        ← Transaction, Category, RecurringTransaction, etc.
└── Services/                      ← IDataService, LocalDataService, SettingsService, InitializationService

Finanzuebersicht/                  ← MAUI app (net10.0-ios, net10.0-maccatalyst)
├── MauiProgram.cs
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── ViewModels/                    ← DashboardVM, TransactionsVM, SettingsVM, etc.
├── Views/                         ← XAML pages
├── Converters/                    ← Value converters for UI
├── Handlers/                      ← Custom Shell renderer (FastShellRenderer)
├── Resources/Styles/              ← Colors.xaml, Styles.xaml
└── Platforms/                     ← iOS, MacCatalyst

Finanzuebersicht.Tests/            ← xUnit tests (net10.0, references Core only)
└── Services/                      ← LocalDataService, InitializationService tests
```

## Git Commit Conventions

**Format:** Gitmoji + Conventional Commits (English)

```
<emoji> <type>(<scope>): <short summary>

<body – what changed and why>

<footer>
```

**Types & Gitmoji:**

| Emoji | Type | Usage |
|-------|------|-------|
| ✨ | `feat` | New feature or functionality |
| 🐛 | `fix` | Bug fix |
| ♻️ | `refactor` | Code restructuring without behavior change |
| 💄 | `style` | UI/UX changes, styling, design |
| 🧪 | `test` | Adding or updating tests |
| 📝 | `docs` | Documentation changes |
| 🔧 | `chore` | Build config, dependencies, tooling |
| ⚡ | `perf` | Performance improvement |
| 🗑️ | `remove` | Removing code or files |
| 🚀 | `deploy` | Deployment-related changes |
| 🏗️ | `arch` | Architecture changes (project structure) |

**Scopes:** `core`, `ui`, `viewmodel`, `service`, `model`, `converter`, `test`, `config`, `shell`, `settings`

**Rules:**
- Subject line: imperative mood, max 72 chars, no period at end
- Body: wrap at 80 chars, explain *what* and *why* (not *how*)
- Always list affected files/components in the body
- Breaking changes: add `BREAKING CHANGE:` in footer
- Always include `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` trailer

**Examples:**

```
✨ feat(service): add configurable data path for LocalDataService

- LocalDataService now accepts SettingsService to read custom data path
- Users can choose iCloud Drive folder for automatic backup
- Falls back to LocalApplicationData when no custom path is set

Affected: LocalDataService.cs, SettingsService.cs, MauiProgram.cs

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

```
🐛 fix(converter): handle null values in BetragDisplayConverter

- Return "0,00 €" instead of throwing NullReferenceException
- Added null check for decimal input parameter

Affected: BetragDisplayConverter.cs

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```
