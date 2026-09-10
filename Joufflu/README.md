# Joufflu

**The core of the Joufflu WPF component library — a design system that makes your
desktop apps look good by default.**

Joufflu gives .NET WPF apps a cohesive design system of themed brushes, dimensions
and layout helpers, plus restyled native controls and a small set of custom ones.
Every control reads its colours through `DynamicResource`, so the whole UI re-themes
live between Light and Dark — no restart, no flicker.

[![Joufflu on NuGet](https://img.shields.io/nuget/v/Joufflu?label=Joufflu&logo=nuget)](https://www.nuget.org/packages/Joufflu)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/ndegheselle/Joufflu/blob/main/LICENSE)

![preview](https://raw.githubusercontent.com/ndegheselle/Joufflu/main/images/preview.PNG)

## What's inside

- 🌗 **Live Light / Dark theming** — flip the theme at runtime and every control follows instantly.
- 🎨 **A real design system** — semantic colours, dimensions, sizing and spacing exposed as override-able resource keys.
- 🪟 **Custom-chrome window & natives** — a themed application shell (`ThemedWindow`) plus restyled built-in WPF controls (buttons, text boxes, combo boxes, data grid, …) that match out of the box.
- 🧰 **Toolkit** — sizing, spacing, tooltip and drop target attached properties in the `Joufflu.Toolkit` namespace, plus `ThemeManager` and live theme customization.

This is the **core package**. Three optional packages build on it:

- [`Joufflu.Feedback`](https://www.nuget.org/packages/Joufflu.Feedback) — badges, spinner & toasts.
- [`Joufflu.Inputs`](https://www.nuget.org/packages/Joufflu.Inputs) — input controls.
- [`Joufflu.Navigation`](https://www.nuget.org/packages/Joufflu.Navigation) — navigation, overlays & paging.

## Getting started

1. Add the package:

   ```sh
   dotnet add package Joufflu
   ```

2. Merge the control styles in `App.xaml`:

   ```xml
   <Application.Resources>
       <ResourceDictionary>
           <ResourceDictionary.MergedDictionaries>
               <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
           </ResourceDictionary.MergedDictionaries>
       </ResourceDictionary>
   </Application.Resources>
   ```

3. Initialize the theme manager once at startup, before the first window shows:

   ```csharp
   // App.xaml.cs — OnStartup
   ThemeManager.Instance.Initialize();
   ```

### Design system

The design system is exposed as resource keys you can override in your own
dictionary (merged **after** the Joufflu resources):

- **Colours / brushes** — `joufflu:Colors.*` and `joufflu:Brushes.*`, including
  the semantic families (primary, secondary, success, info, warning, danger).
- **Dimensions** — `joufflu:Dimensions.*` (corner radius, border thickness,
  spacing, control heights, font sizes and padding per size).

Run the gallery and open **Customize theme** to tweak these interactively and
generate a ready-to-merge dictionary.

## Documentation

📖 Full documentation: <https://ndegheselle.github.io/Joufflu/>

## Acknowledgments

- [Lucide](https://lucide.dev/) icon font
- [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm) for MVVM boilerplate
