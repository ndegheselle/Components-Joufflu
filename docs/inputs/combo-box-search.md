---
title: Search combo box
parent: Inputs
nav_order: 3
---

# Search combo box

An editable combo box that filters its items as you type. `FilterMemberPath` acts
like `DisplayMemberPath` for the filter.

The chevron on the right opens the list, as do <kbd>Up</kbd> and <kbd>Down</kbd>.
Opening it that way shows every choice, even once an item is selected : the text
left behind by a selection is not a search, so it does not filter the list down to
the item already picked.

```xml
<inputs:ComboBoxSearch ItemsSource="{Binding Countries}"
                       SelectedItem="{Binding SelectedCountry}" />
```
