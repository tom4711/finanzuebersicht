Fixes #48 - Dauerauftraege: Intervall, Exceptions, UI (ausfuehrliche Zusammenfassung)

Zusammenfassung
Dieses Pull Request implements das komplette Feature fuer Dauerauftraege (Issue #48).
Ziel war es, die Bedienoberflaeche und Persistenz zu verbessern, Intervall-Werte korrekt
zu speichern und die Dashboard-Prognose so anzupassen, dass nur Monate mit tatsaechlicher
Instanz eines Dauerauftrags eine Prognose-Eintragung erhalten.

Motivation
- Nutzer sahen, dass Intervalle im Detail-Editor nach dem Speichern nicht immer konsistent blieben.
- Die Prognose im Dashboard erzeugte in jedem Monat Test-Auftraege, auch wenn das Intervall z.B.
  auf Quartal gesetzt war.
- UI: Picker fuer Intervalle war string-basiert und problematisch bei Lokalisierung.

Uebersicht der wichtigsten Commits (ausgewaehlt)
- 2134f7a: fix(debug): update log to use Interval.ToString()
  - Kleine Debug-Log-Verbesserung, um gespeicherte Intervalle besser nachverfolgen zu koennen.

- 93d8b56: fix(recurring): bind Picker directly to RecurrenceInterval and add localized display converter
  - Picker in RecurringTransactionDetailPage umgestellt: bindet nun direkt an das Enum `RecurrenceInterval`.
  - Neuer Converter `EnumToLocalizedStringConverter` eingefuehrt, um deutsche Labels anzuzeigen.

- 966e752: debug(recurring): log interval values in VM and UseCase for troubleshooting
  - Zusatliche Log-Ausgaben in ViewModel und UseCase, damit Save-Vorgang und uebergebene Werte sichtbar sind.

- c813e7e: fix(recurring): map localized interval labels (German) to enum on selection
  - Mapping zwischen deutschen Anzeige-Strings und Enum-Werten verbessert (Fallbacks entfernt).

- 7d98c88: fix(recurring): reliably parse and bind selected interval (case-insensitive + TwoWay)
  - ViewModel: robustes Enum-Parsing, TwoWay-Bindung sichergestellt, IntervalValues-Property hinzugefuegt.

- a01915b: fix(recurring): persist selected recurrence interval on save
  - Persistenz: SaveUseCase aktualisiert, Interval und IntervalFactor werden korrekt in JSON gespeichert.

- a0016fa: fix(dashboard): finalize OccursInRange helpers and fix ordering
  - LoadDashboardMonthUseCase: Prognose-Logik erweitert und finalisiert.
  - Neue Helper-Methoden: OccursInRange, GetNextInstance, AddMonthsPreserveDay.
  - Ergebnis: Prognose fuegt nur dann einen Eintrag hinzu, wenn das Intervall/Factor tatsaechlich
    eine Instanz innerhalb des betrachteten Monats erzeugt.

Dateien mit relevanten Aenderungen (Auszug)
- Finanzuebersicht/Views/RecurringTransactionDetailPage.xaml
  - Picker-Umstellung, Nutzung des Converters
- Finanzuebersicht/ViewModels/RecurringTransactionDetailViewModel.cs
  - `IntervalValues`, Enum-Parsing, TwoWay-Bindung, Save-Command Logging
- Finanzuebersicht/Converters/EnumToLocalizedStringConverter.cs
  - Neue Klasse fuer deutsche Anzeige der Intervalle
- Finanzuebersicht.Application/UseCases/RecurringTransactions/SaveRecurringTransactionDetailUseCase.cs
  - Logging und Persistenzanpassungen
- Finanzuebersicht.Application/UseCases/Dashboard/LoadDashboardMonthUseCase.cs
  - Prognose-Logik: nur echte Vorkommen erzeugen Prognose-Eintraege

Validierung / Tests
- Build: macOS (Mac Catalyst) Build erfolgreich (Debug build erzeugt)
- Unit Tests: 116 von 116 Tests bestanden (Finanzuebersicht.Tests)
- Manuelle Validierung: Gespeicherte Dauerauftraege behalten nach dem Speichern das gewaehlte Intervall.
  Dashboard zeigt keine monatlichen Test-Eintraege mehr fuer Quartalsauftraege.

Anleitung fuer Reviewer / Testschritte
1. UI-Tests
   - Leg einen neuen Dauerauftrag an, pruefe Picker-Auswahl: Tages-/Wochen-/Monats-/Quartals-/Jahres-Optionen.
   - Aendere `IntervalFactor` (z.B. 2 fuer alle 2 Monate) und speichere. Oeffne den Eintrag erneut und verifiziere
     dass Interval und IntervalFactor erhalten bleiben.
   - Kontrolliere `recurring.json` in App-Daten (LocalApplicationData) auf korrekte Felder.

2. Prognose-Tests
   - Lege einen Dauerauftrag mit Intervall = Quartal an (z.B. Startdatum 2026-01-15).
   - Wechsle im Dashboard durch die Monate: nur die Monate, in denen eine Instanz erwartet wird, duerfen eine Prognose zeigen.
   - Teste ein Intervall mit IntervalFactor > 1 (z.B. alle 2 Monate) und vergleiche Monatausgaben.

3. Randfaelle
   - Startdatum am 31. eines Monats: pruefe Verhalten in Februar (AddMonthsPreserveDay sorgt fuer korrektes "clamping").
   - Enddatum gesetzt: keine Prognose nach Ablauf.
   - LetzteAusfuehrung gesetzt: Generierung respektiert bereits ausgefuehrte Instanzen.

Bekannte Limitationen / Hinweise
- Die Prognose-Logik verwendet eine Sicherheits-Schleife beim Vorwaertsrechnen (Limit 1000 Iterationen) um Endlosschleifen zu vermeiden.
- CloudKit-Integration ist unveraendert (nicht Teil dieses PR, weiterhin optional deaktiviert).

Commit-Historie und weitere Hinweise
- Fuer detaillierte Diff-Informationen siehe die Commit-Liste im PR (Commits-Tab). Die oben genannten Commits fassen die wichtigsten Schritte zusammen.

Danke! Wenn du moechtest, kann ich noch eine englische Kurzfassung fuer das Changelog hinzufuegen oder die PR-Beschreibung in kleinere Abschnitte mit direkten Links zu Datei-Teilen splitten.
