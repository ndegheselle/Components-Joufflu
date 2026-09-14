---
title: Combo box tags
parent: Inputs
nav_order: 4
---

# Combo box tags

## ComboBoxTags

Builds on `ComboBoxSearch` to select several items, shown as removable tags. Set
`AllowAdd="True"` to let users add values not in the source list.

```xml
<inputs:ComboBoxTags AllowAdd="True"
                     ItemsSource="{Binding Countries}"
                     SelectedItems="{Binding SelectedCountries}" />
```
