<div align="center">

# Finanzübersicht

**Persönliche Finanzverwaltung für iOS und macOS (Mac Catalyst)**

[![CI](https://github.com/tom4711/finanzuebersicht/actions/workflows/ci.yml/badge.svg)](https://github.com/tom4711/finanzuebersicht/actions/workflows/ci.yml)
[![Pre-Release](https://github.com/tom4711/finanzuebersicht/actions/workflows/prerelease.yml/badge.svg)](https://github.com/tom4711/finanzuebersicht/actions/workflows/prerelease.yml)
[![Release](https://github.com/tom4711/finanzuebersicht/actions/workflows/release.yml/badge.svg)](https://github.com/tom4711/finanzuebersicht/actions/workflows/release.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-blueviolet)](https://dotnet.microsoft.com/download)

</div>

Finanzübersicht ist eine Open‑Source App zur lokalen Verwaltung von Einnahmen, Ausgaben und wiederkehrenden Buchungen. Die App speichert Daten lokal als JSON; Cloud‑Funktionen (z. B. CloudKit) sind vorhanden, aber optional und können weitere Voraussetzungen (Apple Developer Account) erfordern.

Kurz: .NET 10 + MAUI, Multi-Language UI (Deutsch & Englisch), MVVM-Architektur.

## Kern-Features

- Dashboard mit Monatsübersicht und Trend-Indikator
- Transaktionen anlegen, editieren und filtern
- Wiederkehrende Buchungen (Daueraufträge)
- Kategorien mit Icon und Farbe
- **Monatsbudgets** pro Kategorie mit Fortschrittsanzeige
- **Sparziele** mit Fortschrittsbalken
- **Backup & Restore** mit automatischer Schema-Migration (v1 → v2+)
- Accessibility / VoiceOver-Unterstützung (iOS & macOS)
- Dark Mode Unterstützung
- Multi-Language Support (Deutsch & Englisch)

Hinweis: Die Benutzeroberfläche unterstützt Deutsch und Englisch; weitere Sprachen sind möglich, werden aber nicht aktiv gepflegt.

## Plattformen

| Plattform | Status |
|-----------|--------|
| macOS (Mac Catalyst) | ✅ Unterstützt |
| iOS | ✅ Unterstützt |
| Windows | ⚠️ Experimentell / eingeschränkt |

## Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- macOS (Xcode) für iOS / Mac Catalyst Builds
- Optional: Windows + Visual Studio für experimentelle Windows-Builds

Installiere benötigte Workloads:

```bash
dotnet workload install maui
```

## Schnellstart

### Für Deutsch-sprechende Entwickler

Siehe [Entwickler-Leitfaden (docs/GUIDE.md)](docs/GUIDE.md) für detaillierte Setup- und Entwicklungsanweisungen.

```bash
git clone https://github.com/tom4711/finanzuebersicht.git
cd finanzuebersicht
dotnet restore
```

Build & Start (Mac Catalyst):

```bash
dotnet build Finanzuebersicht/Finanzuebersicht.csproj -f net10.0-maccatalyst
cp -R "Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/Finanzübersicht.app" "/Applications/Finanzübersicht.app"
open "/Applications/Finanzübersicht.app"

# Tests
dotnet test Finanzuebersicht.Tests
```

### For English Speakers

See [Quick Start (docs/QUICK_START.md)](docs/QUICK_START.md) for a concise guide.

## Projektstruktur (Kurz)

- `Finanzuebersicht/` – MAUI App (Views, ViewModels, Converters, Resources)
- `Finanzuebersicht.Core/` – Geschäftslogik, Modelle, Services
- `Finanzuebersicht.Infrastructure/` – DI-Registrierung, Infrastrukturdienste
- `Finanzuebersicht.Tests/` – Unit-Tests (xUnit)

## Lokalisierung & Ressourcen

- UI‑Texte werden über Ressourcenfiles (`Resources/Strings/AppResources.resx`) verwaltet.
- Unterstützte Sprachen: Deutsch (Standard) und Englisch. Weitere Sprachen sind möglich, werden aber nicht aktiv gewartet.

## Dokumentation

- Roadmap & geplante Features: [docs/ROADMAP.md](docs/ROADMAP.md)
- Kategorisierungs-Regeln: [docs/CATEGORIZATION_RULES.md](docs/CATEGORIZATION_RULES.md)
- (Einige alte Docs wurden entfernt oder zusammengeführt.)

## Versionierung & CI

- Nerdbank.GitVersioning (`version.json`) steuert Versionsnummern
- CI / Pre-Release / Release Workflows in `.github/workflows/`

Full MAUI build (macCatalyst)

- Quick checks (unit tests, linters) werden bei Push auf Branches ausgeführt (ubuntu runner) — das spart macOS-Runner-Minuten.
- Der vollständige MAUI macCatalyst-Build läuft für Pull Requests gegen `main` und für `main`-Pushes. Dadurch werden macOS-Ressourcen nur bei echten Integrationsprüfungen verwendet.
- **Pre-Release:** Bei jedem Push auf `main` wird automatisch ein Beta-Release mit Artifacts erstellt.
- **Release:** Bei Tag-Push (`v*`) oder manuellem Trigger werden Build-Artifacts (macOS + Windows) an das GitHub Release gehängt.
- Um einen vollständigen MAUI-Build für einen PR explizit anzustoßen, füge das Label `run-maui` zur PR hinzu oder starte den Workflow manuell über Actions → "CI - Split quick and full" → Run workflow.

## Mitmachen

Bitte arbeite auf Feature-Branches und eröffne Pull Requests gegen `main`:

```bash
git checkout -b feature/mein-feature
```

`main` ist geschützt; PRs werden geprüft bevor gemerged wird.

## Lizenz

GPL v3 — siehe LICENSE
