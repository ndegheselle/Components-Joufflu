using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Feedback;

public class SpinnerSamplesViewModel : ObservableObject
{
    public string Code =>
        "<feedback:Spinner />\n" +
        "<feedback:Spinner toolkit:Sizing.Size=\"lg\" />";
}
