# Änderungsverlauf

## [1.15] - 2026-06-20

### Hinzugefügt

- Sparziel-Detailseite: bearbeiten, löschen, Beitrag buchen (#230)
- Dashboard-Karte „Nächste 30 Tage“ für Cashflow-Vorschau (#231)
- Kompaktere Dauerauftrag-Aktionen auf dem Dashboard (#232)
- Dashboard Summary-Karte und einklappbare Budget-/Jahresabschnitte (#233)
- Fällige Daueraufträge mit 7-Tage-Vorschau und lokalisierten Hinweisen
- `LanguageChanged`-Event und feste EUR-Formatierung (`CurrencyCulture`) unabhängig von UI-Sprache

### Behoben

- Dauerauftrag- und Sparziel-Startdatum: `DatePicker` mit sofortiger Binding-Aktualisierung
- Kontoauswahl-Dropdown lesbar in Light Mode; „Alle Konten“ in EN lokalisiert
- Sprachwechsel aktualisiert Dauerauftrag-Hinweise und formatierte Beträge sofort
- Sparziel-Fortschritt nach „Beitrag buchen“; Summary-Karte beim Monat/Jahr-Wechsel

## [1.14] - 2026-06-18

### Hinzugefügt

- Onboarding-Wizard (5 Schritte) beim ersten Start, erneut aufrufbar über Einstellungen
- Einheitliches `EmptyStateView` auf Listen-Seiten
- Snackbar-Feedback beim Speichern; Transaktion löschen mit Rückgängig
- Manueller Saldo-Abgleich (Ist-Saldo passt Anfangssaldo an) in Konto bearbeiten (#214)

### Geändert

- MAUI 10.0.71 und Microsoft.Extensions.* 10.0.9

### Behoben

- Onboarding: Einstellungen-Schritt schließt Wizard nicht mehr vorzeitig; eigener Abschluss-Schritt mit „Zum Dashboard“
- Onboarding: UI-Polish (Button-Breite, Skip als Sekundär-Button, einheitliche Aktions-Buttons)

## [1.13] - 2026-06-13

### Hinzugefügt

- `SelectionField` + `SelectionPopup` als Ersatz für native `Picker` in Formularen und Filtern (Mac-Catalyst-Workaround für macOS 27 Beta)
- Scroll-Hinweis (▼) auf langen Formularseiten (Einstellungen, Dauerauftrag bearbeiten)

### Geändert

- Auswahlfelder in Transaktionen, Daueraufträgen, Konten, Kategorien, Umbuchungen, Import-Vorschau, Dashboard- und Cashflow-Filtern nutzen `SelectionField` statt `Picker`
- `DatePicker` bleibt nativ (kein Freeze auf getesteten Systemen)

### Behoben

- Dashboard: Kontosaldo erscheint beim ersten Wechsel von „Alle Konten“ zu einem Konto
- `SelectionPopup`: Schließen per Klick außerhalb; deaktivierte `SelectionField`-Zeilen öffnen kein Popup

### Dokumentation

- Mac-Catalyst-Picker-Untersuchung und Entwickler-Referenz aktualisiert

## [1.12] - 2026-06-10

### Hinzugefügt

- Dashboard-Kontenübersicht mit Gesamtsaldo und Kontofilter
- Anfangssaldo pro Konto in der Kontenverwaltung
- 30-Tage-Cashflow-Vorschau vom Dashboard

## [0.5] - Unveröffentlicht (archiviert)

### Hinzugefügt

- Zentrale `SystemCategoryKeys`-Konstanten eingeführt, um die Verwendung von `magic strings`
  in der Geschäftslogik zu vermeiden. (Issue #43)

### Geändert

- `SysCat_*`-`magic string`-Literale in Core, Infrastruktur und Tests durch
  `SystemCategoryKeys`-Konstanten ersetzt.

### Hinweise

- Alle Unit-Tests laufen lokal erfolgreich durch (118 Tests).
- Version auf 0.5 erhöht.
