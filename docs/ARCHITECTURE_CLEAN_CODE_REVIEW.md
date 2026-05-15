# Architektur-, Clean-Code- und .NET-Best-Practices-Review

Stand: `develop` bei Commit `0e1041e`  
Datum: 2026-05-12

Dieses Review bewertet den aktuellen Projektstand mit Schwerpunkt Architektur,
Clean Code, Testbarkeit, .NET/MAUI-Best-Practices und Wartbarkeit. Es ist so
strukturiert, dass daraus direkt GitHub-Issues mit Beschreibung,
Akzeptanzkriterien und betroffenen Dateien erstellt werden können.

## Kurzfazit

Die Codebasis ist funktional stabil und hat bereits wichtige Qualitätsmerkmale:

- klare Projektaufteilung in `Core`, `Application`, `Infrastructure`, `MAUI App`
  und `Tests`
- MVVM mit `CommunityToolkit.Mvvm`
- viele Tests auf Core-, Infrastructure- und Use-Case-Ebene
- lokale JSON-Persistenz mit spezialisierten Stores
- DI-Registrierung und erste Clean-Architecture-Ausrichtung
- Nerdbank.GitVersioning und CI/CD sind vorhanden

Die größten Architektur-Risiken liegen aktuell aber in unscharfen Layer-Grenzen:

1. `Finanzuebersicht.Core` enthält noch Infrastruktur-Logik wie Datei-I/O,
   Backup/Restore, Settings-Persistenz, Import und Logging.
2. ViewModels enthalten teilweise direkte MAUI-/Shell-/Toolkit-Abhängigkeiten.
3. JSON-Persistenz und Restore sind noch nicht robust genug gegen Korruption
   oder Teilzustände.
4. Build-/Package-Konfiguration ist nicht vollständig reproduzierbar.
5. Tests decken Kernlogik gut ab, aber ViewModel-, UI- und End-to-End-Flows kaum.

## Aktueller technischer Stand

### Projektstruktur

| Projekt | Rolle | Bewertung |
|---------|-------|-----------|
| `Finanzuebersicht.Core` | Modelle, Interfaces, Domain-/Service-Logik | Fachlich zentral, aber noch mit Infrastruktur vermischt |
| `Finanzuebersicht.Application` | Use Cases | guter Schritt Richtung Clean Architecture, teils noch sehr dünn |
| `Finanzuebersicht.Infrastructure` | JSON Stores, DI für Persistenz | sinnvoll, sollte aber mehr Infrastruktur aus `Core` übernehmen |
| `Finanzuebersicht` | MAUI App, Views, ViewModels, UI-Services | funktionsfähig, aber UI-Abhängigkeiten reichen zu weit in ViewModels |
| `Finanzuebersicht.Tests` | xUnit Tests | 161 Tests, Schwerpunkt Services/UseCases |

### Lokaler Qualitätsstand

- `dotnet test Finanzuebersicht.Tests --no-restore --verbosity minimal`
  erfolgreich: 161 Tests, 0 Fehler, 1 Nullable-Warnung
- Mac-Catalyst-App-Build war lokal erfolgreich mit:
  `dotnet build Finanzuebersicht/Finanzuebersicht.csproj -f net10.0-maccatalyst`
- Ein Solution-Build mit `-f net10.0-maccatalyst` ist irreführend, weil auch
  `net10.0`-only-Projekte wie Tests/Application/Core/Infrastructure auf das
  Plattform-TFM gezwungen werden.

## Bestehende offene GitHub-Issues, die mit diesem Review zusammenhängen

| Issue | Thema | Einordnung |
|-------|-------|------------|
| #138 | ViewModel Unit Tests einführen | passt direkt zu Testbarkeit und UI-Entkopplung |
| #64 | Steuer-Export CSV/PDF | profitiert von klarer Export-/Reporting-Architektur |
| #53 | optionale lokale Verschlüsselung | setzt robuste Persistenz- und Settings-Grenzen voraus |
| #49 | Multi-Account & Währung | benötigt saubere Domain- und Persistenzgrenzen |

Für #49 und #53 sollten zuerst Persistenz-, Layering- und Testbarkeits-Themen
stabilisiert werden, damit größere Modelländerungen nicht auf instabilen
Abstraktionen aufbauen.

## Angelegte und aktualisierte GitHub-Issues

Aus diesem Review wurden am 2026-05-12 folgende Issues angelegt bzw. bestehende
Issues ergänzt:

