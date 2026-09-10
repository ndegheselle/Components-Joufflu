---
title: Icons
parent: Native controls
nav_order: 2
---

# Icons

`FontIcon` renders a glyph from the embedded [Lucide](https://lucide.dev) icon
font. It derives from `TextBlock`, so it takes a foreground brush, aligns like
text and can be dropped anywhere content is expected — inside a button, a label
or a stack panel.

The glyphs are exposed as constants on `LucideFontIcons`; set one on the `Text`
property through `x:Static`.

```xml
<!-- Declare the namespaces once on the root element -->
<UserControl
    xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
    xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
    xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu">

    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Leaf}" />
</UserControl>
```

## Sizes

The size follows the inherited `Sizing.Size` attached property (`xs` / `sm` /
`md` / `lg` / `xl`), so an icon scales with the control — or the panel — it sits
in. `md` is the default.

```xml
<fonts:FontIcon toolkit:Sizing.Size="xs" Text="{x:Static fonts:LucideFontIcons.Leaf}" />
<fonts:FontIcon toolkit:Sizing.Size="sm" Text="{x:Static fonts:LucideFontIcons.Leaf}" />
<fonts:FontIcon toolkit:Sizing.Size="md" Text="{x:Static fonts:LucideFontIcons.Leaf}" />
<fonts:FontIcon toolkit:Sizing.Size="lg" Text="{x:Static fonts:LucideFontIcons.Leaf}" />
```

## Colour

Foreground is a normal `TextBlock` property, so point it at any design-system
brush to give an icon a semantic hue.

```xml
<fonts:FontIcon
    Foreground="{DynamicResource {x:Static joufflu:Brushes.PrimaryBrush}}"
    Text="{x:Static fonts:LucideFontIcons.Leaf}" />
<fonts:FontIcon
    Foreground="{DynamicResource {x:Static joufflu:Brushes.SuccessBrush}}"
    Text="{x:Static fonts:LucideFontIcons.Leaf}" />
<fonts:FontIcon
    Foreground="{DynamicResource {x:Static joufflu:Brushes.DangerBrush}}"
    Text="{x:Static fonts:LucideFontIcons.Leaf}" />
```

## In a button

Pair an icon with `Sizing.IsSquare` for a compact, single-icon button.
See [Buttons](buttons.md#icon-buttons).

```xml
<Button toolkit:Sizing.IsSquare="True" Style="{StaticResource PrimaryButton}">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Plus}" />
</Button>
```
