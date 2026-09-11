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

## Autocomplete

A text box completing what is typed with the most likely item of `ItemsSource` :
the rest of that item is drawn greyed after the caret, <kbd>Tab</kbd> takes it,
and the items starting with what is typed are listed under the box, each drawn
with `ItemTemplate`.

```xml
<inputs:Autocomplete ItemsSource="{Binding Airports}"
                     TextMemberPath="Code"
                     SelectedItem="{Binding SelectedAirport}">
    <inputs:Autocomplete.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="6">
                <TextBlock Text="{Binding Code}" FontWeight="Bold" />
                <TextBlock Text="{Binding City}" Style="{StaticResource Muted}" />
            </StackPanel>
        </DataTemplate>
    </inputs:Autocomplete.ItemTemplate>
</inputs:Autocomplete>
```

| Member | What it does |
|---|---|
| `ItemsSource` | The items completed on and listed, any list like a combo box. |
| `ItemTemplate` | How a suggestion is drawn in the list. |
| `TextMemberPath` | Property holding the text of an item ; its `ToString()` when left empty. |
| `Text` | What is typed, without the greyed completion. |
| `SelectedItem` | The item whose text is the one typed, the typed `string` itself when the list holds nothing spelled that way, `null` while the box is empty. |
| `MaxDropDownHeight` | Height the suggestion list scrolls past, `200` by default. |

Nothing forces the text into the list, which is the point of the control : the
value follows the text as it is typed, so a `SelectedItem` bound to an `object`
receives the item when one matches and the plain string when none does. Leaving
the box spells the text the way the list does when the list holds it, so a
`cdg` typed in a hurry is kept as the `CDG` of the item.

Keys : <kbd>Tab</kbd> takes the completion (and only moves the focus on when
there is nothing greyed left to take), <kbd>Down</kbd> and <kbd>Up</kbd> open
the list and walk it with the completion following the highlight,
<kbd>Enter</kbd> takes the highlighted suggestion and <kbd>Escape</kbd> closes
the list without touching the text.

Matching is done on the start of the item text, ignoring case in the current
culture : what is completed is what is already typed, so the list and the greyed
rest always agree.