| Issue | Titel | Milestone |
|-------|-------|-----------|
| #152 | JSON-Persistenz: Korruption nicht stillschweigend als leere Daten behandeln | v1.6 – Architektur & Datenrobustheit |
| #153 | Restore-Prozess transaktional absichern | v1.6 – Architektur & Datenrobustheit |
| #154 | Refactor Core: I/O-, Logging- und Persistenz-Services aus Core auslagern | v1.6 – Architektur & Datenrobustheit |
| #155 | CSV-Import: Teilimporte und globale Side-Effects vermeiden | v1.6 – Architektur & Datenrobustheit |
| #156 | Architektur: UI-nahe MAUI-APIs aus ViewModels herauslösen | v1.5 – Testabdeckung & ViewModel-Tests |
| #157 | Refactor: SettingsViewModel in fachlich getrennte Komponenten zerlegen | v1.5 – Testabdeckung & ViewModel-Tests |
| #158 | Refactor: Lifecycle- und Selection-Logik aus Views entfernen | v1.5 – Testabdeckung & ViewModel-Tests |
| #159 | Recurring-Berechnung zentralisieren und CancellationToken unterstützen | v1.6 – Architektur & Datenrobustheit |
| #160 | DataServiceFacade in fachlich getrennte Interfaces aufteilen | v1.6 – Architektur & Datenrobustheit |
| #161 | DI-Registrierung in modulare Extensions aufteilen | v1.6 – Architektur & Datenrobustheit |
| #162 | Namespaces nach Layern trennen und vereinheitlichen | v1.6 – Architektur & Datenrobustheit |
| #163 | SettingsService: I/O abstrahieren und async/testbar machen | v1.6 – Architektur & Datenrobustheit |
| #164 | Refactor: Static Cache aus KategorieIdToIconConverter entfernen | v1.5 – Testabdeckung & ViewModel-Tests |
| #165 | Build-Reproduzierbarkeit erhöhen: Floating Package-Versionen eliminieren | v1.7 – Build, CI & Wartbarkeit |
| #166 | MSBuild-Qualitätsgate einführen: Warnings als Fehler behandeln | v1.7 – Build, CI & Wartbarkeit |
| #167 | CI um Code-Coverage und Release-Test-Gate erweitern | v1.7 – Build, CI & Wartbarkeit |
| #168 | UseCases um CancellationToken und klare Application-Verantwortung erweitern | v1.6 – Architektur & Datenrobustheit |

Zusätzlich wurden bestehende Issues ergänzt:

| Issue | Änderung |
|-------|----------|
| #138 | um Architektur-Querverweise und ergänzende ViewModel-Test-Akzeptanzkriterien erweitert |
| #64 | dem Milestone v1.8 – Export & Reporting zugeordnet und um Architekturhinweise ergänzt |
| #53 | auf `status:blocked` gesetzt und um Persistenz-/Settings-Vorarbeiten ergänzt |
| #49 | auf `status:blocked` gesetzt und um Architektur-/Persistenz-Vorarbeiten ergänzt |

Milestones `v1.3 – Dashboard & Navigation` und `v1.4 – Backup & Einstellungen`
wurden geschlossen, da sie keine offenen Issues mehr enthalten.

## Priorisierte Issue-Kandidaten

### P0 - Core von Infrastruktur befreien

**Titel:** `Refactor Core: I/O-, Logging- und Persistenz-Services aus Core auslagern`

**Labels:** `architecture`, `refactor`, `.NET`, `priority:high`

**Problem:**  
`Finanzuebersicht.Core` ist aktuell kein reiner Domain-/Abstraktions-Layer. In
`Core` liegen u. a. Datei-I/O, ZIP/JSON-Serialisierung, Settings-Persistenz,
Import-Workflow und Datei-Logging.

**Betroffene Dateien:**

- `Finanzuebersicht.Core/Services/BackupService.cs`
- `Finanzuebersicht.Core/Services/SettingsService.cs`
- `Finanzuebersicht.Core/Services/ImportService.cs`
- `Finanzuebersicht.Core/Services/FileLogger.cs`
- `Finanzuebersicht.Core/Services/AppPaths.cs`
- `Finanzuebersicht.Infrastructure/Services/*`

**Impact:**  
Der Core-Layer ist dadurch an Dateisystem, Serialisierung, Logging und lokale
Speicherstrategie gekoppelt. Das erschwert Tests, spätere Verschlüsselung,
CloudKit/Sync, Multi-Account und alternative Speicherorte.

