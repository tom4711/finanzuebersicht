# Änderungsverlauf

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
