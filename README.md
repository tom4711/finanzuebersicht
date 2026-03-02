<div align="center">

# Finanzübersicht

**Persönliche Finanzverwaltung für iOS und macOS**

[![CI](https://github.com/tom4711/finanzuebersicht/actions/workflows/ci.yml/badge.svg)](https://github.com/tom4711/finanzuebersicht/actions/workflows/ci.yml)
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

## Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- macOS mit Xcode (für iOS/macOS Build)

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

# Anschließend die .app aus dem bin/-Ordner nach /Applications/ kopieren und dort starten
# (macOS startet die App korrekt aus /Applications/)

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

## Contributing

Pull Requests sind willkommen! Bitte erstelle zuerst einen Branch:

```bash
git checkout -b feature/mein-feature
```

Alle Änderungen an `main` müssen über einen Pull Request laufen – direkte Pushes sind gesperrt.

## Lizenz

[GPL v3](LICENSE) © Thomas Menzl
