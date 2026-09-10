---
title: Card
parent: Native controls
nav_order: 3
---

# Card

`Card` and `CardSecondary` are named `Border` styles that provide the app's
padded, rounded surfaces. Apply them through the `Style` property of a plain
`<Border>` and put any content inside.

## Card

The primary surface for grouping content: a `Background100` fill, a themed border,
rounded corners and the standard spacing as padding.

```xml
<Border Style="{StaticResource Card}">
    <StackPanel toolkit:Spacing.Gap="4">
        <TextBlock FontWeight="Bold" Text="Card" />
        <TextBlock
            Foreground="{DynamicResource {x:Static joufflu:Brushes.Foreground100Brush}}"
            Text="The primary surface for grouping content."
            TextWrapping="Wrap" />
    </StackPanel>
</Border>
```

## CardSecondary

A subtly inset panel meant to sit **inside** a `Card`. It uses the deeper
`Background` fill so it reads as a recessed area against the card surface.

```xml
<Border Style="{StaticResource Card}">
    <StackPanel toolkit:Spacing.Gap="8">
        <TextBlock FontWeight="Bold" Text="Nested" />
        <Border Style="{StaticResource CardSecondary}">
            <TextBlock Text="CardSecondary inside a Card." TextWrapping="Wrap" />
        </Border>
    </StackPanel>
</Border>
```

{: .note }
> Both styles derive their fill, border, corner radius and padding from the theme
> resources, so they follow Light / Dark and any custom theme automatically.
