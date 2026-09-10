using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class SpacingSamplesViewModel : ObservableObject
{
    public string GapCode =>
        "<!-- Gap adds spacing between children of any panel -->\n" +
        "<!-- StackPanel: gap along the Orientation axis -->\n" +
        "<StackPanel Orientation=\"Horizontal\" toolkit:Spacing.Gap=\"8\">\n" +
        "    <Button>One</Button> <Button>Two</Button> <Button>Three</Button>\n" +
        "</StackPanel>\n\n" +
        "<!-- Gap is a Thickness: one value is uniform, -->\n" +
        "<!-- \"horizontal,vertical\" sets each axis (Left/Top) -->\n" +
        "<WrapPanel toolkit:Spacing.Gap=\"8,12\">\n" +
        "    ...\n" +
        "</WrapPanel>\n\n" +
        "<!-- Grid: gap sits between rows and columns -->\n" +
        "<Grid toolkit:Spacing.Gap=\"12\">\n" +
        "    ...\n" +
        "</Grid>";
}
