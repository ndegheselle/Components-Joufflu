using System.Windows;
using System.Windows.Controls;
using Joufflu.Assets.Fonts;

namespace Joufflu.Data.Controls;

/// <summary>
/// The glyph a kind is read by, so a shape is taken in without reading it. Optionally followed by
/// the name of the kind, for the places where the glyph alone is not enough.
/// </summary>
public class DataKindIcon : Control
{
    static DataKindIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataKindIcon), new FrameworkPropertyMetadata(typeof(DataKindIcon)));
    }

    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind), typeof(EnumDataKind), typeof(DataKindIcon),
        new PropertyMetadata(EnumDataKind.String, (d, _) => ((DataKindIcon)d).Refresh()));

    private static readonly DependencyPropertyKey GlyphPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Glyph), typeof(string), typeof(DataKindIcon), new PropertyMetadata(LucideFontIcons.Type));

    public static readonly DependencyProperty GlyphProperty = GlyphPropertyKey.DependencyProperty;

    public static readonly DependencyProperty ShowNameProperty = DependencyProperty.Register(
        nameof(ShowName), typeof(bool), typeof(DataKindIcon), new PropertyMetadata(false));

    /// <summary>What the icon stands for.</summary>
    public EnumDataKind Kind
    {
        get => (EnumDataKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>The glyph <see cref="Kind"/> is drawn as, written by the control.</summary>
    public string Glyph => (string)GetValue(GlyphProperty);

    /// <summary>Whether the name of the kind is written next to the glyph.</summary>
    public bool ShowName
    {
        get => (bool)GetValue(ShowNameProperty);
        set => SetValue(ShowNameProperty, value);
    }

    private void Refresh() => SetValue(GlyphPropertyKey, GlyphOf(Kind));

    /// <summary>The glyph [kind] is read by.</summary>
    public static string GlyphOf(EnumDataKind kind) => kind switch
    {
        EnumDataKind.Object => LucideFontIcons.Braces,
        EnumDataKind.Array => LucideFontIcons.Brackets,
        EnumDataKind.Enumeration => LucideFontIcons.List,
        EnumDataKind.String => LucideFontIcons.Type,
        EnumDataKind.Integer or EnumDataKind.Number => LucideFontIcons.Hash,
        EnumDataKind.Boolean => LucideFontIcons.ToggleLeft,
        EnumDataKind.DateTime or EnumDataKind.Date => LucideFontIcons.Calendar,
        EnumDataKind.Time or EnumDataKind.Duration => LucideFontIcons.Timer,
        // A schema saying nothing of a type accepts anything.
        _ => LucideFontIcons.CircleHelp
    };
}
