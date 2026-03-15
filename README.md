<div align="center">

# Finanzübersicht

**Persönliche Finanzverwaltung für iOS und macOS (Mac Catalyst)**

[![CI](https://github.com/tom4711/finanzuebersicht/actions/workflows/ci.yml/badge.svg)](https://github.com/tom4711/finanzuebersicht/actions/workflows/ci.yml)
[![Pre-Release](https://github.com/tom4711/finanzuebersicht/actions/workflows/prerelease.yml/badge.svg)](https://github.com/tom4711/finanzuebersicht/actions/workflows/prerelease.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-blueviolet)](https://dotnet.microsoft.com/download)

</div>

Finanzübersicht ist eine Open‑Source App zur lokalen Verwaltung von Einnahmen, Ausgaben und wiederkehrenden Buchungen. Die App speichert Daten lokal als JSON; Cloud‑Funktionen (z. B. CloudKit) sind vorhanden, aber optional und können weitere Voraussetzungen (Apple Developer Account) erfordern.

Kurz: .NET 10 + MAUI, deutschsprachige UI, MVVM-Architektur.

## Kern-Features

- Dashboard mit Monatsübersicht
- Transaktionen anlegen, editieren und filtern
- Wiederkehrende Buchungen (Daueraufträge)
- Kategorien mit Icon und Farbe
- Dark Mode Unterstützung

Hinweis: Die Benutzeroberfläche ist primär auf Deutsch ausgelegt.

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

## Projektstruktur (Kurz)

- `Finanzuebersicht/` – MAUI App (Views, ViewModels, Converters, Resources)
- `Finanzuebersicht.Core/` – Geschäftslogik, Modelle, Services
- `Finanzuebersicht.Infrastructure/` – DI-Registrierung, Infrastrukturdienste
- `Finanzuebersicht.Tests/` – Unit-Tests (xUnit)

## Lokalisierung & Ressourcen

- UI‑Texte werden über Ressourcenfiles (`Resources/Strings/AppResources.resx`) verwaltet.
- Die App ist primär auf Deutsch ausgerichtet; Übersetzungen sind als ResX vorhanden.

## Dokumentation

- Kategorisierungs-Regeln: [docs/CATEGORIZATION_RULES.md](docs/CATEGORIZATION_RULES.md)
- (Einige alte Docs wurden entfernt oder zusammengeführt.)

## Versionierung & CI

- Nerdbank.GitVersioning (`version.json`) steuert Versionsnummern
- CI / Pre-Release Workflows in `.github/workflows/`

## Mitmachen

Bitte arbeite auf Feature-Branches und eröffne Pull Requests gegen `main`:

```bash
git checkout -b feature/mein-feature
```

`main` ist geschützt; PRs werden geprüft bevor gemerged wird.

## Lizenz

GPL v3 — siehe LICENSE
