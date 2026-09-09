# Joufflu.Navigation

**Navigation and modal overlays for [Joufflu](https://www.nuget.org/packages/Joufflu).**

A small, view-model-first navigation layer that follows the Joufflu design system:
a navigation menu, a page container, awaitable modal dialogs and a paging selector.

[![Joufflu.Navigation on NuGet](https://img.shields.io/nuget/v/Joufflu.Navigation?label=Joufflu.Navigation&logo=nuget)](https://www.nuget.org/packages/Joufflu.Navigation)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/ndegheselle/Joufflu/blob/main/LICENSE)

## What's inside

| Piece | Purpose |
|---|---|
| `NavigationMenu` | A themed navigation menu for moving between sections. |
| `OverlayContainer` | Wraps the whole app and layers the modal overlays above it. |
| Modal overlays | Awaitable modal dialogs driven by a `Navigator`, so you can `await` a dialog and get its result — plus `Confirm()` for the standard confirmation. |
| `OverlayViewModel` | Base for overlay content : it carries its own `OverlayOptions`, gets a `CancelCommand`, and closes itself rather than whatever is on top of the stack. `OverlayViewModel<TResult>` adds the result the overlay is awaited for, and a static `ShowAsync` handing it back. |
| `Paging` | A page selector for large sets of data — `Total`, `PageNumber` and `Capacity` (items per page), plus the displayed range. |

## Getting started

`Joufflu.Navigation` builds on the core `Joufflu` package. Add it (the core comes
along as a dependency):

```sh
dotnet add package Joufflu.Navigation
```

Merge the Joufflu control styles in `App.xaml` and initialize the theme manager
once at startup:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

```csharp
// App.xaml.cs — OnStartup
ThemeManager.Instance.Initialize();
```

Then wire up a `Navigator`, render the current page with a `ContentControl` bound to
`Navigator.CurrentPage`, wrap your window content in an `OverlayContainer`, and drive
navigation and modal overlays from your view models.

To page a list or a `DataGrid`, bind `Paging` two way and load the matching slice
when the view model is notified:

```xml
<nav:Paging
    Total="{Binding Total}"
    PageNumber="{Binding PageNumber, Mode=TwoWay}"
    Capacity="{Binding Capacity, Mode=TwoWay}" />
```

## Documentation

📖 Full documentation: <https://ndegheselle.github.io/Joufflu/>
