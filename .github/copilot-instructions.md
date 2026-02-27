# Copilot Instructions – Finanzübersicht

## Project Overview

A personal finance overview app built with **.NET 10 and .NET MAUI**, targeting **iOS and macOS** (Mac Catalyst). Data is persisted via **iCloud / CloudKit**. Architecture follows **MVVM**.

## Build & Run

```bash
# Restore dependencies
dotnet restore

# Build for iOS (device)
dotnet build -f net10.0-ios

# Build for macOS (Mac Catalyst)
dotnet build -f net10.0-maccatalyst

# Run on iOS simulator
dotnet run -f net10.0-ios

# Run on macOS
dotnet run -f net10.0-maccatalyst
```

> Use Xcode's Simulator or a connected device for iOS. Mac Catalyst builds run natively on macOS.

## Architecture

**MVVM** using `CommunityToolkit.Mvvm`:

- `Models/` – Plain data models (e.g., `Transaction`, `Account`, `Category`)
- `ViewModels/` – Inherit from `ObservableObject`; use `[ObservableProperty]` and `[RelayCommand]` source generators
- `Views/` – XAML pages and controls; bind to ViewModels via `BindingContext`
- `Services/` – Business logic and data access (e.g., CloudKit sync service)

Navigation is handled via Shell (`AppShell.xaml`).

## CloudKit / iCloud Persistence

- Data sync goes through a dedicated service in `Services/` implementing an interface (e.g., `IDataService`)
- ViewModels depend on `IDataService` via constructor injection
- Use `CloudKit` via `CommunityToolkit.Maui` or native bindings; avoid direct `NSUbiquitousKeyValueStore` for complex records — use `CKDatabase` / `CKRecord`
- Always handle sync conflicts and offline states gracefully

## Key Conventions

- Use `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`) — avoid manual `INotifyPropertyChanged` boilerplate
- ViewModels are registered in `MauiProgram.cs` via `builder.Services.AddTransient<>()` / `AddSingleton<>()`; pages resolve their ViewModel via DI
- XAML resource dictionaries in `Resources/Styles/` — define colors, brushes, and styles there, not inline
- Monetary values use `decimal`, never `double` or `float`
- All user-facing strings should be culture-aware (use `CultureInfo.CurrentCulture` for formatting currencies/dates)
- Platform-specific code lives in `Platforms/iOS/` or `Platforms/MacCatalyst/` using partial classes or `#if` preprocessor directives

## Project Structure (expected)

```
Finanzübersicht/
├── MauiProgram.cs          # DI setup, app bootstrap
├── AppShell.xaml           # Shell navigation
├── Models/
├── ViewModels/
├── Views/
├── Services/
├── Resources/
│   ├── Styles/
│   └── Images/
└── Platforms/
    ├── iOS/
    └── MacCatalyst/
```
