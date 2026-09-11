---
title: Toolkit
nav_order: 8
has_children: true
---

# Toolkit

Design-system helpers that shape controls and layouts rather than being controls
themselves. They all live in the `Joufflu.Toolkit` namespace of the core
`Joufflu` package — `Sizing`, `Spacing`, `Derive`, `Tooltip`, `Animate`, `DropTarget` and
`DragSource` — while the theme manager stays in `Joufflu.Themes` and the window
in `Joufflu.Controls`.

- **Sizing** — the `Sizing.Size` and `IsSquare` attached properties.
- **Spacing** — the `Spacing.Gap` attached property for gaps between children.
- **Derived dimensions** — the `Derive.BorderThickness`, `Derive.CornerRadius` and `Derive.Margin` attached properties, with their factors scaling the value side by side, for thicknesses, radii and margins that follow the theme live.
- **Tooltip** — the `Tooltip.Content` and `Tooltip.Placement` attached properties for themed tooltips on any element.
- **Drag and drop** — the `DropTarget.Command`, `IsDragOver` and `Effect` attached properties for turning any element into a drop target, with the `DropData` telling what was dropped and where, and `DragSource.Data`, `AllowedEffects` and `IsDragging` for turning any element into a drag source.
- **Animate** — the `Animate.Bounce` attached property for a looping vertical hop on any element, with `BounceHeight` and `BounceDuration` shaping it.
- **Theme** — `ThemeManager` for System/Light/Dark plus registering custom themes, and how to bind a theme switcher UI to it.
- **Customize theme** — the live theme editor and preset themes.
- **Application shell** — the window styles, the `FullContainer` page host and the overlay/toast containers wrapping the app.

Snippets use the `toolkit` XML namespace for those helpers and `joufflu` for the
design-system keys, plus `nav` and `feedback` for the application shell
containers:

```xml
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"
xmlns:nav="clr-namespace:Joufflu.Navigation.Controls;assembly=Joufflu.Navigation"
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
```
