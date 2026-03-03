# Architektur-Roadmap (Wachstumsfähig)

## Ziel

Diese Roadmap beschreibt eine **inkrementelle Zielarchitektur** für Finanzübersicht,
so dass neue Features (z. B. Budgets, Ziele, Sync, Import/Export, Reports) leicht
hinzugefügt werden können, ohne dass ViewModels oder ein zentraler Service zu groß
werden.

Die Umsetzung ist bewusst in **3 kleine PRs** aufgeteilt, damit das Risiko niedrig
bleibt und die App jederzeit lauffähig ist.

---

## Leitprinzipien

1. **UI bleibt dünn**
   - ViewModels orchestrieren nur, enthalten aber keine Navigation/Dialog-Details.
2. **Fachlogik in Core**
   - Regeln, Validierungen und Berechnungen liegen in App-unabhängigen Services.
3. **Ports vor Implementierungen**
   - Interfaces definieren Verträge; konkrete Klassen sind austauschbar.
4. **Inkrementelle Migration**
   - Keine Big-Bang-Refactorings; jeder Schritt ist rückwärtskompatibel.
5. **Testbarkeit zuerst**
   - Neue Logik wird zuerst über Unit-Tests abgesichert.

---

## Zielarchitektur (Soll-Zustand)

## Projektgrenzen

- `Finanzuebersicht` (MAUI App)
  - Views, ViewModels, Plattform-spezifische Adapter
  - DI-Komposition (`MauiProgram`)
- `Finanzuebersicht.Core`
  - Models, UseCases/Fachservices, Repository-Interfaces, Validierung
- `Finanzuebersicht.Tests`
  - Unit-Tests für Core + ViewModel-Tests (mit Mocks/Fakes)

> Optional mittelfristig: `Finanzuebersicht.Infrastructure` für Persistenzadapter
> (JSON, später SQLite/Cloud), falls Core weiter wächst.

### Ziel-Ordnerstruktur (Core)

```text
Finanzuebersicht.Core/
  Models/
  Interfaces/
    Repositories/
      ICategoryRepository.cs
      ITransactionRepository.cs
      IRecurringTransactionRepository.cs
    Services/
      IRecurringGenerationService.cs
      IReportingService.cs
      ITransactionValidationService.cs
  UseCases/
    Transactions/
      SaveTransactionUseCase.cs
      DeleteTransactionUseCase.cs
      GetMonthlyTransactionsUseCase.cs
    Dashboard/
      GetDashboardMonthUseCase.cs
      GetDashboardYearUseCase.cs
    Recurring/
      GeneratePendingRecurringUseCase.cs
  Services/
    RecurringGenerationService.cs
    ReportingService.cs
    TransactionValidationService.cs
```

### Ziel-Ordnerstruktur (App)

```text
Finanzuebersicht/
  Services/
    Navigation/
      INavigationService.cs
      ShellNavigationService.cs
    Dialogs/
      IDialogService.cs
      ShellDialogService.cs
    (bestehende Theme/Localization Services)
  ViewModels/
    (nutzen nur Interfaces, keine Shell.Current-Aufrufe)
```

---

## Migrationsplan in 3 kleinen PRs

## PR 1 — UI-Kopplung lösen (niedriges Risiko)

**Ziel:** ViewModels von `Shell.Current` und direkten `DisplayAlert`-Aufrufen entkoppeln.

### Änderungen
- App-Projekt:
  - `INavigationService` + `ShellNavigationService`
  - `IDialogService` + `ShellDialogService`
- ViewModels umstellen:
  - `TransactionsViewModel`, `CategoriesViewModel`, `RecurringTransactionsViewModel`
  - `TransactionDetailViewModel`, `RecurringTransactionDetailViewModel`, `SettingsViewModel`
- DI-Registrierung in `MauiProgram.cs`

### Ergebnis
- Bessere Testbarkeit der ViewModels
- Kein Plattformwissen in ViewModels
- Keine funktionale Verhaltensänderung

### Akzeptanzkriterien
- Alle bestehenden UI-Flows funktionieren unverändert
- ViewModels enthalten keine `Shell.Current`-Zugriffe mehr
- Neue Unit-Tests für Navigation/Dialog-Aufrufe vorhanden

---

## PR 2 — `IDataService` hinter Ports aufsplitten (ohne Datenformatänderung)

**Ziel:** Monolithischen Service in klar getrennte Verträge zerlegen.

### Änderungen
- Core:
  - Repository-Interfaces einführen:
    - `ICategoryRepository`
    - `ITransactionRepository`
    - `IRecurringTransactionRepository`
  - Service-Interfaces für Aggregation/Recurring einführen:
    - `IReportingService`
    - `IRecurringGenerationService`
- App/Core Implementierung:
  - `LocalDataService` bleibt zunächst bestehen, implementiert aber die neuen Interfaces
  - `IDataService` bleibt temporär als Kompatibilitäts-Fassade
- ViewModels/Services schrittweise auf neue Interfaces umstellen

### Ergebnis
- Verantwortlichkeiten klar getrennt
- Künftige Persistenzwechsel (JSON → SQLite/Cloud) einfacher
- Kein Big-Bang und kein Datenmigrationszwang

### Akzeptanzkriterien
- Alle bestehenden Tests grün
- Neue Tests pro Interface-Vertrag
- Funktional identisches Verhalten zur vorherigen Version

---

## PR 3 — Fachlogik zentralisieren + Styles konsolidieren

**Ziel:** Regeln aus ViewModels herausziehen und wiederverwendbar machen.

