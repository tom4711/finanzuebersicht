# Daueraufträge — Instanz verschieben

Kurzbeschreibung der UI-Funktion zum Verschieben einzelner Instanzen eines Dauerauftrags.

## Betroffene Dateien

| Rolle | Pfad |
|-------|------|
| Detailseite | `Finanzuebersicht/Views/RecurringTransactionDetailPage.xaml` |
| Detail-ViewModel | `Finanzuebersicht.Presentation/ViewModels/RecurringTransactionDetailViewModel.cs` |
| Shift-Seite | `Finanzuebersicht/Views/RecurringInstanceShiftPage.xaml` |
| Shift-ViewModel | `Finanzuebersicht.Presentation/ViewModels/RecurringInstanceShiftViewModel.cs` |
| Use Case | `Finanzuebersicht.Application/UseCases/RecurringTransactions/ShiftRecurringInstanceUseCase.cs` |

## Manuell testen

1. Projekt bauen:

```bash
dotnet build Finanzuebersicht/Finanzuebersicht.csproj -f net10.0-maccatalyst
```

2. App installieren und starten:

```bash
cp -R Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-arm64/Finanzübersicht.app /Applications/
open /Applications/Finanzübersicht.app
```

3. In der App: **Daueraufträge** → Dauerauftrag auswählen → **Instanz verschieben**
   - Formular zeigt Originaldatum, neues Datum und optionale Notiz
   - Speichern ruft `ShiftRecurringInstanceUseCase` auf und persistiert eine `RecurringException` vom Typ `Shift`

## Entwickler-Hinweise

- **Navigation:** Route `RecurringInstanceShift` (`Routes.cs`); Parameter: `RecurringId`, `InstanceDate`
- **Weitere Aktionen** auf der Detailseite: nächste Instanz überspringen (`Skip`), Ausnahme anlegen
- **Tests:** `ShiftRecurringInstanceUseCaseTests`, `RecurringGenerationServiceTests` in `Finanzuebersicht.Tests`

## Weitere Dauerauftrag-Features

- Automatische Instanz-Generierung bei App-Start und Window-Resume
- Fällige Daueraufträge im Dashboard (buchen / überspringen / verschieben)
- Cashflow-Vorschau berücksichtigt geplante Daueraufträge
