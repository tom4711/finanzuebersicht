# Entwickler-Leitfaden — Finanzübersicht

Dieser Leitfaden hilft beim lokalen Aufbau, Testen und Entwickeln der App.

## 1. Repository klonen

```bash
git clone https://github.com/tom4711/finanzuebersicht.git
cd finanzuebersicht
dotnet restore
```

## 2. Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- **macOS:** Xcode für iOS/Mac Catalyst Builds
- **Empfohlen:** `dotnet workload install maui`

Workloads installieren (macOS):

```bash
dotnet workload install maui
```

## 3. Build & Run

### Mac Catalyst (lokal starten)

```bash
dotnet build Finanzuebersicht/Finanzuebersicht.csproj -f net10.0-maccatalyst
cp -R "Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/Finanzübersicht.app" "/Applications/Finanzübersicht.app"
open "/Applications/Finanzübersicht.app"
```

**Ein-Liner nach jedem Build:**

```bash
dotnet build -f net10.0-maccatalyst && cp -R Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/Finanzübersicht.app /Applications/ && open /Applications/Finanzübersicht.app
```

### iOS (Simulator)

```bash
dotnet build -f net10.0-ios
```

## 4. Tests

```bash
dotnet test Finanzuebersicht.Tests
```

## 5. Wichtige Pfade & Ressourcen

| Element | Pfad |
|---------|------|
| Lokale Datenspeicherung | Einstellung `DataPath` (Settings) |
| Kategorisierungs-Regeln | `Finanzuebersicht.Core/Data/categorization-rules.json` → siehe [docs/CATEGORIZATION_RULES.md](CATEGORIZATION_RULES.md) |
| UI-Texte (Deutsch) | `Finanzuebersicht/Resources/Strings/AppResources.resx` |
| UI-Texte (Englisch) | `Finanzuebersicht/Resources/Strings/AppResources.en.resx` |
| Farbdefinitionen | `Finanzuebersicht/Resources/Styles/Colors.xaml` |

## 6. Projektstruktur

```
Finanzuebersicht.slnx              ← Solution file
version.json                       ← Nerdbank.GitVersioning config
Directory.Build.props              ← Shared MSBuild properties

Finanzuebersicht.Core/             ← Shared library (net10.0)
├── Models/                        ← Transaction, Category, RecurringTransaction, CategoryBudget, SparZiel, etc.
└── Services/                      ← IDataService, LocalDataService, SettingsService, BackupService, DataMigrationService

Finanzuebersicht.Application/      ← Use Cases / Application Layer (net10.0)
Finanzuebersicht.Infrastructure/   ← DI-Registrierung, Infrastrukturdienste (net10.0)

Finanzuebersicht/                  ← MAUI app (net10.0-ios, net10.0-maccatalyst)
├── MauiProgram.cs
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── ViewModels/                    ← DashboardVM, TransactionsVM, SettingsVM, etc.
├── Views/                         ← XAML pages
├── Converters/                    ← Value converters for UI
├── Resources/Strings/             ← AppResources.resx (+ .de.resx)
├── Resources/Styles/              ← Colors.xaml, Styles.xaml
└── Platforms/                     ← iOS, MacCatalyst

Finanzuebersicht.Tests/            ← xUnit tests (net10.0)
└── Services/                      ← BackupService, DataMigrationService, InitializationService tests
```

## 7. Entwicklungs-Konventionen

### MVVM-Architektur

- **Framework:** CommunityToolkit.Mvvm mit Source Generators (kein manuelles `INotifyPropertyChanged`)
- **DI:** Alle Services und ViewModels in `MauiProgram.cs` registriert
- **Pages:** Erhalten ViewModel via Constructor Injection
- **Data Loading:** Triggern via Command in `OnAppearing()`

### Code-Stil

- **Dezimalzahlen:** `decimal` für Geldbeträge, formatiert mit `CultureInfo.CurrentCulture`
- **UI-Elemente:** `Border` mit `StrokeShape="RoundRectangle"` statt deprecated `Frame`
- **Farben:** Über `Colors.xaml` (Apple System Colors), nutze `AppThemeBinding` für Light/Dark Mode
- **Texte:** `ILocalizationService` oder ResX in ViewModels verwenden
- **Dialoge:** `IDialogService` für alle Benutzerdialoge

