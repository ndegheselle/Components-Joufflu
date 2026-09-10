using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Navigation;

namespace Joufflu.Samples.Views.Navigation;

/// <summary>
/// Demo pages for the menu sample. One type per menu item, since a <c>NavigationItem</c> targets a
/// type and shows as selected while a page of that type is current.
/// </summary>
public abstract record DemoPage(string Title)
{
    public override string ToString() => Title;
}

public sealed record DemoHomePage() : DemoPage("Home page");

public sealed record DemoSubmenu1Page() : DemoPage("Submenu 1 page");

public sealed record DemoSubmenu2Page() : DemoPage("Submenu 2 page");

public sealed record DemoNestedSubmenu1Page() : DemoPage("Nested submenu 1 page");

public sealed record DemoNestedSubmenu2Page() : DemoPage("Nested submenu 2 page");

public sealed record DemoSettingsPage() : DemoPage("Settings page");

public class NavigationMenuSamplesViewModel : ObservableObject
{
    /// <summary>Demo pages keyed by their own type, which is what the menu items target.</summary>
    private readonly Dictionary<Type, object> _pages = new object[]
    {
        new DemoHomePage(),
        new DemoSubmenu1Page(),
        new DemoSubmenu2Page(),
        new DemoNestedSubmenu1Page(),
        new DemoNestedSubmenu2Page(),
        new DemoSettingsPage(),
    }.ToDictionary(page => page.GetType());

    /// <summary>A standalone navigator so the demo menu can show selection + navigation.</summary>
    public Navigator DemoNavigator { get; }

    public NavigationMenuSamplesViewModel()
    {
        DemoNavigator = new Navigator(target => _pages.GetValueOrDefault(target));

        DemoNavigator.Navigate(typeof(DemoHomePage));
    }

    public string Code =>
        "<nav:NavigationMenu Navigator=\"{Binding DemoNavigator}\">\n" +
        "    <!-- Optional header slot, hidden when collapsed -->\n" +
        "    <nav:NavigationMenu.Header>\n" +
        "        <StackPanel Orientation=\"Horizontal\" toolkit:Spacing.Gap=\"8\">\n" +
        "            <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Rocket}\" />\n" +
        "            <TextBlock Text=\"Joufflu\" />\n" +
        "        </StackPanel>\n" +
        "    </nav:NavigationMenu.Header>\n" +
        "    <nav:NavigationTitle>Demo</nav:NavigationTitle>\n" +
        "    <!-- An item targets the type of the page it navigates to -->\n" +
        "    <nav:NavigationItem TargetType=\"{x:Type vm:DemoHomePage}\">\n" +
        "        <nav:NavigationItem.Icon>\n" +
        "            <fonts:FontIcon Text=\"{x:Static fonts:LucideFontIcons.Home}\" />\n" +
        "        </nav:NavigationItem.Icon>\n" +
        "        Home\n" +
        "    </nav:NavigationItem>\n" +
        "    <!-- A group displays like an item but expands to reveal children -->\n" +
        "    <nav:NavigationGroup Header=\"Parent\">\n" +
        "        <nav:NavigationItem TargetType=\"{x:Type vm:DemoSubmenu1Page}\">Submenu 1</nav:NavigationItem>\n" +
        "        <nav:NavigationItem TargetType=\"{x:Type vm:DemoSubmenu2Page}\">Submenu 2</nav:NavigationItem>\n" +
        "        <!-- Groups can nest -->\n" +
        "        <nav:NavigationGroup Header=\"Parent\">\n" +
        "            <!-- … more items … -->\n" +
        "        </nav:NavigationGroup>\n" +
        "    </nav:NavigationGroup>\n" +
        "</nav:NavigationMenu>\n" +
        "\n" +
        "// The navigator turns an item's target type into the page instance\n" +
        "DemoNavigator = new Navigator(target => _pages.GetValueOrDefault(target));";
}
