# Quick Start — Finanzübersicht

A quick reference guide for getting started with Finanzübersicht development.

## Clone & Setup

```bash
git clone https://github.com/tom4711/finanzuebersicht.git
cd finanzuebersicht
dotnet restore
dotnet workload install maui
```

## Build & Run

### Mac Catalyst

```bash
dotnet build -f net10.0-maccatalyst
cp -R "Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/Finanzübersicht.app" "/Applications/Finanzübersicht.app"
open "/Applications/Finanzübersicht.app"
```

One-liner:

```bash
dotnet build -f net10.0-maccatalyst && cp -R Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/Finanzübersicht.app /Applications/ && open /Applications/Finanzübersicht.app
```

### iOS

```bash
dotnet build -f net10.0-ios
```

## Run Tests

```bash
dotnet test Finanzuebersicht.Tests
```

## Project Structure

| Location | Purpose |
|----------|---------|
| `Finanzuebersicht.Core/` | Shared models & services (net10.0) |
| `Finanzuebersicht/` | MAUI app (Views, ViewModels, Converters) |
| `Finanzuebersicht.Tests/` | xUnit tests |

## Key Resources

- **Localization:** `Finanzuebersicht/Resources/Strings/AppResources.resx` (German), `AppResources.de.resx`
- **Colors:** `Finanzuebersicht/Resources/Styles/Colors.xaml`
- **Architecture:** MVVM with CommunityToolkit.Mvvm
- **Data:** JSON via `LocalDataService` (persisted locally)

## Architecture Overview

- **MVVM** using CommunityToolkit.Mvvm source generators
- **DI:** Register all services in `MauiProgram.cs`
- **Localization:** German (default) + English support
- **Data Persistence:** JSON files locally; CloudKit available but not maintained
- **Backup:** ZIP-based with schema versioning and automatic migration (`DataMigrationService`)

## Features

- Dashboard with monthly summary, charts and trend indicator
- Transactions, recurring transactions (Daueraufträge), categories
- Monthly budgets per category with progress tracking
- Savings goals (Sparziele) with progress bar
- Backup & Restore with automatic schema migration
- Accessibility / VoiceOver support
- Dark Mode, German & English UI

## Development Conventions

- **Branch:** Use `feature/*`, `fix/*`, `chore/*` (not `main`)
- **Commits:** Gitmoji + Conventional Commits (see [.github/copilot-instructions.md](../.github/copilot-instructions.md))
- **Code:** Use `ILocalizationService` for text, `IDialogService` for dialogs
- **Monetary values:** Always use `decimal`

## Versioning

Automatic SemVer via **Nerdbank.GitVersioning** (`version.json`).

Check version:

```bash
nbgv get-version
```

## Documentation

For detailed setup and development guidelines, see [docs/GUIDE.md](GUIDE.md).

Additional documentation:
- [Categorization Rules](CATEGORIZATION_RULES.md)
- [Recurring Transactions UI](RECURRING_UI.md)

## Contributing

1. Create feature branch from `main`: `git checkout -b feature/<name>`
2. Make changes, commit using Conventional Commits + Gitmoji
3. Push and create a Pull Request
4. `main` is protected — PRs are required

Happy coding! 🚀
