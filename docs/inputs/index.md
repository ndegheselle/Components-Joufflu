---
title: Inputs
nav_order: 4
has_children: true
---

# Inputs

Inputs from the `Joufflu.Inputs` library. Two-way bindable like the built-in WPF
inputs.

- **Numeric & format inputs** — `NumericUpDown`, `DecimalUpDown`,
  `TimeSpanPicker` and the underlying `FormatTextBox`.
- **Search** — a debounced `Search` box.
- **Search combo box** — the searchable `ComboBoxSearch`.
- **Combo box tags** — multi-selection built on `ComboBoxSearch`.
- **Text editable** — a value that becomes editable on demand.
- **File picker** — pick a file through the system dialog.
- **Color picker** — pick a colour with a saturation square, hue and alpha
  sliders.
- **Dropdown** — attached properties opening a popup of any content off a `ToggleButton`.

All snippets assume the `inputs` and `joufflu` XML namespaces:

```xml
xmlns:inputs="clr-namespace:Joufflu.Inputs.Controls;assembly=Joufflu.Inputs"
xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"
```
