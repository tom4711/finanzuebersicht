# Kurzanleitung — Finanzübersicht

Diese kurze Anleitung hilft beim lokalen Aufbau, Testen und Entwickeln der App.

1) Repository klonen

```bash
git clone https://github.com/tom4711/finanzuebersicht.git
cd finanzuebersicht
dotnet restore
```

2) Voraussetzungen

- [.NET 10 SDK]
- Auf macOS: Xcode für iOS/Mac Catalyst Builds
- Empfohlen: `dotnet workload install maui`

3) Lokal starten (Mac Catalyst)

```bash
dotnet build Finanzuebersicht/Finanzuebersicht.csproj -f net10.0-maccatalyst
cp -R "Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/Finanzuebersicht.app" "/Applications/Finanzuebersicht.app"
open "/Applications/Finanzuebersicht.app"
```

4) Tests

```bash
dotnet test Finanzuebersicht.Tests
```

5) Wichtige Pfade & Ressourcen

- Lokale Daten: Einstellung `DataPath` (Settings)
- Kategorisierungs-Regeln: `Finanzuebersicht.Core/Data/categorization-rules.json` (siehe [docs/CATEGORIZATION_RULES.md](CATEGORIZATION_RULES.md))
- UI‑Texte: `Finanzuebersicht/Resources/Strings/AppResources.resx` (+ `.de.resx`)

6) Entwicklungshinweise

- Architektur: MVVM (CommunityToolkit.Mvvm); Services via DI in `MauiProgram.cs` registriert.
- Änderungen an `main` nur per Pull Request; arbeite auf `feature/*`, `fix/*` oder `chore/*`.
- Nutze `IDialogService` für Dialoge und ResX (`ILocalizationService`) für Texte in ViewModels.

7) Backups & Restore

- Backup/Restore-Funktionen sind in den Einstellungen verfügbar (Backup-Ordner, Export CSV).

8) Fragen & Mitwirken

- Öffne Issues für Bugs oder Feature-Requests.
- PR-Format: Gitmoji + Conventional Commits (siehe `.github/copilot-instructions.md`).

Viel Erfolg — sag Bescheid, wenn du diese Anleitung noch erweitern oder um Beispiele ergänzen möchtest.
