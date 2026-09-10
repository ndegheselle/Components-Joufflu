using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Feedback;

public class BadgeSamplesViewModel : ObservableObject
{
    public string VariantsCode =>
        "<feedback:Badge>Default</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Primary\">Primary</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Success\">Active</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Danger\">3</feedback:Badge>";

    public string SizesCode =>
        "<feedback:Badge Variant=\"Primary\" toolkit:Sizing.Size=\"xs\">xs</feedback:Badge>\n" +
        "<feedback:Badge Variant=\"Primary\" toolkit:Sizing.Size=\"lg\">lg</feedback:Badge>";
}
