---
title: Badge
parent: Feedback
nav_order: 1
---

# Badge

## Variants

A pill themed from the semantic brushes. Set `Variant` (`Default`, `Primary`,
`Secondary`, `Success`, `Info`, `Warning`, `Danger`).

```xml
<feedback:Badge>Default</feedback:Badge>
<feedback:Badge Variant="Primary">Primary</feedback:Badge>
<feedback:Badge Variant="Success">Active</feedback:Badge>
<feedback:Badge Variant="Danger">3</feedback:Badge>
```

## Sizes

Sized through the inherited `Sizing.Size` attached property.

```xml
<feedback:Badge Variant="Primary" toolkit:Sizing.Size="xs">xs</feedback:Badge>
<feedback:Badge Variant="Primary" toolkit:Sizing.Size="lg">lg</feedback:Badge>
```
