using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Joufflu.Samples.Views.Inputs;

public class SelectionInputsSamplesViewModel : ObservableObject
{
    private string? _selectedCountry = "France";

    public ObservableCollection<string> Countries { get; } = new()
    {
        "Belgium", "Canada", "Denmark", "France", "Germany",
        "Italy", "Japan", "Norway", "Portugal", "Spain", "Sweden",
    };

    public string? SelectedCountry { get => _selectedCountry; set => SetProperty(ref _selectedCountry, value); }

    public IReadOnlyList<Airport> Airports { get; } = new List<Airport>
    {
        new("AMS", "Amsterdam"), new("BRU", "Brussels"), new("CDG", "Paris"),
        new("FCO", "Rome"), new("LHR", "London"), new("LIS", "Lisbon"),
        new("MAD", "Madrid"), new("NRT", "Tokyo"), new("YUL", "Montreal"),
    };

    private object? _selectedAirport;

    /// <summary>An <see cref="Airport"/> when the code is one of the list, the typed string otherwise.</summary>
    public object? SelectedAirport { get => _selectedAirport; set => SetProperty(ref _selectedAirport, value); }

    public string SearchCode =>
        "<inputs:Search />\n" +
        "// code-behind: search.SearchChanged += text => Filter(text);";

    public string ComboSearchCode =>
        "<inputs:ComboBoxSearch ItemsSource=\"{Binding Countries}\"\n" +
        "                       SelectedItem=\"{Binding SelectedCountry}\" />";

    public string AutocompleteCode =>
        "<inputs:Autocomplete ItemsSource=\"{Binding Airports}\"\n" +
        "                     TextMemberPath=\"Code\"\n" +
        "                     SelectedItem=\"{Binding SelectedAirport}\">\n" +
        "    <inputs:Autocomplete.ItemTemplate>\n" +
        "        <DataTemplate>\n" +
        "            <StackPanel Orientation=\"Horizontal\" toolkit:Spacing.Gap=\"6\">\n" +
        "                <TextBlock Text=\"{Binding Code}\" FontWeight=\"Bold\" />\n" +
        "                <TextBlock Text=\"{Binding City}\" Style=\"{StaticResource Muted}\" />\n" +
        "            </StackPanel>\n" +
        "        </DataTemplate>\n" +
        "    </inputs:Autocomplete.ItemTemplate>\n" +
        "</inputs:Autocomplete>";
}

/// <summary>Sample item : what an autocomplete completes on (<c>Code</c>) is rarely all it shows.</summary>
public class Airport
{
    public Airport(string code, string city)
    {
        Code = code;
        City = city;
    }

    public string Code { get; }

    public string City { get; }

    public override string ToString() => $"{Code} ({City})";
}
