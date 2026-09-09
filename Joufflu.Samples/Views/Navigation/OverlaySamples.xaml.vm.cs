using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.Feedback;
using Joufflu.Navigation;

namespace Joufflu.Samples.Views.Navigation;

public class OverlaySamplesViewModel : ObservableObject
{
    private readonly IOverlayService _overlays;
    private readonly IToastService _toasts;

    public IRelayCommand OpenSimpleCommand { get; }

    public IRelayCommand OpenConfirmCommand { get; }

    public IRelayCommand OpenFormCommand { get; }

    public IRelayCommand OpenFullScreenCommand { get; }

    public IRelayCommand OpenStackedCommand { get; }

    public OverlaySamplesViewModel(IOverlayService overlays, IToastService toasts)
    {
        _overlays = overlays;
        _toasts = toasts;

        OpenSimpleCommand = new RelayCommand(OpenSimple);
        OpenConfirmCommand = new AsyncRelayCommand(OpenConfirmAsync);
        OpenFormCommand = new AsyncRelayCommand(OpenFormAsync);
        OpenFullScreenCommand = new RelayCommand(OpenFullScreen);
        OpenStackedCommand = new RelayCommand(OpenStacked);
    }

    private void OpenSimple()
    {
        var content = new ConfirmViewModel("This is a simple overlay. Click the cross or the dimmed background to dismiss it.");
        _overlays.Show(content, new OverlayOptions { Title = "Simple overlay" });
    }

    private async Task OpenConfirmAsync()
    {
        bool? result = await _overlays.Confirm("Delete the selected item? This action cannot be undone.", "Please confirm", EnumConfirmationType.Danger);
        if (result == true)
            _toasts.Success("Item deleted.", "Confirmed");
        else
            _toasts.Info("Cancelled.");
    }

    private async Task OpenFormAsync()
    {
        // The form carries its own options and hands back what was typed, being an OverlayViewModel.
        string? name = await OverlayViewModel<string>.ShowAsync(new SampleFormViewModel(_overlays));
        if (name != null)
            _toasts.Success($"Saved name: {name}", "Profile");
    }

    private void OpenFullScreen()
    {
        var content = new ConfirmViewModel("This overlay fills the whole surface. Use the close cross to dismiss it.");
        _overlays.Show(content, new OverlayOptions { Title = "Full screen overlay", FullScreen = true });
    }

    private void OpenStacked()
    {
        _overlays.Show(
            new ConfirmViewModel("First overlay. Open another one on top to see overlays stack."),
            new OverlayOptions { Title = "Overlay 1" });

        _overlays.Show(
            new ConfirmViewModel("Second overlay, stacked above the first. Close me to reveal it."),
            new OverlayOptions { Title = "Overlay 2" });
    }

    public string Code =>
        "// Content deriving from OverlayViewModel<T> owns its buttons and its\n" +
        "// options, closes itself, and hands back what it was validated with.\n" +
        "var form = new SampleFormViewModel(overlays);\n" +
        "string? name = await OverlayViewModel<string>.ShowAsync(form);\n" +
        "\n" +
        "// Any object works too, its options being given at show time\n" +
        "bool? result = await overlays.Show(content, new OverlayOptions { Title = \"...\" });\n" +
        "\n" +
        "// Standard confirmation, no content of your own\n" +
        "bool? confirmed = await overlays.Confirm(\"Delete the selected item?\", \"Please confirm\");";
}

/// <summary>Simple overlay content showing a message.</summary>
public class ConfirmViewModel : ObservableObject
{
    public ConfirmViewModel(string message) => Message = message;

    public string Message { get; }
}

/// <summary>
/// Overlay content with an editable field, used by the form overlay demo. Deriving from
/// <see cref="OverlayViewModel{TResult}"/> gives it its options, its cancel command and the result
/// it is awaited for, so it only writes the save.
/// </summary>
public partial class SampleFormViewModel : OverlayViewModel<string>
{
    private string _name = "Ada Lovelace";
    private bool _subscribe = true;

    public SampleFormViewModel(IOverlayService overlays) : base(overlays)
    {
        Options.Title = "Edit profile";
        Options.CloseOnClickAway = false;
    }

    public string Name { get => _name; set => SetProperty(ref _name, value); }

    public bool Subscribe { get => _subscribe; set => SetProperty(ref _subscribe, value); }

    [RelayCommand]
    private void Save() => Close(Name);
}
