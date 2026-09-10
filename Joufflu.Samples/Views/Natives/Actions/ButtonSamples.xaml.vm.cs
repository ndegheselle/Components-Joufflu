using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.Actions;

public class ButtonSamplesViewModel : ObservableObject
{
    public string VariantsCode =>
        "<Button>Default</Button>\n" +
        "<Button Style=\"{StaticResource PrimaryButton}\">Primary</Button>\n" +
        "<Button Style=\"{StaticResource SecondaryButton}\">Secondary</Button>\n" +
        "<Button Style=\"{StaticResource GhostButton}\">Ghost</Button>\n" +
        "<Button Style=\"{StaticResource SuccessButton}\">Success</Button>\n" +
        "<Button Style=\"{StaticResource DangerButton}\">Danger</Button>";

    public string SoftCode =>
        "<!-- Tinted background, semantic hue as text -->\n" +
        "<Button Style=\"{StaticResource SoftPrimaryButton}\">Primary</Button>\n" +
        "<Button Style=\"{StaticResource SoftSuccessButton}\">Success</Button>\n" +
        "<Button Style=\"{StaticResource SoftInfoButton}\">Info</Button>\n" +
        "<Button Style=\"{StaticResource SoftWarningButton}\">Warning</Button>\n" +
        "<Button Style=\"{StaticResource SoftDangerButton}\">Danger</Button>";

    public string OutlineCode =>
        "<!-- Coloured border and text, transparent fill -->\n" +
        "<Button Style=\"{StaticResource OutlinePrimaryButton}\">Primary</Button>\n" +
        "<Button Style=\"{StaticResource OutlineSuccessButton}\">Success</Button>\n" +
        "<Button Style=\"{StaticResource OutlineInfoButton}\">Info</Button>\n" +
        "<Button Style=\"{StaticResource OutlineWarningButton}\">Warning</Button>\n" +
        "<Button Style=\"{StaticResource OutlineDangerButton}\">Danger</Button>";

    public string IconCode =>
        "<Button toolkit:Sizing.IsSquare=\"True\">\n" +
        "    <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Plus}\" />\n" +
        "</Button>";

    public string SizesCode =>
        "<Button toolkit:Sizing.Size=\"xs\">XS</Button>\n" +
        "<Button toolkit:Sizing.Size=\"sm\">SM</Button>\n" +
        "<Button toolkit:Sizing.Size=\"md\">MD</Button>\n" +
        "<Button toolkit:Sizing.Size=\"lg\">LG</Button>";
}
