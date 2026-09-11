using System.Windows.Media;

namespace Joufflu.Samples.Views.Themes;

/// <summary>
/// A named, ready-to-apply colour palette selectable from the theme customizer.
/// <see cref="Colors"/> is keyed by the same resource names the editor uses (e.g. <c>PrimaryColor</c>),
/// while the brush properties expose a few swatches so the selector card can render itself
/// in the theme's own colours (à la daisyUI's theme list).
/// </summary>
public class ThemePreset
{
    public string Name { get; }

    /// <summary>Colour per <see cref="ThemeColorEntry.ResourceName"/>; covers all 26 editable keys.</summary>
    public IReadOnlyDictionary<string, Color> Colors { get; }

    public Brush Background { get; }
    public Brush Foreground { get; }
    public Brush Border { get; }
    public Brush Primary { get; }
    public Brush Secondary { get; }
    public Brush Accent { get; }
    public Brush Neutral { get; }

    public ThemePreset(string name, IReadOnlyDictionary<string, Color> colors)
    {
        Name = name;
        Colors = colors;
        Background = Swatch(colors["BackgroundColor"]);
        Foreground = Swatch(colors["ForegroundColor"]);
        Border = Swatch(colors["BorderColor"]);
        Primary = Swatch(colors["PrimaryColor"]);
        Secondary = Swatch(colors["SecondaryColor"]);
        Accent = Swatch(colors["InfoColor"]);
        Neutral = Swatch(colors["Background200Color"]);
    }

    private static SolidColorBrush Swatch(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
