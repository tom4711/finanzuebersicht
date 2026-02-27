# Copilot Instructions – Finanzübersicht

## Project Overview

A personal finance overview app built with **.NET 10 and .NET MAUI**, targeting **iOS and macOS** (Mac Catalyst). Data is persisted via **iCloud / CloudKit**. Architecture follows **MVVM**. Language: German only.

## Build & Run

```bash
dotnet restore

# Build for macOS (Mac Catalyst)
dotnet build -f net10.0-maccatalyst

# Build for iOS
dotnet build -f net10.0-ios
```

> Xcode version validation is disabled in the `.csproj` (`ValidateXcodeVersion=false`) due to SDK/Xcode version mismatch.

## Architecture

**MVVM** using `CommunityToolkit.Mvvm` source generators:

- `Models/` – `Transaction`, `Category`, `RecurringTransaction`, `TransactionType` enum, helper types (`TransactionGroup`, `KategorieZusammenfassung`)
- `ViewModels/` – Inherit from `ObservableObject`; use `[ObservableProperty]` and `[RelayCommand]`
- `Views/` – XAML pages bound to ViewModels via `BindingContext` (set in code-behind constructor via DI)
- `Services/` – `IDataService` interface with `CloudKitDataService` implementation; `InitializationService` for first-launch setup
- `Converters/` – Value converters for UI (currency display, colors, status text)

Navigation: Shell with 4 tabs (Dashboard, Transaktionen, Daueraufträge, Kategorien) + 3 detail pages via Shell routing.

## CloudKit / iCloud Persistence

- All CRUD goes through `IDataService` → `CloudKitDataService` (Singleton)
- Uses `CKContainer.DefaultContainer.PrivateCloudDatabase`
- CKRecord types: `"Category"`, `"Transaction"`, `"RecurringTransaction"`
- `decimal` values stored as strings in CloudKit (no native decimal type)
- `DateTime` ↔ `NSDate` conversion in mapping helpers
- Recurring transaction auto-generation runs on `App.OnStart()` and `Window.Resumed`

## Key Conventions

- `CommunityToolkit.Mvvm` source generators everywhere — no manual `INotifyPropertyChanged`
- All services and ViewModels registered in `MauiProgram.cs` via DI (`AddSingleton` / `AddTransient`)
- Pages receive their ViewModel via constructor injection
- `OnAppearing()` triggers data loading via command execution
- Monetary values: `decimal` in C#, formatted with `CultureInfo.CurrentCulture`
- Use `Border` with `StrokeShape="RoundRectangle"` instead of deprecated `Frame`
- Colors defined in `Resources/Styles/Colors.xaml` (Apple System Colors palette)
- Light/Dark mode via `AppThemeBinding` in Styles.xaml
- German-only UI — no localization infrastructure

## Project Structure

```
Finanzuebersicht/
├── MauiProgram.cs
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── Models/
│   ├── TransactionType.cs
│   ├── Category.cs
│   ├── Transaction.cs
│   ├── RecurringTransaction.cs
│   ├── TransactionGroup.cs
│   └── KategorieZusammenfassung.cs
├── ViewModels/
│   ├── DashboardViewModel.cs
│   ├── TransactionsViewModel.cs
│   ├── TransactionDetailViewModel.cs
│   ├── RecurringTransactionsViewModel.cs
│   ├── RecurringTransactionDetailViewModel.cs
│   ├── CategoriesViewModel.cs
│   └── CategoryDetailViewModel.cs
├── Views/
│   ├── DashboardPage.xaml
│   ├── TransactionsPage.xaml
│   ├── TransactionDetailPage.xaml
│   ├── RecurringTransactionsPage.xaml
│   ├── RecurringTransactionDetailPage.xaml
│   ├── CategoriesPage.xaml
│   └── CategoryDetailPage.xaml
├── Services/
│   ├── IDataService.cs
│   ├── CloudKitDataService.cs
│   └── InitializationService.cs
├── Converters/
│   ├── TransactionTypToColorConverter.cs
│   ├── BetragDisplayConverter.cs
│   ├── TypButtonColorConverter.cs
│   ├── BoolToOpacityConverter.cs
│   ├── StatusConverters.cs
│   ├── DashboardConverters.cs
│   └── CountToBoolConverter.cs
├── Resources/
│   ├── Styles/Colors.xaml
│   ├── Styles/Styles.xaml
│   └── ...
└── Platforms/
    ├── iOS/
    └── MacCatalyst/
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
