#if MACCATALYST || IOS
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
#endif

namespace Finanzuebersicht.Controls;

/// <summary>
/// Opt-in immediate date binding for form DatePickers that are saved via a button
/// (instead of relying on the picker "Done" action with <see cref="UpdateMode.WhenFinished"/>).
/// </summary>
public static class DatePickerProperties
{
    public static readonly BindableProperty ImmediateUpdateProperty =
        BindableProperty.CreateAttached(
            "ImmediateUpdate",
            typeof(bool),
            typeof(DatePickerProperties),
            false,
            propertyChanged: OnImmediateUpdateChanged);

    public static bool GetImmediateUpdate(BindableObject view)
        => (bool)view.GetValue(ImmediateUpdateProperty);

    public static void SetImmediateUpdate(BindableObject view, bool value)
        => view.SetValue(ImmediateUpdateProperty, value);

    private static void OnImmediateUpdateChanged(BindableObject bindable, object oldValue, object newValue)
    {
#if MACCATALYST || IOS
        if (bindable is Microsoft.Maui.Controls.DatePicker datePicker)
            ApplyUpdateMode(datePicker, (bool)newValue);
#endif
    }

#if MACCATALYST || IOS
    internal static void ApplyUpdateMode(Microsoft.Maui.Controls.DatePicker datePicker)
        => ApplyUpdateMode(datePicker, GetImmediateUpdate(datePicker));

    private static void ApplyUpdateMode(Microsoft.Maui.Controls.DatePicker datePicker, bool immediate)
    {
        datePicker.On<iOS>().SetUpdateMode(
            immediate ? UpdateMode.Immediately : UpdateMode.WhenFinished);
    }
#endif
}
