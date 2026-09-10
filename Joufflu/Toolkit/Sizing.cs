using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Toolkit;

public enum ControlSize { xs, sm, md, lg, xl }

public static class Sizing
{
    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.RegisterAttached(
            "Size",
            typeof(ControlSize),
            typeof(Sizing),
            new FrameworkPropertyMetadata(ControlSize.md, FrameworkPropertyMetadataOptions.Inherits));

    public static ControlSize GetSize(DependencyObject obj) => (ControlSize)obj.GetValue(SizeProperty);
    public static void SetSize(DependencyObject obj, ControlSize value) => obj.SetValue(SizeProperty, value);

    public static readonly DependencyProperty IsSquareProperty =
        DependencyProperty.RegisterAttached(
            "IsSquare",
            typeof(bool),
            typeof(Sizing),
            new FrameworkPropertyMetadata(false));

    public static bool GetIsSquare(DependencyObject obj) => (bool)obj.GetValue(IsSquareProperty);
    public static void SetIsSquare(DependencyObject obj, bool value) => obj.SetValue(IsSquareProperty, value);
}

/// <summary>
/// Attached property that adds a gap between the children of a <see cref="Panel"/>
/// (<see cref="StackPanel"/>, <see cref="WrapPanel"/> and <see cref="Grid"/>) by
/// applying margins to those children.
/// <para>
/// <see cref="GapProperty"/> takes a <see cref="Thickness"/>, so you can write a single
/// value for a uniform gap on both axes (<c>"8"</c>), or use the horizontal/vertical pair
/// to set each axis independently (<c>"8,0"</c> = 8 horizontal / 0 vertical). The gap's
/// <see cref="Thickness.Left"/> drives the horizontal axis and <see cref="Thickness.Top"/>
/// the vertical one; the right/bottom components are ignored.
/// </para>
/// <para>
/// Because spacing is implemented through child margins, any margin set directly on a
/// child is overwritten while spacing is active.
/// </para>
/// </summary>
public static class Spacing
{
    /// <summary>Gap (in DIPs) applied between children; Left is the horizontal gap and Top the vertical gap.</summary>
    public static readonly DependencyProperty GapProperty =
        DependencyProperty.RegisterAttached(
            "Gap",
            typeof(Thickness),
            typeof(Spacing),
            new FrameworkPropertyMetadata(new Thickness(), OnGapChanged));

    public static Thickness GetGap(DependencyObject obj) => (Thickness)obj.GetValue(GapProperty);
    public static void SetGap(DependencyObject obj, Thickness value) => obj.SetValue(GapProperty, value);

    // Holds the per-panel watcher so its LayoutUpdated handler stays removable.
    private static readonly DependencyProperty WatcherProperty =
        DependencyProperty.RegisterAttached(
            "Watcher",
            typeof(SpacingWatcher),
            typeof(Spacing),
            new PropertyMetadata(null));

    private static void OnGapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Panel panel)
            return;

        var watcher = (SpacingWatcher?)panel.GetValue(WatcherProperty);
        if (watcher is null)
        {
            watcher = new SpacingWatcher(panel);
            panel.SetValue(WatcherProperty, watcher);
        }

        watcher.Apply();
    }

    /// <summary>
    /// Watches a single panel and re-applies spacing when its children change.
    /// <para>
    /// Dynamically added/removed children (e.g. an ItemsControl's generated items) are
    /// caught through <see cref="UIElement.LayoutUpdated"/>, but the handler only does work
    /// when the child count actually changes — on every other layout pass it is a single
    /// integer comparison, so spacing no longer costs a full margin sweep per layout pass.
    /// </para>
    /// </summary>
    private sealed class SpacingWatcher
    {
        private readonly Panel _panel;
        private int _lastChildCount = -1;

        public SpacingWatcher(Panel panel)
        {
            _panel = panel;
            _panel.LayoutUpdated += OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            if (_panel.Children.Count == _lastChildCount)
                return;
            Apply();
        }

        public void Apply()
        {
            _lastChildCount = _panel.Children.Count;
            ApplySpacing(_panel);
        }
    }

    private static void ApplySpacing(Panel panel)
    {
        // Left drives the horizontal gap, Top the vertical one (NaN falls back to 0).
        Thickness gap = GetGap(panel);
        double horizontal = double.IsNaN(gap.Left) ? 0 : gap.Left;
        double vertical = double.IsNaN(gap.Top) ? 0 : gap.Top;

        switch (panel)
        {
            case Grid grid:
                ApplyToGrid(grid, horizontal, vertical);
                break;
            case StackPanel stack:
                ApplyToStack(stack, horizontal, vertical);
                break;
            default: // WrapPanel and any other Panel
                ApplyUniform(panel, horizontal, vertical);
                break;
        }
    }

    // StackPanel is one-dimensional: gap every child except the first, on the leading
    // edge of the stacking axis. This leaves no outer margin on the panel.
    private static void ApplyToStack(StackPanel stack, double horizontal, double vertical)
    {
        bool isVertical = stack.Orientation == Orientation.Vertical;
        for (int i = 0; i < stack.Children.Count; i++)
        {
            if (stack.Children[i] is not FrameworkElement child)
                continue;

            double gap = i == 0 ? 0 : (isVertical ? vertical : horizontal);
            SetMargin(child, isVertical ? new Thickness(0, gap, 0, 0) : new Thickness(gap, 0, 0, 0));
        }
    }

    // Grid gaps are driven by each child's Grid.Row / Grid.Column: anything past the
    // first row/column gets a leading margin, so gaps sit only between cells.
    private static void ApplyToGrid(Grid grid, double horizontal, double vertical)
    {
        foreach (UIElement element in grid.Children)
        {
            if (element is not FrameworkElement child)
                continue;

            double left = Grid.GetColumn(child) > 0 ? horizontal : 0;
            double top = Grid.GetRow(child) > 0 ? vertical : 0;
            SetMargin(child, new Thickness(left, top, 0, 0));
        }
    }

    // WrapPanel (and other panels) wrap in both directions, so line breaks are not known
    // up front. Apply a trailing margin on both axes to every child; gaps between items
    // are exact, at the cost of a trailing margin on the panel's edges.
    private static void ApplyUniform(Panel panel, double horizontal, double vertical)
    {
        foreach (UIElement element in panel.Children)
        {
            if (element is FrameworkElement child)
                SetMargin(child, new Thickness(0, 0, horizontal, vertical));
        }
    }

    private static void SetMargin(FrameworkElement child, Thickness margin)
    {
        if (child.Margin != margin)
            child.Margin = margin;
    }
}