**Empfohlener Ansatz:**

- In `Core` nur Modelle, fachliche Services und Interfaces behalten.
- Datei-/ZIP-/JSON-/Settings-/Logging-Implementierungen nach
  `Finanzuebersicht.Infrastructure` verschieben.
- Falls nötig neue Contracts einführen, z. B. `ISettingsStore`,
  `IBackupArchiveStore`, `IFileLogger`, `IAppPathProvider`.
- Bestehende Tests auf Interfaces ausrichten.

**Akzeptanzkriterien:**

- `Core` enthält keine direkten Dateisystemzugriffe (`File`, `Directory`,
  `ZipFile`) außerhalb klar begründeter reiner Hilfslogik.
- `BackupService`, `SettingsService`, `ImportService` oder deren
  Infrastruktur-Anteile leben im Infrastructure-Layer.
- MAUI-App und Tests beziehen Implementierungen nur über DI.
- Bestehende Tests laufen unverändert oder mit gezielter Anpassung weiter.

---

### P0 - JSON-Persistenz gegen Datenkorruption absichern

**Titel:** `JSON-Persistenz: Korruption nicht stillschweigend als leere Daten behandeln`

**Labels:** `bug`, `architecture`, `data`, `priority:high`

**Problem:**  
`JsonDataStoreBase.LoadAsync<T>` behandelt korruptes JSON wie eine leere Liste:

- `Finanzuebersicht.Infrastructure/Services/JsonDataStoreBase.cs:33-47`

Dadurch können beschädigte Dateien in der App wie "keine Daten vorhanden"
wirken. Das ist gefährlich, weil Datenverlust oder Datenkorruption nicht
sichtbar wird.

**Impact:**  
Nutzer könnten nach einer beschädigten Datei scheinbar leere Daten sehen.
Folgeoperationen können diese leeren Listen wieder speichern und den
ursprünglichen Zustand überschreiben.

**Empfohlener Ansatz:**

- Fehlende Datei und korruptes JSON strikt unterscheiden.
- Bei korruptem JSON eine domänenspezifische Exception oder ein explizites
  Ergebnis zurückgeben.
- Vor Überschreiben beschädigter Dateien ein Recovery-/Backup-Konzept nutzen.
- Optional Quarantäne-Dateien anlegen, z. B. `transactions.corrupt.<timestamp>.json`.
- Save-Operationen atomar machen: temp file schreiben, flushen, dann ersetzen.

**Akzeptanzkriterien:**

- Korruptes JSON führt nicht mehr zu `[]`.
- Fehler wird sichtbar geloggt und in der UI oder über einen Recovery-Flow
  behandelbar.
- Writes sind atomar oder mindestens gegen halbe Dateien abgesichert.
- Tests decken fehlende Datei, leere Datei, korruptes JSON und erfolgreichen
  Roundtrip ab.

---

### P0 - Restore wirklich atomar machen

**Titel:** `Restore-Prozess transaktional absichern`

**Labels:** `bug`, `data`, `architecture`, `priority:high`

**Problem:**  
`BackupService.AtomicRestoreAsync` schreibt Entity-Typen nacheinander und macht
bei Fehlern ein Best-Effort-Rollback:

- `Finanzuebersicht.Core/Services/BackupService.cs:237-293`

Wenn Restore oder Rollback fehlschlägt, kann ein gemischter Datenzustand
entstehen.

**Impact:**  
Ein Restore kann Kategorien, Transaktionen, Daueraufträge, Budgets und Sparziele
teilweise aus unterschiedlichen Zuständen hinterlassen. Das ist besonders
kritisch für spätere Features wie Verschlüsselung oder Multi-Account.

**Empfohlener Ansatz:**

- Restore in ein Staging-Verzeichnis schreiben.
- Alle Dateien validieren.
- Danach per finalem Swap/Replace aktivieren.
- Rollback über alte Dateien/Verzeichnisse absichern statt über erneutes
  Schreiben aus In-Memory-Snapshots.
- RestoreResult um technische Fehlerdetails und Recovery-Hinweise erweitern.

**Akzeptanzkriterien:**

- Bei Restore-Fehler bleibt der vorherige Datenbestand unverändert.
- Bei Rollback-Fehler gibt es einen reproduzierbaren Recovery-Pfad.
- Tests simulieren Fehler nach jedem Entity-Typ.
- Restore schreibt nie dauerhaft teilvalidierte Daten.

---

### P1 - ImportService als explizite Batch-Operation modellieren

