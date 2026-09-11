using System.Windows;

namespace Joufflu;

public static class Dimensions
{
    public static ComponentResourceKey Radius => new(typeof(Dimensions), "Radius");
    public static ComponentResourceKey CornerRadius => new(typeof(Dimensions), "CornerRadius");
    public static ComponentResourceKey Thickness => new(typeof(Dimensions), "Thickness");

    public static ComponentResourceKey BorderThickness => new(typeof(Dimensions), "BorderThickness");

    public static ComponentResourceKey Spacing => new(typeof(Dimensions), "Spacing");
    public static ComponentResourceKey SpacingThickness => new(typeof(Dimensions), "SpacingThickness");

    public static ComponentResourceKey TitleBarHeight => new(typeof(Dimensions), "TitleBarHeight");
    public static ComponentResourceKey TitleBarHeightOffset => new(typeof(Dimensions), "TitleBarHeightOffset");

    public static ComponentResourceKey HeightXs => new(typeof(Dimensions), "HeightXs");
    public static ComponentResourceKey HeightSm => new(typeof(Dimensions), "HeightSm");
    public static ComponentResourceKey HeightMd => new(typeof(Dimensions), "HeightMd");
    public static ComponentResourceKey HeightLg => new(typeof(Dimensions), "HeightLg");

    // 0.625 of HeightMd, kept in sync automatically by the theme customizer. Used by the small
    // "clear" close buttons embedded inside other controls (search, combobox, numeric input…).
    public static ComponentResourceKey HeightEmbedded => new(typeof(Dimensions), "HeightEmbedded");

    public static ComponentResourceKey FontSizeXs => new(typeof(Dimensions), "FontSizeXs");
    public static ComponentResourceKey FontSizeSm => new(typeof(Dimensions), "FontSizeSm");
    public static ComponentResourceKey FontSizeMd => new(typeof(Dimensions), "FontSizeMd");
    public static ComponentResourceKey FontSizeLg => new(typeof(Dimensions), "FontSizeLg");
    public static ComponentResourceKey FontSizeXl => new(typeof(Dimensions), "FontSizeXl");

    public static ComponentResourceKey PaddingXs => new(typeof(Dimensions), "PaddingXs");
    public static ComponentResourceKey PaddingSm => new(typeof(Dimensions), "PaddingSm");
    public static ComponentResourceKey PaddingMd => new(typeof(Dimensions), "PaddingMd");
    public static ComponentResourceKey PaddingLg => new(typeof(Dimensions), "PaddingLg");

    // Text inputs read tighter than a button, the horizontal padding is half a button padding so
    // that the text is more flused against the border
    public static ComponentResourceKey InputPaddingXs => new(typeof(Dimensions), "InputPaddingXs");
    public static ComponentResourceKey InputPaddingSm => new(typeof(Dimensions), "InputPaddingSm");
    public static ComponentResourceKey InputPaddingMd => new(typeof(Dimensions), "InputPaddingMd");
    public static ComponentResourceKey InputPaddingLg => new(typeof(Dimensions), "InputPaddingLg");
}