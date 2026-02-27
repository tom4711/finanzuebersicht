#if MACCATALYST || IOS
using CoreAnimation;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using UIKit;

namespace Finanzuebersicht.Handlers;

/// <summary>
/// Custom Shell renderer that speeds up tab switch animations
/// for a snappier feel on Mac Catalyst and iOS.
/// </summary>
public class FastShellRenderer : ShellRenderer
{
    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
    {
        return new FastShellItemRenderer(this)
        {
            ShellItem = shellItem
        };
    }
}

public class FastShellItemRenderer : ShellItemRenderer
{
    public FastShellItemRenderer(IShellContext context) : base(context) { }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Intercept tab selection to speed up the transition
        ViewControllerSelected += OnTabSelected;
    }

    private void OnTabSelected(object? sender, UITabBarSelectionEventArgs e)
    {
        // Speed up any pending animations to 80ms
        foreach (var child in View?.Subviews ?? [])
        {
            foreach (var sub in child.Subviews)
            {
                sub.Layer.Speed = 4.0f;
                CATransaction.Begin();
                CATransaction.AnimationDuration = 0.08;
                CATransaction.Commit();
            }
        }

        // Reset speed after a brief delay
        NSTimer.CreateScheduledTimer(0.3, _ =>
        {
            foreach (var child in View?.Subviews ?? [])
            {
                foreach (var sub in child.Subviews)
                {
                    sub.Layer.Speed = 1.0f;
                }
            }
        });
    }
}
#endif
