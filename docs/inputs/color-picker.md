---
title: Color picker
parent: Inputs
nav_order: 7
---

# Color picker

## ColorPicker

Edits a colour through an editable hex value and a popup with a saturation square,
hue and alpha sliders. Two-way bindable through `Color`
(`System.Windows.Media.Color`).

```xml
<inputs:ColorPicker Color="{Binding Color, Mode=TwoWay}" />
```

The popup provides:

- a **saturation / brightness** square you can drag,
- a **hue** slider,
- an **alpha** slider (checkerboard behind a transparent → opaque ramp),
- an editable **hex** field and an **opacity** percentage.
