using System.Windows;
using Joufflu.Assets.Fonts;
using Joufflu.Toolkit;
using NJsonSchema;

namespace Joufflu.Data.Controls;

/// <summary>
/// The glyph a <see cref="JsonObjectType"/> is read by, so a shape is taken in without reading it.
/// The name of the type is left to the tooltip.
/// </summary>
public class DataIcon : FontIcon
{
    static DataIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataIcon), new FrameworkPropertyMetadata(typeof(FontIcon)));
    }

    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
        nameof(Type), typeof(JsonObjectType), typeof(DataIcon),
        new PropertyMetadata(JsonObjectType.String, (d, _) => ((DataIcon)d).Refresh()));

    /// <summary>What the icon stands for.</summary>
    public JsonObjectType Type
    {
        get => (JsonObjectType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public DataIcon() { Refresh(); }

    private void Refresh()
    {
        Text = GlyphOf(Type);
        Tooltip.SetContent(this, Type.ToString());
    }

    /// <summary>
    /// The glyph [type] is read by. <see cref="JsonObjectType"/> is a flag enum, so a type holding
    /// several flags is read by the first one that carries a shape.
    /// </summary>
    public static string GlyphOf(JsonObjectType type)
    {
        if (type.HasFlag(JsonObjectType.Object))
            return LucideFontIcons.Braces;
        if (type.HasFlag(JsonObjectType.Array))
            return LucideFontIcons.Brackets;
        if (type.HasFlag(JsonObjectType.String))
            return LucideFontIcons.Type;
        if (type.HasFlag(JsonObjectType.Integer) || type.HasFlag(JsonObjectType.Number))
            return LucideFontIcons.Hash;
        if (type.HasFlag(JsonObjectType.Boolean))
            return LucideFontIcons.ToggleLeft;
        if (type.HasFlag(JsonObjectType.File))
            return LucideFontIcons.File;
        if (type.HasFlag(JsonObjectType.Null))
            return LucideFontIcons.CircleSlash;
        // A schema saying nothing of a type accepts anything.
        return LucideFontIcons.CircleHelp;
    }
}
