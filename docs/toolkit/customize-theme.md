---
title: Customize theme
parent: Toolkit
nav_order: 7
---

# Customize theme

The gallery's theme customizer tweaks colours and dimensions with the whole
gallery updating live. Start from a **preset theme**, then copy or save the
generated resource dictionary and merge it into your app **after** the Joufflu
resources to apply it.

The design system is exposed as resource keys you override in a dictionary:
`joufflu:Colors.*` / `joufflu:Brushes.*` for the colours and `joufflu:Dimensions.*`
for the metrics. [Design tokens](tokens.html) lists them all, with the role each one
plays in the control styles.

```xml
<!-- Merge your overrides AFTER the Joufflu resources -->
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
    <ResourceDictionary Source="MyTheme.xaml" />
</ResourceDictionary.MergedDictionaries>
```

Run the **`Joufflu.Samples`** app and open *Customize theme* to generate a
dictionary interactively.
