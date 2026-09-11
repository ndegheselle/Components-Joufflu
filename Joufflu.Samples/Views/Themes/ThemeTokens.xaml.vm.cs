using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Themes;

/// <summary>
/// One documented resource key: its name, what the control styles use it for, and - for a colour -
/// the brush key painting the live swatch shown next to it.
/// </summary>
public sealed record TokenEntry(string Key, string Usage, ComponentResourceKey? Swatch = null);

/// <summary>A family of related keys, shown as one titled table.</summary>
public sealed record TokenGroup(string Title, string Description, IReadOnlyList<TokenEntry> Entries);

/// <summary>
/// One accent family, the three keys it is made of shown side by side rather than described: the
/// fill, the hover and pressed fill, and the content drawn on top of both.
/// </summary>
public sealed record AccentEntry(
    string Name,
    string Usage,
    ComponentResourceKey Fill,
    ComponentResourceKey Hover,
    ComponentResourceKey Content);

/// <summary>
/// Reference of every key a theme is made of. The descriptions are the roles the keys actually play
/// in the control styles, so a custom palette can be written without reading them.
/// </summary>
public class ThemeTokensViewModel : ObservableObject
{
    /// <summary>Every key of <c>Joufflu.Colors</c> / <c>Joufflu.Brushes</c>.</summary>
    public IReadOnlyList<TokenGroup> ColorTokens { get; } =
    [
        new TokenGroup(
            "Surfaces",
            "The neutral ground every control is drawn on, from the furthest back to the closest.",
            [
                new TokenEntry(
                    "Background",
                    "Window and page background, the furthest back surface.",
                    Joufflu.Brushes.BackgroundBrush),
                new TokenEntry(
                    "Background100",
                    "Elevated surface sitting on top of the background: cards, popups, drop-downs, data rows.",
                    Joufflu.Brushes.Background100Brush),
                new TokenEntry(
                    "Background200",
                    "Transient surface: hovered ghost button, hovered or selected row, slider track.",
                    Joufflu.Brushes.Background200Brush),
                new TokenEntry(
                    "Border",
                    "Default border of every framed control (text box, card, drop-down).",
                    Joufflu.Brushes.BorderBrush),
                new TokenEntry(
                    "Border100",
                    "Stronger border, used when a control is hovered or focused and for separator lines.",
                    Joufflu.Brushes.Border100Brush),
            ]),

        new TokenGroup(
            "Text",
            "Foreground of text and icons. The suffix says how much the text recedes.",
            [
                new TokenEntry(
                    "Foreground",
                    "Default text and icon colour, on Background or Background100.",
                    Joufflu.Brushes.ForegroundBrush),
                new TokenEntry(
                    "Foreground100",
                    "Muted text: placeholders, hints, secondary labels. This is what the Muted typography style uses.",
                    Joufflu.Brushes.Foreground100Brush),
                new TokenEntry(
                    "Foreground200",
                    "Text on a selected row, where the default foreground would lose contrast.",
                    Joufflu.Brushes.Foreground200Brush),
            ]),

    ];

    /// <summary>
    /// The six accent families. Each one is rendered as its three swatches so the relationship
    /// between them is seen rather than read.
    /// </summary>
    public IReadOnlyList<AccentEntry> AccentTokens { get; } =
    [
        new AccentEntry(
            "Primary",
            "Main call to action: the default filled button, the selected tab or navigation entry.",
            Joufflu.Brushes.PrimaryBrush,
            Joufflu.Brushes.Primary100Brush,
            Joufflu.Brushes.PrimaryContentBrush),
        new AccentEntry(
            "Secondary",
            "Neutral filled action, for a button that must read as a button without competing with Primary.",
            Joufflu.Brushes.SecondaryBrush,
            Joufflu.Brushes.Secondary100Brush,
            Joufflu.Brushes.SecondaryContentBrush),
        new AccentEntry(
            "Success",
            "Confirmation: success toast, valid state, positive badge.",
            Joufflu.Brushes.SuccessBrush,
            Joufflu.Brushes.Success100Brush,
            Joufflu.Brushes.SuccessContentBrush),
        new AccentEntry(
            "Info",
            "Neutral information: info toast, informative badge.",
            Joufflu.Brushes.InfoBrush,
            Joufflu.Brushes.Info100Brush,
            Joufflu.Brushes.InfoContentBrush),
        new AccentEntry(
            "Warning",
            "Something needs attention but nothing is broken yet.",
            Joufflu.Brushes.WarningBrush,
            Joufflu.Brushes.Warning100Brush,
            Joufflu.Brushes.WarningContentBrush),
        new AccentEntry(
            "Danger",
            "Destructive action and validation errors - a delete button, the border of a field in error.",
            Joufflu.Brushes.DangerBrush,
            Joufflu.Brushes.Danger100Brush,
            Joufflu.Brushes.DangerContentBrush),
    ];

