---
title: Derived dimensions
parent: Toolkit
nav_order: 9
---

# Derived dimensions

A `Thickness` or a `CornerRadius` declared in a `ResourceDictionary` is baked at
parse time. Its `Left`/`Top`/`Right`/`Bottom` (or `TopLeft`/`TopRight`/…) are
plain CLR properties, not dependency properties, so they can only be fed with
`StaticResource` and never follow a later change of the scalar they were built
from:

```xml
<!-- Baked at load: editing Dimensions.Thickness at runtime does nothing here -->
<Thickness x:Key="MenuBorderThickness"
           Right="{StaticResource {x:Static joufflu:Dimensions.Thickness}}" />
```

`Derive.BorderThickness`, `Derive.CornerRadius` and `Derive.Margin` build the
value on the element instead, from a real `DynamicResource`. A scalar edited at
runtime — by the [theme customizer](customize-theme.html), for instance — flows
straight through, and no derived resource key has to be declared or re-pushed by
hand.

## Factors

Each of the three properties has a matching factor, a `Thickness` (or a
`CornerRadius`) whose components multiply the derived value side by side:

| Factor component | Result |
| --- | --- |
| `0` | The side or corner is dropped |
| `1` | It is kept as is |
| anything else | It is scaled — `2` for a doubled margin, `0.5` for a halved radius |

The default is `1,1,1,1`, so the derived value is applied whole.

## Derive.BorderThickness

Point `Derive.BorderThickness` at a resource key and pick the sides with
`Derive.BorderThicknessFactor`, in the usual `Left,Top,Right,Bottom` order.

```xml
<!-- Right edge only, following Dimensions.Thickness live -->
<Border toolkit:Derive.BorderThickness="{x:Static joufflu:Dimensions.Thickness}"
        toolkit:Derive.BorderThicknessFactor="0,0,1,0" />

<!-- Open at the bottom, with a doubled top edge -->
<Border toolkit:Derive.BorderThickness="{x:Static joufflu:Dimensions.Thickness}"
        toolkit:Derive.BorderThicknessFactor="1,2,1,0" />
```

Both properties work in a `Style` setter, so a control can derive its own border
without a keyed `Thickness`:

```xml
<Style TargetType="{x:Type nav:NavigationMenu}">
    <Setter Property="toolkit:Derive.BorderThickness" Value="{x:Static joufflu:Dimensions.Thickness}" />
    <Setter Property="toolkit:Derive.BorderThicknessFactor" Value="0,0,1,0" />
</Style>
```

> **In a style, the fed property stops being overridable.** The derived value is
> pushed with `SetCurrentValue`, which outranks a style setter — and even a local
> value. So do not also set the property it feeds (here `BorderThickness`) in the
> same style, move any `Style.Triggers` that change it onto the `Derive` property,
> and expect that a consumer can no longer override it. When the fed property has
> to stay overridable — a control's `Padding`, say — give it a plain keyed default
> instead of deriving it.

## Derive.CornerRadius

Same shape, with `Derive.CornerRadiusFactor` in the
`TopLeft,TopRight,BottomRight,BottomLeft` order.

```xml
<!-- Top corners only, matching the border it sits in -->
<Border toolkit:Derive.CornerRadius="{x:Static joufflu:Dimensions.Radius}"
        toolkit:Derive.CornerRadiusFactor="1,1,0,0" />
```

## Derive.Margin

Same shape again, on any `FrameworkElement`, with `Derive.MarginFactor`.

```xml
<!-- Spaced everywhere but the top -->
<Border toolkit:Derive.Margin="{x:Static joufflu:Dimensions.Spacing}"
        toolkit:Derive.MarginFactor="1,0,1,1" />
```

## Notes

- The source resource may be a `double` (the usual case, spread over every side
  or corner before scaling) or an already built `Thickness` / `CornerRadius`,
  whose own sides are then scaled.
- `Derive.BorderThickness` applies to `Border` and to any `Control`;
  `Derive.CornerRadius` applies to `Border`; `Derive.Margin` to any
  `FrameworkElement`. Anything else throws.
- The derived value is written with `SetCurrentValue`, which outranks a style
  setter : a style deriving a value should not also set the property it feeds,
  and the `Style.Triggers` meant to change it have to move to the `Derive`
  property instead. An animation or a template trigger still takes over. So
  `Derive` fits a value the control owns — a `Border` inside a template — not one
  a consumer overrides : a property like a text input's `Padding` carries a plain
  keyed default (`Dimensions.InputPadding*`) instead, which a local value or a
  style still beats.

Snippets use these XML namespaces:

```xml
xmlns:joufflu="clr-namespace:Joufflu;assembly=Joufflu"
xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"
```
