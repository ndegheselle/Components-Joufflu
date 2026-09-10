using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Joufflu.Samples.Views.Natives.DataDisplay;

public class TreeNode
{
    public string Name { get; set; } = "";

    public ObservableCollection<TreeNode> Children { get; } = new();
}

public class TreeViewSamplesViewModel : ObservableObject
{
    public ObservableCollection<TreeNode> Tree { get; } = new();

    public TreeViewSamplesViewModel()
    {
        var fruits = new TreeNode { Name = "Fruits" };
        var apple = new TreeNode { Name = "Apple" };
        apple.Children.Add(new TreeNode { Name = "Granny Smith" });
        apple.Children.Add(new TreeNode { Name = "Fuji" });
        fruits.Children.Add(apple);
        fruits.Children.Add(new TreeNode { Name = "Banana" });

        var veggies = new TreeNode { Name = "Vegetables" };
        veggies.Children.Add(new TreeNode { Name = "Carrot" });
        veggies.Children.Add(new TreeNode { Name = "Potato" });

        Tree.Add(fruits);
        Tree.Add(veggies);
    }

    public string Code =>
        "<TreeView ItemsSource=\"{Binding Tree}\">\n" +
        "    <TreeView.ItemTemplate>\n" +
        "        <HierarchicalDataTemplate ItemsSource=\"{Binding Children}\">\n" +
        "            <TextBlock Text=\"{Binding Name}\" />\n" +
        "        </HierarchicalDataTemplate>\n" +
        "    </TreeView.ItemTemplate>\n" +
        "</TreeView>";

    public string SizesCode =>
        "<TreeView toolkit:Sizing.Size=\"xs\" ItemsSource=\"{Binding Tree}\" />\n" +
        "<TreeView toolkit:Sizing.Size=\"sm\" ItemsSource=\"{Binding Tree}\" />\n" +
        "<TreeView ItemsSource=\"{Binding Tree}\" /> <!-- md, the default -->\n" +
        "<TreeView toolkit:Sizing.Size=\"lg\" ItemsSource=\"{Binding Tree}\" />";
}
