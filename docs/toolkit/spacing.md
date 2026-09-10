---
title: Spacing
parent: Toolkit
nav_order: 2
---

# Spacing

## Spacing.Gap

`Gap` adds even spacing between a panel's children — no per-child margins. A
`StackPanel` spaces along its orientation; `WrapPanel` and `Grid` space on both
axes.

`Gap` is a `Thickness`: a single value is a uniform gap on both axes, a
`"horizontal,vertical"` pair sets each axis independently (`Left` drives
horizontal, `Top` vertical).

```xml
<!-- Gap adds spacing between children of any panel -->
<!-- StackPanel: gap along the Orientation axis -->
<StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="8">
    <Button>One</Button> <Button>Two</Button> <Button>Three</Button>
</StackPanel>

<!-- Gap is a Thickness: one value is uniform, -->
<!-- "horizontal,vertical" sets each axis (Left/Top) -->
<WrapPanel toolkit:Spacing.Gap="8,12">
    ...
</WrapPanel>

<!-- Grid: gap sits between rows and columns -->
<Grid toolkit:Spacing.Gap="12">
    ...
</Grid>
```

{: .note }
> Spacing is implemented through child margins, so any margin set directly on a
> child is overwritten while spacing is active.
