using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Samples.Controls;

/// <summary>
/// Sample helper: paints an element with the brush resource whose key comes from a binding. A
/// <c>DynamicResource</c> needs a literal key written in the markup, so a data driven token list
/// cannot use one — this pushes the resource reference itself, keeping every swatch live when the
/// theme is swapped.
/// </summary>
public static class ResourceSwatch
{
    public static readonly DependencyProperty ForegroundKeyProperty = DependencyProperty.RegisterAttached(
        "ForegroundKey",
        typeof(object),
        typeof(ResourceSwatch),
        new PropertyMetadata(null, OnForegroundKeyChanged));

    public static object? GetForegroundKey(DependencyObject element) => element.GetValue(ForegroundKeyProperty);

    public static void SetForegroundKey(DependencyObject element, object? value)
        => element.SetValue(ForegroundKeyProperty, value);

    private static void OnForegroundKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock text)
            return;

        if (e.NewValue == null)
            text.ClearValue(TextBlock.ForegroundProperty);
        else
            text.SetResourceReference(TextBlock.ForegroundProperty, e.NewValue);
    }

    public static readonly DependencyProperty BackgroundKeyProperty = DependencyProperty.RegisterAttached(
        "BackgroundKey",
        typeof(object),
        typeof(ResourceSwatch),
        new PropertyMetadata(null, OnBackgroundKeyChanged));

    public static object? GetBackgroundKey(DependencyObject element) => element.GetValue(BackgroundKeyProperty);

    public static void SetBackgroundKey(DependencyObject element, object? value)
        => element.SetValue(BackgroundKeyProperty, value);

    private static void OnBackgroundKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Border border)
            return;

        if (e.NewValue == null)
            border.ClearValue(Border.BackgroundProperty);
        else
            border.SetResourceReference(Border.BackgroundProperty, e.NewValue);
    }
}
