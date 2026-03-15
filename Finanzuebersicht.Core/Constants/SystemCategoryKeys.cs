namespace Finanzuebersicht.Core.Constants
{
    /// <summary>
    /// Zentrale Schlüssel für System- und Fallback-Kategorien zur Vermeidung von Magic Strings.
    /// Neue Schlüssel hier als öffentliche Konstanten hinzufügen.
    /// </summary>
    public static class SystemCategoryKeys
    {
        // System- / Fallback-Kategorie-Schlüssel
        public const string Unkategorisiert = "SysCat_Unkategorisiert";
        public const string Sonstiges = "SysCat_Sonstiges";
        public const string Lebensmittel = "SysCat_Lebensmittel";
        public const string Transport = "SysCat_Transport";
        public const string Wohnen = "SysCat_Wohnen";
        public const string Unterhaltung = "SysCat_Unterhaltung";
        public const string Gesundheit = "SysCat_Gesundheit";
        public const string Gehalt = "SysCat_Gehalt";

        // Weitere Schlüssel bei Bedarf hinzufügen, z.B.:
        // public const string Unassigned = "SysCat_Unassigned";
    }
}