**Titel:** `CSV-Import: Teilimporte und globale Side-Effects vermeiden`

**Labels:** `refactor`, `data`, `import`, `priority:medium`

**Problem:**  
`ImportService.ImportFromCsvAsync` speichert Transaktionen während der
Verarbeitung einzeln und läuft bei Save-Fehlern weiter:

- `Finanzuebersicht.Core/Services/ImportService.cs:28-223`
- Save-Fehler werden in `ImportService.cs:195-205` geloggt, aber der Import
  wird fortgesetzt.

Zusätzlich gibt es direkte `FileLogger.Append`-Side-Effects und mehrere Stellen,
an denen Fehler zu `[]` führen.

**Impact:**  
Importe können teilweise erfolgreich sein, ohne dass die App oder der Nutzer
einen klaren Importstatus erhält. Das erschwert spätere Reconciliation,
Multi-Account und Tests.

**Empfohlener Ansatz:**

- `ImportResult` einführen mit `Imported`, `Skipped`, `Duplicates`, `Errors`.
- Vor dem Speichern validieren und deduplizieren.
- Save als Batch/Unit-of-Work durchführen oder klare Fail-Fast-Strategie
  definieren.
- Logging nur über `ILogger` oder abstrahierten Logger.
- Keine stillen `return []` bei Infrastrukturfehlern.

**Akzeptanzkriterien:**

- Import liefert einen vollständigen Ergebnisreport.
- Teilimporte sind entweder explizit erlaubt und sichtbar oder werden verhindert.
- Fehlerfälle sind getestet: Parserfehler, Lesefehler, Save-Fehler,
  Duplikate, Abbruch per `CancellationToken`.

---

### P1 - ViewModels von direkten MAUI-/Toolkit-APIs entkoppeln

**Titel:** `Architektur: UI-nahe MAUI-APIs aus ViewModels herauslösen`

**Labels:** `architecture`, `ui`, `viewmodel`, `test`, `priority:high`

**Problem:**  
Mehrere ViewModels nutzen direkt MAUI- oder CommunityToolkit-APIs:

- `SettingsViewModel` nutzt `FolderPicker.Default` und `FileSaver.Default`
  (`Finanzuebersicht/ViewModels/SettingsViewModel.cs:181-219`,
  `SettingsViewModel.cs:384-427`)
- `TransactionsViewModel` nutzt `MainThread` und Import/File/UI-nahe Flows
  (`Finanzuebersicht/ViewModels/TransactionsViewModel.cs:188-206`)
- Navigation/Dialog sind teilweise abstrahiert, aber Shell-Abhängigkeiten liegen
  noch in konkreten Services.

**Impact:**  
ViewModels sind schwer isoliert testbar und hängen an MAUI-Runtime,
Threading-Modell und konkreten Toolkit-Singletons. Das erschwert Issue #138
("ViewModel Unit Tests").

**Empfohlener Ansatz:**

- Interfaces einführen:
  - `IFolderPickerService`
  - `IFileSaverService`
  - `IFilePickerService`
  - `IDispatcherService` oder `IUiThreadDispatcher`
  - optional `IDataChangedNotifier`
- MAUI-spezifische Implementierungen im Presentation-/MAUI-Projekt halten.
- ViewModels ausschließlich gegen diese Interfaces testen.

**Akzeptanzkriterien:**

- ViewModels können ohne MAUI-AppHost instanziiert und getestet werden.
- Keine direkte Nutzung von `FolderPicker.Default`, `FileSaver.Default`,
  `FilePicker.Default`, `Shell.Current` oder `MainThread` in ViewModels.
- Erste ViewModel-Tests aus #138 lassen sich ohne Plattform-Workarounds
  schreiben.

---

### P1 - SettingsViewModel fachlich zerlegen

**Titel:** `Refactor: SettingsViewModel in fachlich getrennte Komponenten zerlegen`

**Labels:** `refactor`, `viewmodel`, `clean-code`, `priority:medium`

**Problem:**  
`SettingsViewModel` mischt mehrere Verantwortlichkeiten:

- Theme
- Sprache
- Datenpfad
- Backup-Erstellung
- Backup-Navigation
- CSV-Export
- Versions-/Library-Info

Betroffene Datei:

- `Finanzuebersicht/ViewModels/SettingsViewModel.cs`

**Impact:**  
Die Klasse ist schwer zu testen, Änderungen sind risikoreich, und fachlich
unabhängige Features koppeln über einen großen ViewModel-Typ.

