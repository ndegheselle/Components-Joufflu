using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class SearchSamplesViewModel : ObservableObject
{
    public string SearchCode =>
        "<inputs:Search />\n" +
        "// code-behind: search.SearchChanged += text => Filter(text);";
}