### Änderungen
- Core UseCases/Services einführen:
  - Speichern/Validieren von Transaktionen
  - Wiederkehrende Buchungen generieren
  - Dashboard-/Jahresaggregationen
- ViewModels vereinfachen:
  - nur Input sammeln, UseCase aufrufen, Ergebnis für UI mappen
- Hardcoded Werte reduzieren:
  - Farbwerte aus ViewModels/Convertern/Charts über zentrale Ressourcen/Tokens beziehen

### Ergebnis
- Einheitliche Fachregeln
- Weniger Duplikate, weniger Regressionen
- Bessere Erweiterbarkeit (z. B. Budget-Checks, Warnungen, Forecast-Regeln)

### Akzeptanzkriterien
- Kernregeln liegen nicht mehr in ViewModels
- Neue UseCase-Unit-Tests decken Erfolgs- und Fehlerpfade ab
- UI verwendet zentrale Style-/Theme-Definitionen

---

## Phase 2 (später) — Projektaufteilung ohne Framework-Wechsel

**Ziel:** Architektur weiter entkoppeln, dabei bewusst bei `CommunityToolkit.Mvvm` bleiben
(kein Wechsel auf Prism geplant).

### Hintergrund
- Für ein Solo-Projekt ist `CommunityToolkit.Mvvm` leichtgewichtig und ausreichend.
- Die Trennung in zusätzliche Projekte verbessert dennoch Wartbarkeit und Austauschbarkeit.

### Geplante Zielstruktur

```text
Finanzuebersicht.Domain/          (optional, fachliche Kernmodelle)
Finanzuebersicht.Application/     (UseCases, Ports/Interfaces, Validierung)
Finanzuebersicht.Infrastructure/  (JSON/SQLite/Cloud-Adapter, z. B. LocalDataService)
Finanzuebersicht/                 (MAUI UI, ViewModels, DI-Komposition)
Finanzuebersicht.Tests/           (Tests pro Layer)
```

### Migrationsvorschlag in kleinen PRs

1. **PR A — `Finanzuebersicht.Application` einführen (strukturierend)**
  - Neues Projekt anlegen
  - Bestehende Interfaces/Verträge (Repositories/Services) dorthin verschieben
  - Referenzen und Namespaces ohne Logikänderung anpassen

2. **PR B — `Finanzuebersicht.Infrastructure` einführen (adapterseitig)**
  - Neues Projekt anlegen
  - `LocalDataService` und künftige Persistenzadapter dorthin verschieben
  - DI in `MauiProgram.cs` neu verdrahten

3. **PR C — Teststruktur nachziehen und konsolidieren**
  - Tests je Layer sauber zuordnen
  - Build-/Testläufe für alle Projekte absichern

### Akzeptanzkriterien für Phase 2
- Keine funktionale Verhaltensänderung in der App
- Alle bestehenden Tests bleiben grün
- UI referenziert nur Application-Contracts, keine konkreten Infrastructure-Typen
- Persistenzadapter sind austauschbar, ohne ViewModels anzupassen

---

## Technische Schulden / Beobachtungen

1. `LocalDataService` enthält aktuell mehrere Verantwortungen (CRUD + Reporting + Recurring).
2. Einige ViewModels enthalten UI-Details (Navigation/Dialogs), was Tests erschwert.
3. Mehrere Farbwerte sind hart kodiert und nicht zentral konfiguriert.
4. Readme und Projektanleitung sollten Sprach-/Plattformangaben konsistent halten.

---

## Definition of Done (gesamt)

- Neue Features können ohne Änderungen an bestehenden ViewModels in klaren UseCases ergänzt werden.
- Austausch der Persistenz ist auf Infrastructure/Adapter begrenzt.
- ViewModels bleiben dünn und testbar.
- Tests schützen Kernlogik und verhindern Regressionsfehler bei Wachstum.

---

## Umsetzungsstand (Stand: PR #34)

### Bereits umgesetzt

- **PR 1 (UI-Kopplung lösen):** abgeschlossen
  - Navigation/Dialog über `INavigationService` und `IDialogService`
  - ViewModels ohne direkte `Shell.Current`-/`DisplayAlert`-Zugriffe

- **PR 2 (`IDataService` hinter Ports aufsplitten):** abgeschlossen
  - Aufteilung in kleinere Verträge (`ICategoryRepository`, `ITransactionRepository`,
    `IRecurringTransactionRepository`)
  - Consumer auf granulare Ports migriert
  - Kompatibilitäts-Fassade (`DataServiceFacade`) eingeführt

- **PR 3 (Fachlogik zentralisieren):** weitgehend abgeschlossen
  - `ITransactionValidationService` + `TransactionValidationService`
  - `IRecurringGenerationService` + `RecurringGenerationService`
  - `IReportingService` + `ReportingService`

- **Styles/Farbkonsolidierung:** teilweise umgesetzt
  - Converter-Farben zentralisiert (`ColorResourceHelper`, PR #34)

### Noch offen

- UseCase-Ordner-/Typstruktur in Core konsequent aufbauen (`UseCases/*`)
- Restliche hardcodierte Farbwerte (u. a. in ViewModels/Charts) auf zentrale Tokens umstellen
- Später optional: Phase 2 mit separaten Projekten (`Application`/`Infrastructure`)

---

## Vorschlag für nächste konkrete Umsetzung

Start mit **PR 1 (UI-Kopplung lösen)**, weil:
- sehr geringe Fachrisiken,
- sofort messbarer Testbarkeitsgewinn,
- ideale Basis für PR 2 und PR 3.