**Empfohlener Ansatz:**

- Fachliche Teilbereiche extrahieren:
  - `AppearanceSettingsViewModel` oder Service
  - `StorageSettingsViewModel` oder Service
  - `BackupSettingsViewModel` oder Service
  - `AboutViewModel`/`LibraryInfoProvider`
- Alternativ zuerst Services extrahieren und UI später splitten.
- Bestehende SettingsPage kann zunächst erhalten bleiben.

**Akzeptanzkriterien:**

- Backup/Export-Logik ist nicht mehr direkt im SettingsViewModel.
- Library-Ermittlung ist separat testbar.
- Theme- und Sprachwechsel bleiben unverändert nutzbar.
- Bestehende UI bleibt funktional.

---

### P1 - View-Code-Behind auf reines UI-Binding reduzieren

**Titel:** `Refactor: Lifecycle- und Selection-Logik aus Views entfernen`

**Labels:** `refactor`, `ui`, `mvvm`, `priority:medium`

**Problem:**  
Mehrere Views lösen fachliche Aktionen im Code-Behind aus:

- `TransactionsPage.xaml.cs:28-57`
- `DashboardPage.xaml.cs`
- `RecurringTransactionsPage.xaml.cs`
- `BackupListPage.xaml.cs`
- `TransactionDetailPage.xaml.cs`

Beispiel: `TransactionsPage` führt im `OnAppearing` direkt
`LoadTransaktionenCommand` aus und behandelt Selektion im Code-Behind.

**Impact:**  
Lifecycle-Logik ist dupliziert, schwer testbar und über Views verteilt.
Navigation und Reload-Verhalten werden dadurch inkonsistent.

**Empfohlener Ansatz:**

- Einheitliches `ILoadableViewModel`-Pattern oder MAUI Behaviors nutzen.
- Selection über Commands im XAML binden, wo möglich.
- Code-Behind nur noch für reine View-Belange verwenden.

**Akzeptanzkriterien:**

- Pages enthalten keine fachlichen Load-/Refresh-Entscheidungen mehr.
- Wiederkehrendes `OnAppearing -> LoadCommand` ist zentralisiert.
- Selection-Handling ist testbarer und einheitlich.

---

### P1 - Recurring-Berechnung zentralisieren und abbrechbar machen

**Titel:** `Recurring-Berechnung zentralisieren und CancellationToken unterstützen`

**Labels:** `refactor`, `architecture`, `recurring`, `priority:medium`

**Problem:**  
Recurring-Logik ist über Service und Use Case verteilt. Scheduling,
Ausnahmen/Hints und nächste Fälligkeit sollten fachlich an einer Stelle liegen.
Außerdem fehlen an einigen APIs durchgereichte `CancellationToken`.

**Betroffene Dateien:**

- `Finanzuebersicht.Core/Services/RecurringGenerationService.cs`
- `Finanzuebersicht.Application/UseCases/RecurringTransactions/GetDueRecurringWithHintsUseCase.cs`
- weitere Recurring-UseCases

**Impact:**  
Duplizierte oder divergierende Fälligkeitslogik erzeugt Fehler bei Randfällen
wie verschobenen Instanzen, Ausnahmen oder Wiederaufnahme nach App-Start.

**Empfohlener Ansatz:**

- Reine Domänenkomponente einführen, z. B. `RecurringScheduleCalculator`.
- Service und UseCases nutzen dieselbe Berechnung.
- `CancellationToken` konsequent in längeren Operationen durchreichen.

**Akzeptanzkriterien:**

- Next-Due-/Exception-/Hint-Logik existiert nur noch einmal.
- Tests decken monatlich, jährlich, übersprungene und verschobene Instanzen ab.
- Public async APIs akzeptieren bei potenziell längeren Operationen
  `CancellationToken`.

---

### P2 - DataServiceFacade in fachliche Interfaces aufteilen

**Titel:** `DataServiceFacade in fachlich getrennte Interfaces aufteilen`

**Labels:** `architecture`, `refactor`, `priority:medium`

**Problem:**  
`DataServiceFacade` aggregiert Repositories und Services für Kategorien,
Transaktionen, Daueraufträge, Reporting, Budgets und Sparziele:

- `Finanzuebersicht.Core/Services/DataServiceFacade.cs:5-99`

**Impact:**  
Die Fassade versteckt fachliche Grenzen und lädt dazu ein, breitflächig
`IDataService` zu injizieren. Änderungen an einem Aggregat können dadurch
unnötig viele Klassen betreffen.

