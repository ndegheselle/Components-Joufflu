---
title: Application shell
parent: Toolkit
nav_order: 8
---

# Application shell

Styles that shape the whole window rather than a control on a page. The
`Joufflu.Samples` gallery window is the live example.

## Window

The default `Window` style themes a standard WPF window (background, foreground,
native chrome) to match the design system. Applied implicitly to every `Window`.

```xml
<Window ...>
    <!-- inherits the themed Window style automatically -->
</Window>
```

## ThemedWindow

`ThemedWindow` is a custom window with a fully styled title bar and caption
buttons.

```xml
<controls:ThemedWindow xmlns:controls="clr-namespace:Joufflu.Controls;assembly=Joufflu"
                       Title="My app">
    ...
</controls:ThemedWindow>
```

### Title bar over content

Set `AllowContentOverTitleBar="True"` to draw content beneath a transparent title
bar instead of below it. A full-height side panel's background then reaches the
top of the window, while the caption buttons keep floating top-right:

```xml
<controls:ThemedWindow ...
    AllowContentOverTitleBar="True"
    IconVisibility="Collapsed">
    ...
</controls:ThemedWindow>
```

`IconVisibility="Collapsed"` hides the title-bar icon and `TitleVisibility="Collapsed"`
hides the title text — set both to clear the top-left corner for the side panel's
own header.

#### Keeping content clear of the bar

**Why.** In this mode the title bar spans the full window width and stays
draggable across its whole surface — that is what lets you drag the window from
anywhere along the top. But the bar is drawn *over* the content, and though
transparent its drag surface is hit-test visible: any control in the top strip is
covered by it. A side-panel collapse button becomes unclickable; a page's
vertical scrollbar runs behind the caption buttons.

**How.** Reserve a strip of empty space at the top of your content equal to the
bar's height. That height is a fixed value shared through the `Dimensions.TitleBarHeight`
and `Dimensions.TitleBarHeightOffset` that can be used like so :

```xml
<controls:ThemedWindow ...
    xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
    AllowContentOverTitleBar="True">

    <feedback:ToastContainer Toasts="{Binding Toasts}">
        <nav:OverlayContainer Overlays="{Binding Overlays}">
            <DockPanel>
                <nav:NavigationMenu DockPanel.Dock="Left" ... />

                <!-- The page drops below the bar; overlays and toasts stay full-bleed. -->
                <ContentControl
                    Margin="{StaticResource {x:Static joufflu:Dimensions.TitleBarHeightOffset}}"
                    Content="{Binding Navigator.CurrentPage}" />
            </DockPanel>
        </nav:OverlayContainer>
    </feedback:ToastContainer>
</controls:ThemedWindow>
```

The `Joufflu.Samples` gallery window uses this setup. Because the height is a shared
resource, the offset always matches the title bar even if that height changes.

{: .note }
> Offset only the panels whose top strip holds interactive content — a hosted page
> and its scrollbar. Offsetting the page alone leaves the containers around it
> full-bleed, so modal backdrops still cover the whole window.

#### FullContainer

`FullContainer` (`Joufflu.Navigation`) does that placement for you: it puts a page's
header in the title bar strip and scrolls the content below it, so you no longer
apply the offset by hand.

```xml
<nav:FullContainer Header="Profile">
    <!-- the page content, scrolled below the title bar -->
</nav:FullContainer>
```

`Header` is templated as an `H1` by default; set `HeaderTemplate` for anything else
(a title plus a toolbar, for instance).

## OverlayContainer

Wraps the whole application and layers the modal overlay stack above it
(`Joufflu.Navigation`). Because it encapsulates everything — side menu included —
a full screen overlay covers the whole window.

```xml
<nav:OverlayContainer Overlays="{Binding Overlays}">
    <!-- the whole app: menu, page, status bar, ... -->
</nav:OverlayContainer>
```

## ToastContainer

Stacks the toasts in a corner of whatever it wraps — `Position` picks which one
(`Joufflu.Feedback`, usable on its own without the navigation package). Wrap it
*around* the `OverlayContainer` so toasts stay above the overlays too:

```xml
<feedback:ToastContainer Toasts="{Binding Toasts}" Position="BottomRight">
    <nav:OverlayContainer Overlays="{Binding Overlays}">
        <!-- the whole app -->
    </nav:OverlayContainer>
</feedback:ToastContainer>
```

## The current page

The current page needs no dedicated container: a plain `ContentControl` bound to
the navigator renders it, the view resolved by an implicit `DataTemplate`.

```xml
<ContentControl Content="{Binding Navigator.CurrentPage}" />
```
