using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class DropdownSamplesViewModel : ObservableObject
{
    public string Code =>
        "<ToggleButton Content=\"Actions\" inputs:Dropdown.CloseOnClick=\"True\">\n" +
        "    <inputs:Dropdown.Popup>\n" +
        "        <StackPanel toolkit:Spacing.Gap=\"4\">\n" +
        "            <Button Content=\"Rename\" Style=\"{StaticResource GhostButton}\" />\n" +
        "            <Button Content=\"Duplicate\" Style=\"{StaticResource GhostButton}\" />\n" +
        "        </StackPanel>\n" +
        "    </inputs:Dropdown.Popup>\n" +
        "</ToggleButton>";

    public string PlacementCode =>
        "<!-- BottomLeft (default), BottomRight, TopLeft, TopRight -->\n" +
        "<ToggleButton Content=\"Filter\" inputs:Dropdown.Placement=\"BottomRight\">\n" +
        "    <inputs:Dropdown.Popup>\n" +
        "        <TextBlock Text=\"Opens down, right aligned.\" />\n" +
        "    </inputs:Dropdown.Popup>\n" +
        "</ToggleButton>";

    public string StyleCode =>
        "<ToggleButton\n" +
        "    inputs:Dropdown.Placement=\"BottomRight\"\n" +
        "    toolkit:Sizing.IsSquare=\"True\"\n" +
        "    toolkit:Sizing.Size=\"lg\">\n" +
        "    <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.EllipsisVertical}\" />\n" +
        "    <inputs:Dropdown.Popup>\n" +
        "        <TextBlock Text=\"An icon only dropdown.\" />\n" +
        "    </inputs:Dropdown.Popup>\n" +
        "</ToggleButton>";

    public string PopupStyleCode =>
        "<ToggleButton Content=\"Padded\">\n" +
        "    <inputs:Dropdown.PopupStyle>\n" +
        "        <Style\n" +
        "            TargetType=\"{x:Type inputs:DropdownPopupHost}\"\n" +
        "            BasedOn=\"{StaticResource {x:Type inputs:DropdownPopupHost}}\">\n" +
        "            <Setter Property=\"Padding\" Value=\"12,8\" />\n" +
        "        </Style>\n" +
        "    </inputs:Dropdown.PopupStyle>\n" +
        "    <inputs:Dropdown.Popup>\n" +
        "        <TextBlock Text=\"Content padded away from the border.\" />\n" +
        "    </inputs:Dropdown.Popup>\n" +
        "</ToggleButton>";
}
