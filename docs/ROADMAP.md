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