**Empfohlener Ansatz:**

- Neue Features gegen spezifische Interfaces schreiben:
  - `ITransactionRepository`
  - `ICategoryRepository`
  - `IReportingService`
  - `IBudgetRepository`
  - `ISparZielRepository`
- `IDataService` nur noch als Legacy-Kompatibilität nutzen und schrittweise
  entfernen.

**Akzeptanzkriterien:**

- Neue UseCases injizieren kein breites `IDataService`.
- Bestehende UseCases werden schrittweise auf spezifische Abhängigkeiten
  migriert.
- `IDataService` ist als Übergang markiert oder deutlich reduziert.

---

### P2 - DI-Registrierung modularisieren

**Titel:** `DI-Registrierung in modulare Extensions aufteilen`

**Labels:** `architecture`, `clean-code`, `priority:medium`

**Problem:**  
`MauiProgram.cs` registriert aktuell Services, UseCases, ViewModels und Pages
zentral:

- `Finanzuebersicht/MauiProgram.cs:37-114`

**Impact:**  
Die Composition Root wird unübersichtlich und änderungsanfällig. Neue Features
führen schnell zu Merge-Konflikten und erschweren Reviews.

**Empfohlener Ansatz:**

- Extension-Methoden einführen:
  - `AddApplicationServices()`
  - `AddInfrastructureServices()` existiert bereits und kann erweitert werden
  - `AddPresentationServices()`
  - `AddViewModels()`
  - `AddPages()`
- `MauiProgram` nur als Orchestrator belassen.

**Akzeptanzkriterien:**

- `MauiProgram.CreateMauiApp` bleibt kurz und übersichtlich.
- Registrierungen sind nach Layern/Funktionen gruppiert.
- Keine Verhaltensänderung beim App-Start.

---

### P2 - Namespaces nach Layern vereinheitlichen

**Titel:** `Namespaces nach Layern trennen und vereinheitlichen`

**Labels:** `architecture`, `refactor`, `clean-code`, `priority:medium`

**Problem:**  
Der Namespace `Finanzuebersicht.Services` wird in mehreren Layern semantisch
unterschiedlich verwendet. Dadurch verschwimmen Domain-, Application-,
Infrastructure- und Presentation-Verantwortlichkeiten.

**Impact:**  
Dateien wirken auf den ersten Blick gleichartig, obwohl sie unterschiedliche
Layer-Aufgaben haben. Das erschwert Code Reviews, Navigation und Refactorings.

**Empfohlener Ansatz:**

- Zielstruktur definieren:
  - `Finanzuebersicht.Domain` oder `Finanzuebersicht.Core`
  - `Finanzuebersicht.Application`
  - `Finanzuebersicht.Infrastructure`
  - `Finanzuebersicht.Presentation` oder MAUI-spezifisch
- Namespaces schrittweise angleichen.
- Bei großen Umbenennungen kleine PRs bevorzugen.

**Akzeptanzkriterien:**

- Neue Dateien verwenden eindeutige Layer-Namespaces.
- Bestehende Services sind mindestens nach Zielnamespace klassifiziert.
- Keine zyklischen oder unerwünschten Projektabhängigkeiten entstehen.

---

### P2 - SettingsService async und testbar machen

**Titel:** `SettingsService: I/O abstrahieren und async/testbar machen`

**Labels:** `refactor`, `settings`, `.NET`, `priority:medium`

**Problem:**  
`SettingsService` arbeitet synchron und hängt direkt an Pfaden bzw.
Dateisystemlogik.

**Betroffene Dateien:**

- `Finanzuebersicht.Core/Services/SettingsService.cs`
- `Finanzuebersicht.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`

**Impact:**  
Synchronous File-I/O kann den UI-Thread blockieren. Tests und spätere
Speicherstrategien wie Verschlüsselung oder Cloud-Sync werden erschwert.

**Empfohlener Ansatz:**

- `ISettingsStore` oder `ISettingsRepository` einführen.
- Async API anbieten, z. B. `GetAsync`, `SetAsync`, `SaveAsync`.
- Pfadauflösung über `IAppPathProvider` abstrahieren.
- Migration von bestehenden synchronen Calls in kleinen Schritten.

**Akzeptanzkriterien:**

- Settings-Persistenz ist ohne echtes Dateisystem testbar.
- UI-nahe Aufrufe blockieren nicht durch direkte File-I/O.
- Bestehende Settings bleiben migrierbar.

---

