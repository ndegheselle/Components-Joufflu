using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Feedback.Controls;

/// <summary>
/// An indeterminate, continuously spinning loading indicator. Its color comes from
/// <see cref="Control.Foreground"/> (accent by default) and its diameter from
/// <see cref="Joufflu.Toolkit.Sizing.SizeProperty"/>.
/// </summary>
public class Spinner : Control
{
    static Spinner()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Spinner),
            new FrameworkPropertyMetadata(typeof(Spinner)));
    }
}
