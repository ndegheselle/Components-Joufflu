---
title: Feedback
nav_order: 6
has_children: true
---

# Feedback

Feedback controls from the optional
[`Joufflu.Feedback`](https://www.nuget.org/packages/Joufflu.Feedback) package,
built on the design system.

- **Badge** — a themed pill in the semantic colours.
- **Spinner** — an indeterminate loading indicator.
- **Toasts** — transient notifications shown from a service, stacked in any corner by a `ToastContainer`.

Add the package (`dotnet add package Joufflu.Feedback`) and merge its
`Resources.xaml` after the core one. The controls live in the
`Joufflu.Feedback.Controls` namespace:

```xml
xmlns:feedback="clr-namespace:Joufflu.Feedback.Controls;assembly=Joufflu.Feedback"
xmlns:fonts="clr-namespace:Joufflu.Assets.Fonts;assembly=Joufflu"
xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"
```
