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
├── Models/                        ← Transaction, Category, RecurringTransaction, etc.
└── Services/                      ← IDataService, LocalDataService, SettingsService

Finanzuebersicht/                  ← MAUI app (net10.0-ios, net10.0-maccatalyst)
├── MauiProgram.cs
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── ViewModels/                    ← DashboardVM, TransactionsVM, SettingsVM, etc.
├── Views/                         ← XAML pages
├── Converters/                    ← Value converters for UI
├── Resources/Strings/             ← AppResources.resx (+ .en.resx)
├── Resources/Styles/              ← Colors.xaml, Styles.xaml
└── Platforms/                     ← iOS, MacCatalyst

Finanzuebersicht.Tests/            ← xUnit tests (net10.0)
└── Services/                      ← LocalDataService, InitializationService tests
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

- **Branch:** Arbeite auf `feature/*`, `fix/*` oder `chore/*`, nicht auf `main`
- **Commit:** Gitmoji + Conventional Commits (siehe [.github/copilot-instructions.md](../.github/copilot-instructions.md))
- **PR:** Erstelle Pull Request gegen `main`; `main` ist geschützt

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
- ✅ Backup erstellen (Export zu Ordner)
- ✅ CSV-Import durchführen (mit Auto-Kategorisierung)
- ✅ Daten exportieren

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

- **Quick Checks:** Unit Tests, Linter auf Ubuntu (schnell, billig)
- **Full MAUI Build:** Nur für PRs gegen `main` und `main`-Pushes
- **Explizit triggern:** Label `run-maui` zur PR hinzufügen oder manuell via Actions starten

## 12. Fragen & Mitwirken

- **Issues:** Öffne Issues für Bugs oder Feature-Requests
- **PR-Format:** Siehe Git-Workflow oben
- **Technische Docs:** 
  - [Kategorisierungs-Regeln](CATEGORIZATION_RULES.md)
  - [Daueraufträge-UI](RECURRING_UI.md)

Viel Erfolg! 🚀
