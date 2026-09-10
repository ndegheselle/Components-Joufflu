using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Joufflu.Toolkit;

/// <summary>Side of the target element a tooltip is placed on.</summary>
public enum TooltipPlacement { Top, Bottom, Left, Right }

/// <summary>
/// Attaches a themed <see cref="ToolTip"/> to any element from a single pair of properties:
/// set <see cref="ContentProperty"/> to the content to show (a string or arbitrary XAML) and,
/// optionally, <see cref="PlacementProperty"/> to the side it sits on.
/// <para>
/// The native <see cref="ToolTip"/> is reused under the hood, so hover delay, fade-in and
/// screen-edge flipping all keep working; this just wires it up and applies the placement.
/// </para>
/// </summary>
public static class Tooltip
{
    /// <summary>Content shown in the tooltip. <c>null</c> removes the tooltip.</summary>
    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.RegisterAttached(
            "Content",
            typeof(object),
            typeof(Tooltip),
            new PropertyMetadata(null, OnChanged));

    public static object? GetContent(DependencyObject obj) => obj.GetValue(ContentProperty);
    public static void SetContent(DependencyObject obj, object? value) => obj.SetValue(ContentProperty, value);

    /// <summary>Side of the element the tooltip is placed on (defaults to <see cref="TooltipPlacement.Top"/>).</summary>
    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.RegisterAttached(
            "Placement",
            typeof(TooltipPlacement),
            typeof(Tooltip),
            new PropertyMetadata(TooltipPlacement.Bottom, OnChanged));

    public static TooltipPlacement GetPlacement(DependencyObject obj) => (TooltipPlacement)obj.GetValue(PlacementProperty);
    public static void SetPlacement(DependencyObject obj, TooltipPlacement value) => obj.SetValue(PlacementProperty, value);

    /// <summary>Gap in DIPs kept between the element and the tooltip.</summary>
    private const double Gap = 2;

    // Content and Placement can be set in any order in XAML, so both handlers funnel here
    // and (re)build the tooltip from whatever the current values are.
    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => Apply(d);

    private static void Apply(DependencyObject d)
    {
        object? content = GetContent(d);
        if (content is null)
        {
            ToolTipService.SetToolTip(d, null);
            return;
        }

        // Show instantly, and stay instant on subsequent hovers (native default is ~400ms / 100ms).
        ToolTipService.SetInitialShowDelay(d, 0);
        ToolTipService.SetBetweenShowDelay(d, 0);

        if (ToolTipService.GetToolTip(d) is not ToolTip toolTip)
        {
            toolTip = new ToolTip();
            // Center/space the tooltip once it is measured — sizes aren't known before it opens.
            toolTip.Opened += OnToolTipOpened;
            ToolTipService.SetToolTip(d, toolTip);
        }

        toolTip.Content = content;
        toolTip.Placement = ToPlacementMode(GetPlacement(d));
    }

    // PlacementMode.Top/Bottom left-align the tooltip to the target, and none of the modes leave
    // a gap. Once the tooltip is open we know both sizes, so we translate it into place: center it
    // on the shared edge and push it Gap DIPs clear of the element. Popup offsets are screen-space
    // (positive = right / down) and update live, so setting them here repositions the open tooltip.
    private static void OnToolTipOpened(object sender, RoutedEventArgs e)
    {
        if (sender is not ToolTip toolTip || toolTip.PlacementTarget is not FrameworkElement target)
            return;

        double dx = (target.ActualWidth - toolTip.ActualWidth) / 2;
        double dy = (target.ActualHeight - toolTip.ActualHeight) / 2;

        switch (GetPlacement(target))
        {
            case TooltipPlacement.Top:
                toolTip.HorizontalOffset = dx;
                toolTip.VerticalOffset = -Gap;
                break;
            case TooltipPlacement.Bottom:
                toolTip.HorizontalOffset = dx;
                toolTip.VerticalOffset = Gap;
                break;
            case TooltipPlacement.Left:
                toolTip.HorizontalOffset = -Gap;
                toolTip.VerticalOffset = dy;
                break;
            case TooltipPlacement.Right:
                toolTip.HorizontalOffset = Gap;
                toolTip.VerticalOffset = dy;
                break;
        }
    }

    private static PlacementMode ToPlacementMode(TooltipPlacement placement) => placement switch
    {
        TooltipPlacement.Bottom => PlacementMode.Bottom,
        TooltipPlacement.Left => PlacementMode.Left,
        TooltipPlacement.Right => PlacementMode.Right,
        _ => PlacementMode.Top,
    };
}
