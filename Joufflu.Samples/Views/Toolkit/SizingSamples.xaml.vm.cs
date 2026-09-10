using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class SizingSamplesViewModel : ObservableObject
{
    public string SizeCode =>
        "<!-- Attached property drives height, font size and padding -->\n" +
        "<Button toolkit:Sizing.Size=\"xs\" />\n" +
        "<Button toolkit:Sizing.Size=\"sm\" />\n" +
        "<Button toolkit:Sizing.Size=\"md\" />  <!-- default -->\n" +
        "<Button toolkit:Sizing.Size=\"lg\" />\n\n" +
        "<!-- Size is inherited, so a panel sets it for every child -->\n" +
        "<StackPanel toolkit:Sizing.Size=\"lg\">\n" +
        "    <TextBox /> <ComboBox /> <Button>OK</Button>\n" +
        "</StackPanel>";

    public string SquareCode =>
        "<Button toolkit:Sizing.IsSquare=\"True\"\n" +
        "        toolkit:Sizing.Size=\"lg\">\n" +
        "    <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Leaf}\" />\n" +
        "</Button>";
}
