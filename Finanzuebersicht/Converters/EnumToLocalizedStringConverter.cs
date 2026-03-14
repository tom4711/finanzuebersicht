using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Converters;

public class EnumToLocalizedStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RecurrenceInterval ri)
        {
            // German labels; adjust if localization system is added
            return ri switch
            {
                RecurrenceInterval.Daily => "Täglich",
                RecurrenceInterval.Weekly => "Wöchentlich",
                RecurrenceInterval.Monthly => "Monatlich",
                RecurrenceInterval.Quarterly => "Quartal",
                RecurrenceInterval.Yearly => "Jährlich",
                _ => ri.ToString()
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            var key = s.Trim().ToLowerInvariant();
            if (key.Contains("täg")) return RecurrenceInterval.Daily;
            if (key.Contains("wöch")) return RecurrenceInterval.Weekly;
            if (key.Contains("monat")) return RecurrenceInterval.Monthly;
            if (key.Contains("quart")) return RecurrenceInterval.Quarterly;
            if (key.Contains("jahr")) return RecurrenceInterval.Yearly;
        }
        return RecurrenceInterval.Monthly;
    }
}
