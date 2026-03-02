using Finanzuebersicht.Services;
using Microsoft.Maui.Controls.Xaml;

namespace Finanzuebersicht.Extensions;

/// <summary>
/// XAML Markup Extension für Lokalisierung.
/// Verwendung: Text="{loc:Translate Nav_Transaktionen}"
/// Aktualisiert sich automatisch bei Sprachwechsel via LocalizationResourceManager.
/// </summary>
[ContentProperty(nameof(Key))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
        => new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Key}]",
            Source = LocalizationResourceManager.Current
        };

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        => ProvideValue(serviceProvider);
}
