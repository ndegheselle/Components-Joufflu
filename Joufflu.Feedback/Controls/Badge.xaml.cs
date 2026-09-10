using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Feedback.Controls;

/// <summary>Semantic color intent for a <see cref="Badge"/>.</summary>
public enum BadgeVariant
{
    Default,
    Primary,
    Secondary,
    Success,
    Info,
    Warning,
    Danger
}

/// <summary>
/// A small pill that labels or counts something, themed from the design system's
/// semantic brushes and sized through <see cref="Joufflu.Toolkit.Sizing.SizeProperty"/>.
/// </summary>
public class Badge : ContentControl
{
    static Badge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Badge),
            new FrameworkPropertyMetadata(typeof(Badge)));
    }

    public BadgeVariant Variant
    {
        get => (BadgeVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(BadgeVariant), typeof(Badge), new PropertyMetadata(BadgeVariant.Default));
}