### P2 - Static Cache im Kategorie-Icon-Converter entfernen

**Titel:** `Refactor: Static Cache aus KategorieIdToIconConverter entfernen`

**Labels:** `refactor`, `ui`, `clean-code`, `priority:low`

**Problem:**  
`KategorieIdToIconConverter` nutzt globalen statischen Zustand, der von
ViewModels gesetzt wird.

**Betroffene Dateien:**

- `Finanzuebersicht/Converters/KategorieIdToIconConverter.cs`
- `Finanzuebersicht/ViewModels/TransactionsViewModel.cs`

**Impact:**  
Globale Reihenfolgeabhängigkeit kann zu veralteten oder falschen Icons führen.
Tests müssen globalen Zustand zurücksetzen.

**Empfohlener Ansatz:**

- Icon direkt im ViewModel-/View-State bereitstellen.
- Alternativ DI-fähigen Resolver verwenden.
- Converter stateless halten.

**Akzeptanzkriterien:**

- Kein globaler statischer Cache im Converter.
- Icon-Anzeige bleibt in Monats- und Suchansicht korrekt.
- Tests sind unabhängig voneinander ausführbar.

---

### P2 - Floating Package-Versionen eliminieren

**Titel:** `Build-Reproduzierbarkeit erhöhen: Floating Package-Versionen eliminieren`

**Labels:** `chore`, `.NET`, `dependencies`, `priority:medium`

**Problem:**  
Mehrere Projekte nutzen floating package versions:

- `Finanzuebersicht/Finanzuebersicht.csproj:85-88`
  - `Microsoft.Maui.Controls` `10.0.*`
  - `CommunityToolkit.Mvvm` `8.*`
  - `CommunityToolkit.Maui` `14.*`
- `Finanzuebersicht.Core/Finanzuebersicht.Core.csproj:17`
  - `Microsoft.Extensions.Logging.Abstractions` `*`
- `Finanzuebersicht.Tests/Finanzuebersicht.Tests.csproj:13`
  - `NSubstitute` `5.*`

**Impact:**  
Builds und Restores sind nicht vollständig reproduzierbar. CI und lokale
Umgebung können unterschiedliche Paketstände nutzen.

**Empfohlener Ansatz:**

- Paketversionen pinnen.
- Optional `Directory.Packages.props` für Central Package Management einführen.
- Dependabot auf zentrale Paketdatei ausrichten.

**Akzeptanzkriterien:**

- Keine `*`-Versionen in Projektdateien.
- Restore ist reproduzierbar.
- Dependabot funktioniert weiterhin.

---

### P2 - Warnungs- und Analyzer-Policy einführen

**Titel:** `MSBuild-Qualitätsgate einführen: Warnings als Fehler behandeln`

**Labels:** `chore`, `.NET`, `quality`, `priority:medium`

**Problem:**  
Nullable-Warnungen sind aktuell sichtbar, blockieren aber nicht:

- `SettingsViewModel.cs` hatte im Mac-Catalyst-Build Nullable-Warnungen.
- `SearchTransactionsUseCaseTests.cs` erzeugt aktuell `CS8601`.

Außerdem gibt es keine zentrale Analyzer-/Warning-Policy in
`Directory.Build.props`.

**Impact:**  
Warnungen können sich einschleichen und technische Schuld erhöhen.

**Empfohlener Ansatz:**

- Bestehende Warnungen zuerst beheben.
- Danach zentral konfigurieren:
  - `TreatWarningsAsErrors`
  - `AnalysisLevel`
  - ggf. `WarningsNotAsErrors` für bekannte externe Tooling-Fälle
- CI so konfigurieren, dass Warnungen nicht übersehen werden.

**Akzeptanzkriterien:**

- Aktueller Build ist warnungsfrei oder bekannte Ausnahmen sind dokumentiert.
- Neue Nullable-/Compiler-Warnungen schlagen lokal und in CI fehl.
- Policy liegt zentral in `Directory.Build.props`.

---

### P2 - CI um Coverage und Release-Test-Gate erweitern

**Titel:** `CI um Code-Coverage und Release-Test-Gate erweitern`

**Labels:** `ci`, `test`, `.NET`, `priority:medium`

**Problem:**  
Coverage wird in CI nicht sichtbar ausgewertet. Release- und Prerelease-Workflows
bauen Artefakte, sind aber nicht klar an einen vorherigen Testlauf gekoppelt.
Zusätzlich nutzt `ci-split.yml` noch deprecated `set-output`:

