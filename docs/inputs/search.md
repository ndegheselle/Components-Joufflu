---
title: Search
parent: Inputs
nav_order: 2
---

# Search

A text box that debounces before raising `SearchChanged`, to limit calls to an
API or database. <kbd>Escape</kbd> clears it.

```xml
<inputs:Search />
```

```csharp
// code-behind: search.SearchChanged += text => Filter(text);
```
