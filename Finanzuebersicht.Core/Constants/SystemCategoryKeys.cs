using System;

namespace Finanzuebersicht.Core.Constants
{
    /// <summary>
    /// Centralized keys for system and fallback categories to avoid magic strings.
    /// Add new keys here as public constants.
    /// </summary>
    public static class SystemCategoryKeys
    {
        // System / fallback category keys
        public const string Unkategorisiert = "SysCat_Unkategorisiert";
        public const string Sonstiges = "SysCat_Sonstiges";
        public const string Lebensmittel = "SysCat_Lebensmittel";
        public const string Transport = "SysCat_Transport";
        public const string Wohnen = "SysCat_Wohnen";
        public const string Unterhaltung = "SysCat_Unterhaltung";
        public const string Gesundheit = "SysCat_Gesundheit";
        public const string Gehalt = "SysCat_Gehalt";

        // Add additional keys as needed, e.g.:
        // public const string Unassigned = "SysCat_Unassigned";
    }
}
