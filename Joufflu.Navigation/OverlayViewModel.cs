using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Navigation;

/// <summary>
/// Base for content shown as a modal overlay : it carries its own <see cref="Options"/>, so
/// <see cref="IOverlayService.Show"/> takes nothing but the content, and it closes itself rather
/// than whatever is on top of the stack.
/// <para>
/// Use <see cref="OverlayViewModel{TResult}"/> when the overlay is awaited for something it picks
/// or builds; this one is for the overlays only worth a yes or a no.
/// </para>
/// </summary>
public abstract partial class OverlayViewModel : ObservableObject, IOverlayContent
{
    /// <summary>
    /// The chrome the overlay is shown with. Set in the constructor of the content : the options
    /// are read once, as the overlay is pushed on the stack.
    /// </summary>
    public OverlayOptions Options { get; } = new();

    protected IOverlayService Overlays { get; }

    protected OverlayViewModel(IOverlayService overlays)
    {
        Overlays = overlays;
    }

    /// <summary>
    /// Close this overlay, handing [result] back to whoever is awaiting it.
    /// </summary>
    protected void Close(bool? result) => Overlays.Close(this, result);

    /// <summary>
    /// Close without a result, which is what dismissing the overlay does.
    /// </summary>
    [RelayCommand]
    protected void Cancel() => Close(false);
}

/// <summary>
/// An <see cref="OverlayViewModel"/> awaited for what it picks or builds : it closes with a
/// <typeparamref name="TResult"/>, which <see cref="ShowAsync"/> hands back to its caller.
/// </summary>
public abstract class OverlayViewModel<TResult> : OverlayViewModel
{
    /// <summary>
    /// What the overlay was closed with, only set once it has been validated.
    /// </summary>
    public TResult? Result { get; private set; }

    protected OverlayViewModel(IOverlayService overlays) : base(overlays)
    { }

    /// <summary>
    /// Close this overlay with [result], which is what a validation does.
    /// </summary>
    protected void Close(TResult result)
    {
        Result = result;
        Close(true);
    }

    /// <summary>
    /// Show [viewModel] and wait for it to be closed, its result being handed back — the default of
    /// <typeparamref name="TResult"/> when it was cancelled or dismissed.
    /// </summary>
    public static async Task<TResult?> ShowAsync(OverlayViewModel<TResult> viewModel)
        => await viewModel.Overlays.Show(viewModel) == true ? viewModel.Result : default;
}
