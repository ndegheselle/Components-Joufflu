using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Feedback;
using Joufflu.Navigation;
using Joufflu.Navigation.Controls;
using Joufflu.Samples.Views.Data;
using Joufflu.Samples.Views.Feedback;
using Joufflu.Samples.Views.FileExplorer;
using Joufflu.Samples.Views.Inputs;
using Joufflu.Samples.Views.Natives.Actions;
using Joufflu.Samples.Views.Natives.DataDisplay;
using Joufflu.Samples.Views.Natives.DataInput;
using Joufflu.Samples.Views.Natives.Feedback;
using Joufflu.Samples.Views.Natives.Layout;
using Joufflu.Samples.Views.Natives.Navigation;
using Joufflu.Samples.Views.Navigation;
using Joufflu.Samples.Views.Themes;
using Joufflu.Samples.Views.Toolkit;

namespace Joufflu.Samples.ViewModels;

/// <summary>
/// Shell view model: owns the shared navigation services and the pages the side menu can reach.
/// The menu items are declared in XAML and point at a page through its type; the
/// <see cref="Navigator"/> turns that type into the page instance via <see cref="ResolvePage"/>.
/// </summary>
public class AppViewModel : ObservableObject
{
    public OverlayService Overlays { get; } = new();

    public ToastService Toasts { get; } = new();

    /// <summary>Pages keyed by their own type, which is what the menu's <c>NavigationItem</c>s target.</summary>
    private readonly Dictionary<Type, object> _pages;

    public Navigator Navigator { get; }

    /// <summary>
    /// Kept accessible so the window's <c>ToastContainer</c> can follow the corner its
    /// position sample picks.
    /// </summary>
    public ToastSamplesViewModel ToastSamples { get; }

    public AppViewModel()
    {
        ToastSamples = new ToastSamplesViewModel(Toasts);

        _pages = new object[]
        {
            // Native controls
            new ButtonSamplesViewModel(),
            new ToggleButtonSamplesViewModel(),

            new TextBoxSamplesViewModel(),
            new ComboBoxSamplesViewModel(),
            new CheckBoxSamplesViewModel(),
            new RadioButtonSamples(),
            new SliderSamplesViewModel(),
            new DatePickerSamplesViewModel(),
            new CalendarSamplesViewModel(),
            new ListBoxSamplesViewModel(),

            new TypographySamplesViewModel(),
            new FontIconSamplesViewModel(),
            new LabelSamples(),
            new ListViewSamplesViewModel(),
            new TreeViewSamplesViewModel(),
            new DataGridSamplesViewModel(),

            new ProgressBarSamplesViewModel(),
            new StatusBarSamples(),

            new CardSamples(),
            new GroupBoxSamples(),
            new ExpanderSamples(),
            new ScrollViewerSamples(),
            new GridSplitterSamples(),

            new MenuSamples(),
            new TabControlSamples(),
            new ToolBarSamples(),
            new HyperlinkSamples(),

            // Inputs (Joufflu.Inputs library)
            new NumericInputsSamplesViewModel(),
            new SelectionInputsSamplesViewModel(),
            new ComboBoxTagsSamplesViewModel(),
            new TextEditableSamplesViewModel(),
            new FilePickerSamplesViewModel(),
            new ColorPickerSamplesViewModel(),
            new DropdownSamplesViewModel(),

            // Navigation (Joufflu.Navigation library)
            new NavigationMenuSamplesViewModel(),
            new OverlaySamplesViewModel(Overlays, Toasts),
            new PagingSamplesViewModel(),

            // Schema (Joufflu.Data library)
            new SchemaSamplesViewModel(),

            // File explorer (Joufflu.FileExplorer library)
            new ExplorerSamplesViewModel(Toasts),
            new ExplorerListSamplesViewModel(Toasts),
            new ExplorerTreeSamplesViewModel(Toasts),

            // Custom controls
            new BadgeSamplesViewModel(),
            new SpinnerSamplesViewModel(),
            ToastSamples,
            new TooltipSamplesViewModel(),

            // Toolkit
            new SizingSamplesViewModel(),
            new SpacingSamplesViewModel(),
            new DropTargetSamplesViewModel(),
            new AnimateSamplesViewModel(),
            new ShellSamples(),

            // Themes
            new ThemeSamplesViewModel(),
            new ThemeTokensViewModel(),
            new ThemeCustomizerViewModel(),
        }.ToDictionary(page => page.GetType());

        Navigator = new Navigator(ResolvePage);

        Navigator.Navigate(typeof(ButtonSamplesViewModel));
    }

    /// <summary>Maps a menu item's target type to its page instance, or null when unknown.</summary>
    private object? ResolvePage(Type target) => _pages.GetValueOrDefault(target);
}
