using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Toolkit;

public class TooltipSamplesViewModel : ObservableObject
{
    public string PlacementCode =>
        "<!-- Tooltip.Content wires up a themed tooltip on any element -->\n" +
        "<!-- Tooltip.Placement picks the side: Top / Bottom / Left / Right -->\n" +
        "<Button Content=\"Top\"\n" +
        "        toolkit:Tooltip.Content=\"Placed above\"\n" +
        "        toolkit:Tooltip.Placement=\"Top\" />\n\n" +
        "<Button Content=\"Right\"\n" +
        "        toolkit:Tooltip.Content=\"Placed right\"\n" +
        "        toolkit:Tooltip.Placement=\"Right\" />";

    public string ContentCode =>
        "<!-- Tooltip.Content is an object: pass arbitrary XAML -->\n" +
        "<Button Content=\"Rich tooltip\" toolkit:Tooltip.Placement=\"Bottom\">\n" +
        "    <toolkit:Tooltip.Content>\n" +
        "        <StackPanel Orientation=\"Horizontal\" toolkit:Spacing.Gap=\"8\">\n" +
        "            <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Info}\" />\n" +
        "            <TextBlock Text=\"Arbitrary content, icons included.\" />\n" +
        "        </StackPanel>\n" +
        "    </toolkit:Tooltip.Content>\n" +
        "</Button>";
}
