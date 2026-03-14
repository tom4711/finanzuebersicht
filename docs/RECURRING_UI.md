Kurzbeschreibung
---------------
Diese Notiz beschreibt die neu hinzugefügte UI-Funktion zum Verschieben einzelner Instanzen eines Dauerauftrags.

Wo die Änderung liegt
- Detailseite: [Finanzuebersicht/Views/RecurringTransactionDetailPage.xaml](Finanzuebersicht/Views/RecurringTransactionDetailPage.xaml)
- Detail-ViewModel: [Finanzuebersicht/ViewModels/RecurringTransactionDetailViewModel.cs](Finanzuebersicht/ViewModels/RecurringTransactionDetailViewModel.cs)
- Neue Shift-Seite: [Finanzuebersicht/Views/RecurringInstanceShiftPage.xaml](Finanzuebersicht/Views/RecurringInstanceShiftPage.xaml)
- Shift-ViewModel: [Finanzuebersicht/ViewModels/RecurringInstanceShiftViewModel.cs](Finanzuebersicht/ViewModels/RecurringInstanceShiftViewModel.cs)

Wie man die Funktion benutzt (manuell testen)
1. Projekt wie gewohnt bauen:

```bash
dotnet build -f net10.0-maccatalyst
```

2. App für Mac Catalyst installieren und starten (siehe .github/copilot-instructions.md für Hinweise):

```bash
cp -R Finanzuebersicht/bin/Debug/net10.0-maccatalyst/maccatalyst-x64/Finanzuebersicht.app /Applications/
open /Applications/Finanzuebersicht.app
```

3. In der App: "Daueraufträge" öffnen → Dauerauftrag auswählen → "Instanz verschieben" drücken.
   - Das Formular zeigt das Originaldatum, erlaubt ein neues Datum und eine optionale Notiz.
   - Speichern ruft `ShiftRecurringInstanceUseCase` auf und persistiert eine `RecurringException` vom Typ `Shift`.

Anmerkungen für Entwickler
- Navigation: die Seite wird über DI-registrierte Route `RecurringInstanceShiftPage` geöffnet. Die Übergabeparameter sind `RecurringId` und `InstanceDate`.
- Die Detailseite bietet außerdem Buttons zum Überspringen der nächsten Instanz (`Skip`) und zum direkten Anlegen einer Ausnahme.
- Unit-Tests: bestehende Tests wurden ergänzt und neue Tests hinzugefügt (z. B. `RecurringGenerationServiceTests`); alle Tests laufen lokal (siehe `Finanzuebersicht.Tests`).

Weiteres: Wenn ihr UI-Tests (z. B. Appium / WinAppDriver) hinzufügen wollt, empfehle ich einen separaten kleinen Test-Plan für macCatalyst oder iOS-Simulator, da MAUI UI-Tests plattformabhängig sind.