    /// <summary>The odd one out of <c>Joufflu.Colors</c>: a number rather than a colour.</summary>
    public IReadOnlyList<TokenGroup> StateTokens { get; } =
    [
        new TokenGroup(
            "State",
            "Not a colour, but it lives in Colors because it only exists to dim one.",
            [
                new TokenEntry(
                    "DisabledOpacity",
                    "Opacity applied to a whole control while IsEnabled is False (0.5). Disabled controls "
                    + "fade rather than switch to a dedicated palette, so a single key covers every control.",
                    null),
            ]),
    ];

    /// <summary>Every key of <c>Joufflu.Dimensions</c>.</summary>
    public IReadOnlyList<TokenGroup> DimensionTokens { get; } =
    [
        new TokenGroup(
            "Shape",
            "A scalar plus the ready made Thickness / CornerRadius built from it. Bind the scalar through "
            + "Derive when you need only some sides or corners, the composite one otherwise.",
            [
                new TokenEntry("Thickness", "double - base border width (1)."),
                new TokenEntry("BorderThickness", "Thickness - Thickness on all four sides, for a uniform border."),
                new TokenEntry("Radius", "double - base corner radius (4)."),
                new TokenEntry(
                    "CornerRadius",
                    "CornerRadius - Radius on all four corners, for a uniformly rounded border."),
            ]),

        new TokenGroup(
            "Layout",
            "Distances between things rather than inside them.",
            [
                new TokenEntry(
                    "Spacing",
                    "double - standard gap between sibling elements (12). Feed it to Spacing.Gap."),
                new TokenEntry("SpacingThickness", "Thickness - Spacing on all four sides, the standard page padding."),
                new TokenEntry("TitleBarHeight", "double - height of the custom window title bar (30)."),
                new TokenEntry(
                    "TitleBarHeightOffset",
                    "Thickness - Top only, used as a margin to push content below an overlapping title bar."),
            ]),

        new TokenGroup(
            "Control heights",
            "Vertical size of single line controls. Sizing.Size picks one of the four; md is the default "
            + "written in the base styles, so only xs / sm / lg appear in the size triggers.",
            [
                new TokenEntry("HeightXs", "24 - dense tables and toolbars."),
                new TokenEntry("HeightSm", "28"),
                new TokenEntry("HeightMd", "32 - the default."),
                new TokenEntry("HeightLg", "40 - prominent, touch friendly controls."),
                new TokenEntry(
                    "HeightEmbedded",
                    "20 - 0.625 of HeightMd, kept in sync by the theme customizer. The small clear button "
                    + "embedded inside a search box, combo box or numeric input."),
            ]),

        new TokenGroup(
            "Font sizes",
            "The typography styles (H1…H6, Muted, …) are built on these, so restyling the scale restyles the text.",
            [
                new TokenEntry("FontSizeXs", "11"),
                new TokenEntry("FontSizeSm", "12"),
                new TokenEntry("FontSizeMd", "13 - the default body size."),
                new TokenEntry("FontSizeLg", "16"),
                new TokenEntry("FontSizeXl", "24"),
            ]),

        new TokenGroup(
            "Paddings",
            "Space inside a control, one Thickness per size. Two families: buttons and anything content "
            + "shaped use Padding*, text inputs use InputPadding*, whose halved horizontal keeps the caret "
            + "closer to the border.",
            [
                new TokenEntry("PaddingXs", "6,2"),
                new TokenEntry("PaddingSm", "9,3"),
                new TokenEntry("PaddingMd", "12,4 - the default."),
                new TokenEntry("PaddingLg", "18,6"),
                new TokenEntry("InputPaddingXs", "3,2"),
                new TokenEntry("InputPaddingSm", "4.5,3"),
                new TokenEntry("InputPaddingMd", "6,4 - the default."),
                new TokenEntry("InputPaddingLg", "9,6"),
            ]),
    ];

    public string UsageCode =>
        "<!-- Always DynamicResource: a theme swap, or the customizer editing a -->\n" +
        "<!-- dimension, has to reach controls already on screen.               -->\n" +
        "<Border\n" +
        "    Background=\"{DynamicResource {x:Static joufflu:Brushes.Background100Brush}}\"\n" +
        "    BorderBrush=\"{DynamicResource {x:Static joufflu:Brushes.BorderBrush}}\"\n" +
        "    BorderThickness=\"{DynamicResource {x:Static joufflu:Dimensions.BorderThickness}}\"\n" +
        "    CornerRadius=\"{DynamicResource {x:Static joufflu:Dimensions.CornerRadius}}\" />\n\n" +
        "<!-- The raw Color is there for what a brush cannot express -->\n" +
        "<GradientStop Color=\"{DynamicResource {x:Static joufflu:Colors.PrimaryColor}}\" Offset=\"0\" />";
}
