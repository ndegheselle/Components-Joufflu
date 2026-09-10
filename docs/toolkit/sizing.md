---
title: Sizing
parent: Toolkit
nav_order: 1
---

# Sizing

## Sizing.Size

The inherited `Size` attached property (`xs` / `sm` / `md` / `lg`) scales height,
font size and padding. Since it inherits, setting it on a panel sizes every child.

```xml
<!-- Attached property drives height, font size and padding -->
<Button toolkit:Sizing.Size="xs" />
<Button toolkit:Sizing.Size="sm" />
<Button toolkit:Sizing.Size="md" />  <!-- default -->
<Button toolkit:Sizing.Size="lg" />

<!-- Size is inherited, so a panel sets it for every child -->
<StackPanel toolkit:Sizing.Size="lg">
    <TextBox /> <ComboBox /> <Button>OK</Button>
</StackPanel>
```

Other controls honor the same attached property — `TreeView` scales its expander,
indentation and text together:

```xml
<TreeView toolkit:Sizing.Size="lg" ItemsSource="{Binding Tree}" />
```

## Sizing.IsSquare

`IsSquare` forces equal width and height, for single-icon buttons.

```xml
<Button toolkit:Sizing.IsSquare="True"
        toolkit:Sizing.Size="lg">
    <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.Leaf}" />
</Button>
```
