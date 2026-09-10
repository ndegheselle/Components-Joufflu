using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Joufflu.Samples.Views.Natives.DataDisplay;

public class DataGridSamplesViewModel : ObservableObject
{
    public ObservableCollection<Person> People { get; } = new()
    {
        new Person { Name = "Ada Lovelace", Role = "Engineer", Age = 36 },
        new Person { Name = "Alan Turing", Role = "Researcher", Age = 41 },
        new Person { Name = "Grace Hopper", Role = "Admiral", Age = 79 },
        new Person { Name = "Katherine Johnson", Role = "Mathematician", Age = 52 },
        new Person { Name = "Edsger Dijkstra", Role = "Engineer", Age = 72 },
    };

    public string Code =>
        "<DataGrid ItemsSource=\"{Binding People}\" AutoGenerateColumns=\"False\">\n" +
        "    <DataGrid.Columns>\n" +
        "        <DataGridTextColumn Header=\"Name\" Binding=\"{Binding Name}\" />\n" +
        "        <DataGridTextColumn Header=\"Role\" Binding=\"{Binding Role}\" />\n" +
        "    </DataGrid.Columns>\n" +
        "</DataGrid>";

    public string CompactCode =>
        "<DataGrid toolkit:Sizing.Size=\"xs\"\n" +
        "          ItemsSource=\"{Binding People}\" AutoGenerateColumns=\"False\">\n" +
        "    <DataGrid.Columns>\n" +
        "        <DataGridTextColumn Header=\"Name\" Binding=\"{Binding Name}\" />\n" +
        "        <DataGridTextColumn Header=\"Role\" Binding=\"{Binding Role}\" />\n" +
        "    </DataGrid.Columns>\n" +
        "</DataGrid>";
}
