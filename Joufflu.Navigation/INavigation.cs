using CommunityToolkit.Mvvm.ComponentModel;
using Joufflu.Navigation.Controls;

namespace Joufflu.Navigation;

/// <summary>
/// Options describing an overlay's chrome (title, close affordances).
/// The overlay content is responsible for rendering its own action buttons.
/// </summary>
public class OverlayOptions : ObservableObject
{
    public string Title { get; set; } = "";

    /// <summary>Shows the close cross in the title bar.</summary>
    public bool ShowCloseButton { get; set; } = true;

    /// <summary>Closes the overlay when the dimmed background behind it is clicked.</summary>
    public bool CloseOnClickAway { get; set; } = true;

    /// <summary>Stretches the overlay to fill the whole surface instead of a centered, sized panel.</summary>
    public bool FullScreen { get; set; } = false;
}

public enum EnumConfirmationType
{
    Neutral,
    Info,
    Success,
    Warning,
    Danger
}

/// <summary>
/// Optional contract for a view model that wants to react to navigation lifecycle events.
/// Navigation works on any object (resolved to a view through implicit <c>DataTemplate</c>s);
/// implementing this interface is only needed when lifecycle callbacks are useful.
/// </summary>
public interface IPage
{
    /// <summary>Called right after the page becomes the active content.</summary>
    void OnNavigatedTo() { }

    /// <summary>Called right after the page stops being the active content.</summary>
    void OnNavigatedFrom() { }
}

/// <summary>
/// Displays a single page (view model) at a time and exposes the current one.
/// </summary>
public interface INavigator
{
    object? CurrentPage { get; }

    void Navigate(object? page);

    /// <summary>Navigates to the page the implementation resolves for <paramref name="type"/>.</summary>
    void Navigate(Type? type);

    event EventHandler<object?>? Navigated;
}

/// <summary>
/// Hosts a stack of modal overlays on top of the current page.
/// </summary>
public interface IOverlayService
{
    /// <summary>
    /// Shows <paramref name="content"/> as a modal overlay and completes when it is closed.
    /// The result carries whatever the closing action provided (<see langword="null"/> when dismissed).
    /// </summary>
    Task<bool?> Show(object content, OverlayOptions? options = null);

    /// <summary>
    /// Show a confirmation overlay with a simple message.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<bool?> Confirm(string message, string title = "", EnumConfirmationType type = EnumConfirmationType.Neutral);

    void Close(OverlayInstance overlay, bool? result = null);

    /// <summary>
    /// Closes the overlay showing <paramref name="content"/>, doing nothing when it isn't on the
    /// stack anymore. Lets content close itself rather than whatever is on top, which is what an
    /// overlay opening another one of its own kind needs.
    /// </summary>
    void Close(object content, bool? result = null);

    void CloseTop(bool? result = null);
}

/// <summary>
/// Optional contract for overlay content that wants to provide its own options
/// (title, action bar, ...) instead of having them supplied at <see cref="IOverlayService.Show"/> time.
/// </summary>
public interface IOverlayContent : IPage
{
    OverlayOptions Options { get; }
}
