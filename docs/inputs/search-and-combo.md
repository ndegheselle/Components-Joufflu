---
title: Search & combo
parent: Inputs
nav_order: 2
---

# Search & combo

## Search

A text box that debounces before raising `SearchChanged`, to limit calls to an
API or database. <kbd>Escape</kbd> clears it.

```xml
<inputs:Search />
```

```csharp
// code-behind: search.SearchChanged += text => Filter(text);
```

## ComboBoxSearch

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
