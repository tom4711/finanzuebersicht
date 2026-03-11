# Kategorisierungs-Regeln

Die automatische Kategorisierung von Transaktionen basiert auf einem **Regelwerk**, das du anpassen kannst.

## 📍 Regelwerk-Datei

```
Finanzuebersicht.Core/Data/categorization-rules.json
```

## 📋 Format

Das Regelwerk ist eine **JSON-Datei** mit folgender Struktur:

```json
{
  "rules": [
    {
      "categoryName": "Lebensmittel",
      "patterns": [
        "REWE",
        "EDEKA",
        "KAUFLAND",
        "ALDI"
      ],
      "description": "Grocery stores and supermarkets"
    }
  ]
}
```

## 🔍 Wie es funktioniert

1. **Beim CSV-Import** prüft die App jeden Transaktionseintrag
2. **Für jeden Pattern** in den Regeln wird nach **Wort-Grenzen** gesucht (Case-Insensitive)
3. **Erste Übereinstimmung gewinnt** – beim ersten Match wird die entsprechende Kategorie zugewiesen
4. **Fallback**: Wenn kein Pattern passt → Kategorie "Unkategorisiert"

### Beispiel:
- Pattern: `"REWE"` → Findet ✅ "REWE MARKT GMBH", "REWE-ZENTRAL", "REWE 12345"
- Pattern: `"APOTHEKE"` → Findet ✅ "EBER APOTHEKE", "AM-APOTHEKE", "APOTHEKE AM MARKT"

## ✏️ Regeln anpassen

### 1. Pattern hinzufügen
Öffne `categorization-rules.json` und ergänze den entsprechenden `patterns`-Array:

```json
{
  "categoryName": "Freizeit",
  "patterns": [
    "KINO",
    "NETFLIX",
    "SPOTIFY",
    "STEAM",
    "CINEMA"  // ← Neu hinzugefügt
  ]
}
```

### 2. Neue Kategorie hinzufügen
Falls du eine neue Kategorie brauchst, ergänze ein neues Objekt:

```json
{
  "categoryName": "Versicherung",
  "patterns": [
    "ALLIANZ",
    "AXA",
    "DEBEKA",
    "TECHNIKER KRANKENKASSE"
  ],
  "description": "Insurance payments"
}
```

### 3. Kategorie umbenennen
⚠️ **Wichtig**: Der `categoryName` muss **exakt** mit einer existierenden Kategorie in der App übereinstimmen. Die Standard-Kategorien sind:
- Lebensmittel
- Transport
- Sonstiges
- Wohnen
- Gesundheit
- Freizeit
- Unkategorisiert (System-Kategorie)

## 🧪 Testen

Nach dem Ändern der Regeln:

1. **Datei speichern**
2. **App neu starten** (oder CSV erneut importieren)
3. **Neu importierte Transaktionen** werden nach den neuen Regeln kategorisiert

## 💡 Best Practices

### Gute Patterns:
- ✅ `"REWE"` – Häufiges Wort, einfache Erkennung
- ✅ `"DEUTSCHE BAHN"` – Spezifische Phrase
- ✅ `"SPOTIFY"` – Eindeutiger Service-Name

### Zu vermeiden:
- ❌ `"E"` – Zu generisch, viele False Positives
- ❌ `"BANK"` – Wird in vielen Namen vorkommen
- ❌ `"GmbH"` – Zu allgemein

### Tipps:
- Nutze **aussagekräftige Begriffe** aus Zahlungsempfängernamen
- **Testlauf**: Import mit Test-Transaktionen machen
- **Priorität beachten**: Strategien werden in dieser Reihenfolge geprüft:
  1. **Keyword-Muster** (Pattern Matching aus dieser Datei)
  2. **Historischer Verlauf** (Was wurde bei diesem Zahlungsempfänger früher kategorisiert?)
  3. **Fallback**: "Unkategorisiert"

## 🔧 Strategien

Die App nutzt **2 Strategien** zur Auto-Kategorisierung:

### 1. Keyword Pattern Matching (Priority: 10)
- Nutzt dieses Regelwerk (`categorization-rules.json`)
- Regex-basiert mit Wort-Grenzen
- Läuft zuerst

### 2. Historical Category Matching (Priority: 20)
- Merkt sich, welche Kategorien du manuell für einen Zahlungsempfänger gewählt hast
- Falls z.B. "REWE" nie einen Match findet, aber du hast früher Transaktionen von "REWE" als "Lebensmittel" kategorisiert → wird wieder als "Lebensmittel" erkannt
- Läuft nach Keyword-Matching

## 📝 Beispiel: Weitere Patterns für deine CSV

Basierend auf den Transaktionen aus deiner Test-CSV:

```json
{
  "categoryName": "Freizeit",
  "patterns": [
    "GASTHOF",
    "RESTAURANT"
  ]
},
{
  "categoryName": "Sonstiges",
  "patterns": [
    "APPLE.COM",
    "POSTCODE LOTTERIE"
  ]
},
{
  "categoryName": "Gesundheit",
  "patterns": [
    "OPTIK"
  ]
}
```

---

**Viel Erfolg beim Anpassen der Regeln!** 🚀
