# Roadmap

Übersicht über geplante Releases und Features. Die Roadmap wird fortlaufend aktualisiert.

---

## ✅ v1.0 — Stable Release *(abgeschlossen)*

- Transaktionen, Kategorien, Daueraufträge
- Dashboard mit Charts (Monatsübersicht, Jahresverlauf)
- Budgetverwaltung & Sparziele
- Backup / Restore mit Schema-Migrations-Framework
- Accessibility (VoiceOver / Tastaturnavigation)
- CI/CD: Build-Artifacts für macOS & Windows
- Vollständige Fehlerbehandlung in allen ViewModels

---

## ✅ v1.6 — Architektur & Datenrobustheit *(abgeschlossen)*

Fokus: Layering bereinigen, Persistenz robuster machen und DI modularisieren.

| Issue | Thema | Status |
|-------|-------|--------|
| [#152](https://github.com/tom4711/finanzuebersicht/issues/152) | JSON-Persistenz: Korruption nicht stillschweigend als leere Daten behandeln | ✅ Closed |
| [#153](https://github.com/tom4711/finanzuebersicht/issues/153) | Restore-Prozess transaktional absichern | ✅ Closed |
| [#154](https://github.com/tom4711/finanzuebersicht/issues/154) | Refactor Core: I/O-, Logging- und Persistenz-Services aus Core auslagern | ✅ Closed |
| [#155](https://github.com/tom4711/finanzuebersicht/issues/155) | CSV-Import: Teilimporte und globale Side-Effects vermeiden | ✅ Closed |
| [#159](https://github.com/tom4711/finanzuebersicht/issues/159) | Recurring-Berechnung zentralisieren und CancellationToken unterstützen | ✅ Closed |
| [#160](https://github.com/tom4711/finanzuebersicht/issues/160) | DataServiceFacade in fachlich getrennte Interfaces aufteilen | ✅ Closed |
| [#161](https://github.com/tom4711/finanzuebersicht/issues/161) | DI-Registrierung in modulare Extensions aufteilen | ✅ Closed |
| [#162](https://github.com/tom4711/finanzuebersicht/issues/162) | Namespaces nach Layern trennen und vereinheitlichen | ✅ Closed |
| [#163](https://github.com/tom4711/finanzuebersicht/issues/163) | SettingsService: I/O abstrahieren und async/testbar machen | ✅ Closed |
| [#168](https://github.com/tom4711/finanzuebersicht/issues/168) | UseCases um CancellationToken und klare Application-Verantwortung erweitern | ✅ Closed |

Alle 10 Issues des Milestones sind geschlossen.

---

## 🔧 v1.1 — Qualität & Performance *(geplant)* · [Milestone](https://github.com/tom4711/finanzuebersicht/milestone/5)

Fokus: Code-Qualität, technische Schulden abbauen, Performance verbessern.

| Issue | Thema | Aufwand |
|-------|-------|---------|
| [#101](https://github.com/tom4711/finanzuebersicht/issues/101) | Store-Basisklasse (`JsonStore<T>`) – DRY für alle 5 Stores | M |
| [#102](https://github.com/tom4711/finanzuebersicht/issues/102) | `YearOverviewViewModel`: `List<>` → `ObservableCollection` | S |
| [#103](https://github.com/tom4711/finanzuebersicht/issues/103) | Namespace-Bereinigung (ASCII-Konvention) | M |
| [#100](https://github.com/tom4711/finanzuebersicht/issues/100) | Bulk-Replace API für Restore (O(n²) → O(n) I/O) | M |

---

## 📊 v1.2 — Export & Auswertungen *(geplant)* · [Milestone](https://github.com/tom4711/finanzuebersicht/milestone/6)

Fokus: Erweiterte Exportmöglichkeiten für Steuern und Analysen.

| Issue | Thema | Aufwand |
|-------|-------|---------|
| [#64](https://github.com/tom4711/finanzuebersicht/issues/64) | CSV/PDF-Export für Steuerzwecke | M |

---

## 🔐 v2.0 — Sicherheit & Multi-Account *(geplant)* · [Milestone](https://github.com/tom4711/finanzuebersicht/milestone/7)

Fokus: Größere Architekturänderungen – eigener Release-Zyklus.

| Issue | Thema | Aufwand |
|-------|-------|---------|
| [#53](https://github.com/tom4711/finanzuebersicht/issues/53) | Optionale lokale Verschlüsselung (PBKDF2/Argon2) | L |
| [#49](https://github.com/tom4711/finanzuebersicht/issues/49) | Multi-Account & Währungsunterstützung | XL |

---

## 💡 Ideen / Nicht eingeplant

Features, die diskutiert wurden aber noch keinen Milestone haben:

- CloudKit-Sync (erfordert Apple Developer Membership)
- Widget für iOS/macOS
- Wiederkehrende Transaktionen mit variablem Betrag
- Kategorien-Hierarchie (Ober-/Unterkategorien)

---

*Versionsschema: [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) – Patch-Version = Git-Commit-Höhe*
