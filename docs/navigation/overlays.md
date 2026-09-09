---
title: Overlays
parent: Navigation
nav_order: 2
---

# Overlays

Modal content shown above the page: a title bar with a close cross and a content
area. Multiple overlays stack.

## Hosting the overlays

Overlays are rendered by an `OverlayContainer` bound to the `OverlayService` you
show them from. Wrap the whole window content in it so an overlay covers
everything — side menu included:

```xml
<nav:OverlayContainer Overlays="{Binding Overlays}">
    <!-- the whole app -->
</nav:OverlayContainer>
```

Toasts have their own [`ToastContainer`](../feedback/toasts.md); wrap it around
this one to keep them above the overlays.

## Showing an overlay

The overlay content owns its buttons and closes itself. `Show` completes when the overlay closes and returns the
result it closed with - `null` when dismissed.

`OverlayOptions` exposes `Title`, `ShowCloseButton`, `CloseOnClickAway` (set
`false` to force the user through the action buttons) and `FullScreen`.

### `OverlayViewModel`

Deriving overlay content from `OverlayViewModel` gives it its own `Options` (set
in the constructor, read once when the overlay is pushed), a `CancelCommand`, and
a `Close` that closes itself:

```csharp
public class SampleFormViewModel : OverlayViewModel<string>
{
    public SampleFormViewModel(IOverlayService overlays) : base(overlays)
    {
        Options.Title = "Edit profile";
        Options.CloseOnClickAway = false;
    }

    public string Name { get; set; } = "Joe Doe";

    [RelayCommand]
    private void Save() => Close(Name);
}
```

Use the non-generic `OverlayViewModel` for overlays only worth a yes or a no. 
Use `OverlayViewModel<TResult>` for overlays awaited, its static `ShowAsync` shows the 
content and hands back what it was closed with, the default of `TResult` when it was 
cancelled or dismissed:

```csharp
string? name = await OverlayViewModel<string>.ShowAsync(new SampleFormViewModel(overlays));
```

 Use

### Any object

Content doesn't have to derive from `OverlayViewModel` — any object works, with
the options given at show time and closed via the service:

```csharp
var options = new OverlayOptions { Title = "Edit profile" };
bool? result = await overlays.Show(content, options);
```

Content implementing `IOverlayContent` carries its own `Options`, so `Show` can be
called without them; that's what `OverlayViewModel` does. Close it with
`overlays.Close(content, result)` — rather than `CloseTop`, so content opening
another overlay of its own kind still closes itself and not whichever is on top.

## Standard confirmation

For the common "are you sure?" case, `Confirm` shows a built-in overlay — the
message plus a *Cancel* / *Confirm* pair — with no content of your own:

```csharp
bool? result = await overlays.Confirm(
    "Delete the selected item? This action cannot be undone.",
    "Please confirm",
    EnumConfirmationType.Danger);

if (result == true)
    // confirmed
```

`EnumConfirmationType` colours the confirm button in the matching semantic style:
`Neutral` (the default), `Info`, `Success`, `Warning` or `Danger`.

## Full screen

`FullScreen = true` stretches the overlay over the entire container instead of a
centered, sized panel — a whole-window editor or wizard rather than a dialog:

```csharp
await overlays.Show(content, new OverlayOptions { Title = "Edit", FullScreen = true });
```
