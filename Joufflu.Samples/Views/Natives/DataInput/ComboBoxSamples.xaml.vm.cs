using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Joufflu.Samples.Views.Natives.DataInput;

public class ComboBoxSamplesViewModel : ObservableObject
{
    private string? _selectedOption = "Two";

    public ObservableCollection<string> Options { get; } = new() { "One", "Two", "Three", "Four" };

    public string? SelectedOption { get => _selectedOption; set => SetProperty(ref _selectedOption, value); }

    public string SelectionCode =>
        "<ComboBox ItemsSource=\"{Binding Options}\"\n" +
        "          SelectedItem=\"{Binding SelectedOption}\" />";

    public string EditableCode =>
        "<ComboBox IsEditable=\"True\"\n" +
        "          ItemsSource=\"{Binding Options}\"\n" +
        "          Text=\"{Binding SelectedOption}\" />";

    public string SizesCode =>
        "<ComboBox toolkit:Sizing.Size=\"sm\" />\n" +
        "<ComboBox />\n" +
        "<ComboBox toolkit:Sizing.Size=\"lg\" />";
}
