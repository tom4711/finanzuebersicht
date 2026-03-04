<div align="center">

# Finanzübersicht

**Persönliche Finanzverwaltung für iOS, macOS und Windows**

[![CI](https://github.com/tom4711/finanzuebersicht/actions/workflows/ci.yml/badge.svg)](https://github.com/tom4711/finanzuebersicht/actions/workflows/ci.yml)
[![Pre-Release](https://github.com/tom4711/finanzuebersicht/actions/workflows/prerelease.yml/badge.svg)](https://github.com/tom4711/finanzuebersicht/actions/workflows/prerelease.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-blueviolet)](https://dotnet.microsoft.com/download)
[![Platform](https://img.shields.io/badge/platform-iOS%20%7C%20macOS%20%7C%20Windows-lightgrey)](#plattformen)

</div>

---

Finanzübersicht ist eine Open-Source-App zur einfachen Verwaltung von Einnahmen, Ausgaben und wiederkehrenden Buchungen – lokal auf deinem Gerät, ohne Cloud-Zwang.

## Features

- 📊 **Dashboard** – Monatliche Übersicht über Einnahmen und Ausgaben
- 💸 **Transaktionen** – Buchungen erfassen, bearbeiten und filtern
- 🔁 **Wiederkehrende Buchungen** – Miete, Abos & Co. einmalig anlegen
- 🏷️ **Kategorien** – Eigene Kategorien mit Farbe und Icon
- 🌙 **Dark Mode** – Vollständige Unterstützung für Light & Dark Mode
- 🌐 **Mehrsprachigkeit** – Deutsch und Englisch, live umschaltbar in den Einstellungen

## Plattformen

| Plattform | Status    |
|-----------|-----------|
| macOS     | ✅ Stabil |
| iOS       | ✅ Stabil |
| Windows   | ✅ Stabil |

## Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- macOS mit Xcode (für iOS/macOS Build)
- Windows 10/11 mit Visual Studio 2022 (für Windows Build)

```bash
dotnet workload install maui
```

## Getting Started

```bash
git clone https://github.com/tom4711/finanzuebersicht.git
cd finanzuebersicht
dotnet restore
```

**App starten (macOS):**
```bash
# Build
dotnet build Finanzuebersicht/Finanzuebersicht.csproj -f net10.0-maccatalyst

# Anschließend .app nach /Applications kopieren und starten
cp -R "Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/Finanzübersicht.app" "/Applications/Finanzübersicht.app"
open "/Applications/Finanzübersicht.app"

# Tests
dotnet test Finanzuebersicht.Tests
```

## Tech Stack

| Bereich | Technologie |
|---------|-------------|
| Framework | [.NET 10 MAUI](https://github.com/dotnet/maui) |
| Architektur | MVVM via [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) |
| Persistenz | JSON-Dateien (lokal, kein Cloud-Zwang) |
| Versionierung | [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) |

## Architekturstatus

- Phase 1 und Phase 2 der Architektur-Roadmap sind abgeschlossen (Stand: März 2026).
- Die UI nutzt UseCases/Contracts; Infrastructure-Registrierung ist im Infrastructure-Layer gekapselt.

## Architektur-Roadmap

Eine konkrete Zielarchitektur inkl. inkrementellem Migrationspfad findest du in:

- [docs/ARCHITEKTUR-ROADMAP.md](docs/ARCHITEKTUR-ROADMAP.md)

## Datenverhalten

- Beim Löschen einer Kategorie bleiben bestehende Transaktionen und Daueraufträge erhalten.
- Referenzen werden automatisch auf die Fallback-Kategorie „Sonstiges“ umgehängt.

## Builds & Downloads (GitHub)

- **Pre-Releases** enthalten direkt herunterladbare ZIP-Builds für **macOS (Mac Catalyst)** und **Windows**.
- Du findest sie unter [Releases](https://github.com/tom4711/finanzuebersicht/releases) (als `Pre-release` markiert).
- Die Versionierung erfolgt weiterhin über Nerdbank.GitVersioning (`0.x` vor `1.0.0`).

## CI/CD Workflows

- `CI` ([.github/workflows/ci.yml](.github/workflows/ci.yml))
	- Führt Tests aus und erzeugt Build-Artefakte für macOS/Windows.
- `Pre-Release` ([.github/workflows/prerelease.yml](.github/workflows/prerelease.yml))
	- Baut macOS/Windows in `Release`, verpackt ZIPs und veröffentlicht sie als GitHub Pre-Release.
	- Trigger: Push auf `main` oder manuell via `workflow_dispatch`.

## Contributing

Pull Requests sind willkommen! Bitte erstelle zuerst einen Branch:

```bash
git checkout -b feature/mein-feature
```

Alle Änderungen an `main` müssen über einen Pull Request laufen – direkte Pushes sind gesperrt.

## Lizenz

[GPL v3](LICENSE) © Thomas Menzl
