# Änderungsverlauf

## [0.5] - Unveröffentlicht
### Hinzugefügt
- Zentrale `SystemCategoryKeys`-Konstanten eingeführt, um die Verwendung von `magic strings`
  in der Geschäftslogik zu vermeiden. (Issue #43)

### Geändert
- `SysCat_*`-`magic string`-Literale in Core, Infrastruktur und Tests durch
  `SystemCategoryKeys`-Konstanten ersetzt.

### Hinweise
- Alle Unit-Tests laufen lokal erfolgreich durch (118 Tests).
- Version auf 0.5 erhöht.