### Git-Workflow

- **Branch:** Arbeite auf `feature/*`, `fix/*` oder `chore/*` – Basis ist immer `develop`
- **Commit:** Gitmoji + Conventional Commits (siehe [.github/copilot-instructions.md](../.github/copilot-instructions.md))
- **PR:** Erstelle Pull Request gegen `develop`; `main` ist geschützt und nur für Releases

**Branch-Strategie:**

```
feature/* → develop  (PR, für laufende Entwicklung)
develop   → main     (PR, wenn ein Milestone abgeschlossen ist)
main      → v1.x-Tag (löst release.yml aus)
```

Beispiel-Commit:

```
✨ feat(viewmodel): add recurring transaction filter

- Add FilterRecurringTransactions method to DashboardViewModel
- Users can now filter by category and date range
- Include unit tests for new filter logic

Affected: DashboardViewModel.cs, DashboardViewModel.Tests.cs, DashboardPage.xaml.cs

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

## 8. Datenspeicherung

- **Lokal:** JSON-Dateien via `LocalDataService` (Standard, keine externe Abhängigkeit)
- **Pfad:** Standardmäßig `LocalApplicationData/Finanzuebersicht`, über `SettingsService` anpassbar
- **CloudKit:** Code vorhanden (`CloudKitDataService`), aber deaktiviert (erfordert kostenpflichtiges Apple Developer Account)
- **Daueraufträge:** Automatische Generierung läuft auf `App.OnStart()` und `Window.Resumed`

## 9. Backup & Restore

Nutzer können in den Einstellungen:
- ✅ Backup erstellen (ZIP-Export mit allen Daten inkl. Budgets & Sparziele)
- ✅ Backup wiederherstellen mit automatischer Schema-Migration
- ✅ CSV-Import durchführen (mit Auto-Kategorisierung)

### Schema-Versionierung

Backups sind versioniert (`SchemaVersion` in den Metadaten). Der `DataMigrationService` migriert ältere Backups beim Restore automatisch auf die aktuelle Version:

| Version | Inhalt |
|---------|--------|
| v1 | categories, transactions, recurring |
| v2 | + budgets, sparziele |

Neue Migratoren können als `IDataMigrator`-Implementierungen in DI registriert werden.

## 10. Versionierung

- **System:** Nerdbank.GitVersioning (`version.json`)
- **Format:** `<major>.<minor>.<git-height>` (z.B. `0.1.5` = 5 Commits seit 0.1.0)
- **MAUI-Version:** Automatisch gesetzt via `ApplicationDisplayVersion` und `ApplicationVersion` zur Buildzeit
- **Release-Branches:** `main` und `release/v*` produzieren Stable-Versionen

**Aktuellen Version abfragen:**

```bash
nbgv get-version
```

**Version bumpen:**

```bash
nbgv set-version <new-version>
```

## 11. CI/CD

- **Quick Checks:** Unit Tests auf Ubuntu (schnell, günstig) — bei jedem Push auf `develop`, `main`, `feature/*` und PRs
- **Full MAUI Build:** Nur für PRs gegen `main` und `main`-Pushes (macOS-Runner)
- **Pre-Release:** Manuell über Actions → "Pre-Release" → Run workflow (Tag z.B. `v1.2.0-beta.1`)
- **Release:** Bei Tag-Push `v*` oder manuellem Trigger → Artifacts werden an GitHub Release angehängt

## 12. Fragen & Mitwirken

- **Issues:** Öffne Issues für Bugs oder Feature-Requests
- **PR-Format:** Siehe Git-Workflow oben
- **Technische Docs:** 
  - [Kategorisierungs-Regeln](CATEGORIZATION_RULES.md)
  - [Daueraufträge-UI](RECURRING_UI.md)

Viel Erfolg! 🚀
