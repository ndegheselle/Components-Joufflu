---
title: Tooltip
parent: Toolkit
nav_order: 3
---

# Tooltip

## Tooltip.Content

Set `Tooltip.Content` on **any** element to attach a themed tooltip that shows
instantly on hover. It builds on the native `ToolTip`, so screen-edge flipping and
fade-in keep working, but unlike the native tooltip it appears with no delay.

`Tooltip.Content` is an `object`: it accepts a plain string or arbitrary XAML.

```xml
<!-- A string tooltip -->
<Button Content="Save" toolkit:Tooltip.Content="Save your changes" />

<!-- Arbitrary content: icons, panels, anything -->
<Button Content="Rich tooltip">
    <toolkit:Tooltip.Content>
        <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="8">
            <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Info}" />
            <TextBlock Text="Arbitrary content, icons included." />
        </StackPanel>
    </toolkit:Tooltip.Content>
</Button>
```

## Tooltip.Placement

`Tooltip.Placement` chooses which side the tooltip sits on — `Top` (default),
`Bottom`, `Left` or `Right`. It is centered on the shared edge, kept a small gap
clear of the element, and still flips automatically when it would run off a screen
edge.

```xml
<Button Content="Top"    toolkit:Tooltip.Content="Placed above" toolkit:Tooltip.Placement="Top" />
<Button Content="Bottom" toolkit:Tooltip.Content="Placed below" toolkit:Tooltip.Placement="Bottom" />
<Button Content="Left"   toolkit:Tooltip.Content="Placed left"  toolkit:Tooltip.Placement="Left" />
<Button Content="Right"  toolkit:Tooltip.Content="Placed right" toolkit:Tooltip.Placement="Right" />
```

{: .note }
> Setting `Tooltip.Content` to `null` removes the tooltip, so it composes with
> triggers — the [navigation menu](../navigation/navigation-menu.md) attaches a
> right-placed tooltip to each item only while the menu is collapsed.
