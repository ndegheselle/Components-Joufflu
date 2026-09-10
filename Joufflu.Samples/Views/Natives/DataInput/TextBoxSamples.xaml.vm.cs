using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Natives.DataInput;

public class TextBoxSamplesViewModel : ObservableObject
{
    private string _text = "Hello";

    public string Text { get => _text; set => SetProperty(ref _text, value); }

    public string TextCode => "<TextBox Text=\"{Binding Text}\" />";

    public string SizesCode =>
        "<TextBox toolkit:Sizing.Size=\"sm\" />\n" +
        "<TextBox />\n" +
        "<TextBox toolkit:Sizing.Size=\"lg\" />";

    public string OtherCode =>
        "<PasswordBox />\n" +
        "<TextBox AcceptsReturn=\"True\" TextWrapping=\"Wrap\" />";
}