- `.github/workflows/ci-split.yml:57-60`

**Impact:**  
Regressionen können unbemerkt bleiben. Coverage-Trends und Mindestschwellen
fehlen. Deprecated GitHub-Actions-Syntax erzeugt Wartungsrisiken.

**Empfohlener Ansatz:**

- `dotnet test --collect:"XPlat Code Coverage"` in CI aktivieren.
- Coverage-Report veröffentlichen.
- Optional Mindestschwelle je nach Projektphase definieren.
- Release/Prerelease von erfolgreichem Testjob abhängig machen.
- `set-output` durch `$GITHUB_OUTPUT` ersetzen.

**Akzeptanzkriterien:**

- CI zeigt Coverage-Artefakt oder Summary.
- Release-Workflow läuft nicht ohne erfolgreichen Test-Gate.
- Keine deprecated `set-output`-Nutzung mehr.

---

### P2 - Application UseCases klarer als Application Layer modellieren

**Titel:** `UseCases um CancellationToken und klare Application-Verantwortung erweitern`

**Labels:** `architecture`, `application`, `.NET`, `priority:medium`

**Problem:**  
Einige UseCases sind sehr dünne Pass-Throughs und akzeptieren keine
`CancellationToken`, z. B.:

- `Finanzuebersicht.Application/UseCases/Transactions/LoadTransactionsMonthUseCase.cs`

**Impact:**  
Der Application Layer bleibt formal vorhanden, übernimmt aber nicht immer klare
Orchestrierungsverantwortung. Lange Operationen sind nicht überall abbrechbar.

**Empfohlener Ansatz:**

- UseCase-API vereinheitlichen:
  - `ExecuteAsync(commandOrQuery, CancellationToken cancellationToken = default)`
- Application-spezifische DTOs/Result-Typen bewusst modellieren.
- Keine UI-/MAUI-Typen im Application Layer.
- Wo UseCases nur delegieren, prüfen ob sie nötig sind oder fachlich erweitert
  werden sollten.

**Akzeptanzkriterien:**

- Neue/angepasste UseCases akzeptieren `CancellationToken`.
- UseCases kapseln fachliche Orchestrierung oder werden entfernt.
- Tests decken Cancellation und Result-Formate ab.

---

## Empfohlene Reihenfolge für GitHub-Issues

| Reihenfolge | Issue | Grund |
-------------|-------|-------|
| 1 | JSON-Persistenz absichern | schützt Nutzerdaten unmittelbar |
| 2 | Restore transaktional absichern | schützt vor inkonsistentem Datenzustand |
| 3 | Core von Infrastruktur befreien | Grundlage für Verschlüsselung/Multi-Account |
| 4 | ViewModels von MAUI-APIs entkoppeln | Grundlage für #138 ViewModel-Tests |
| 5 | ImportService Batch-Ergebnis | verhindert stille Teilimporte |
| 6 | Floating Package-Versionen eliminieren | schnelle Build-Stabilisierung |
| 7 | Warnings/Analyzer-Policy | verhindert neue technische Schuld |
| 8 | DI modularisieren | verbessert Wartbarkeit vor größeren Features |
| 9 | SettingsViewModel splitten | reduziert UI-Komplexität |
| 10 | Coverage/Release-Gates | macht Qualität messbar |

## Vorschlag für ein einheitliches GitHub-Issue-Template

```markdown
## Problem

<Was ist aktuell problematisch?>

## Impact

<Warum ist das relevant? Welche Risiken entstehen?>

## Betroffene Dateien/Komponenten

- `<Pfad>`

## Vorschlag

<Technischer Lösungsansatz, ohne Implementierung zu stark vorzuschreiben>

## Akzeptanzkriterien

- [ ] Verhalten ist implementiert
- [ ] Relevante Tests sind ergänzt/angepasst
- [ ] Bestehende Tests laufen
- [ ] Keine neue Compiler-/Nullable-Warnung

## Hinweise

<Abhängigkeiten, Folge-Issues, Migrationshinweise>
```

## Nicht sofort als eigene Issues duplizieren

Diese Themen existieren bereits als offene Issues und sollten eher verlinkt oder
erweitert werden:

- #138 ViewModel Unit Tests
- #64 Steuer-Export
- #53 lokale Verschlüsselung
- #49 Multi-Account und Währungen

Insbesondere #53 und #49 sollten nicht isoliert umgesetzt werden, bevor
Persistenzrobustheit, Layer-Grenzen und Settings-Abstraktion verbessert sind.
