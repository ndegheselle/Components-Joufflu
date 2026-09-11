---
title: Home
nav_order: 1
---

# Joufflu WPF Components

**A modern WPF component library that makes your desktop apps look good by
default.**

WPF inputs and reusable components for .NET, built on a design system of themed
brushes, dimensions and layout helpers. Every control reads its colours through
`DynamicResource`, so the whole UI re-themes live between Light and Dark — no
restart, no flicker.

These pages mirror the controls in the **`Joufflu.Samples`** gallery app — same
titles, descriptions and code snippets. Each control page lists example markup you
can paste into your views.

{: .note }
> **Using an LLM agent?** Point it at the machine-readable
> [`llms.txt`](https://raw.githubusercontent.com/ndegheselle/Joufflu/main/docs/llms.txt)
> index — a single file linking every page's raw Markdown source. Use that raw URL rather
> than the rendered site, which can return `403` to automated fetchers.

## Highlights

- 🌗 **Live Light / Dark theming** — flip the theme at runtime and every control follows instantly.
- 🎨 **A real design system** — semantic colours, dimensions, sizing and spacing exposed as override-able resource keys.
- 🧩 **Ready-to-use inputs** — numeric, decimal and timespan pickers, searchable and tag combo boxes, file and colour pickers, inline-editable text, dropdowns.
- 🧭 **Navigation & overlays** — a navigation menu, a view-model-first page container, awaitable modal dialogs and a paging selector.
- 🗂️ **A file explorer** — breadcrumb, folder tree and file list sharing a source, with shell-backed file operations and nodes of your own.
- 🪟 **Custom-chrome window & natives** — a themed application shell plus restyled built-in WPF controls that match out of the box.
- 📦 **Modular packages** — take just the core styles, or add inputs and navigation only where you need them.

{: .note }
> The gallery's **Natives** group — WPF's built-in controls restyled to match the
> design system (text boxes, combo boxes, data grid, …) — is mostly undocumented
> here; only natives with named style variants are (see
> [Native controls](natives/index.md)). Otherwise these pages cover the
> custom controls the library adds.

## Sections

| Section | What's inside |
|---|---|
| [Native controls](natives/index.md) | Buttons (solid, soft & outline variants), `FontIcon`, the `Card` styles and the typography scale |
| [Inputs](inputs/index.md) | `NumericUpDown`, `DecimalUpDown`, `TimeSpanPicker`, `FormatTextBox`, `Search`, `ComboBoxSearch`, `Autocomplete`, `ComboBoxTags`, `TextEditable`, `FilePicker`, `ColorPicker`, `Dropdown` |
| [Navigation](navigation/index.md) | `NavigationMenu`, overlays (modal dialogs), `Paging`, `FullContainer` |
| [Feedback](feedback/index.md) | `Badge`, `Spinner`, toasts (`Joufflu.Feedback`) |
| [File explorer](file-explorer/index.md) | `Explorer`, `ExplorerList`, `ExplorerTree` and their sources (`Joufflu.FileExplorer`) |
| [Toolkit](toolkit/index.md) | Sizing, spacing, derived dimensions, tooltips, animation, drag and drop, the design tokens, theme customization, application shell |

## Getting started

1. Add the packages you need. `Joufflu` is the core (styles & theming);
   `Joufflu.Feedback`, `Joufflu.FileExplorer`, `Joufflu.Inputs` and
   `Joufflu.Navigation` are optional and all build on it:

   ```sh
   dotnet add package Joufflu
   dotnet add package Joufflu.Feedback      # optional: badges, spinner, toasts
   dotnet add package Joufflu.FileExplorer  # optional: file explorer (needs Joufflu.Feedback)
   dotnet add package Joufflu.Inputs        # optional: input controls
   dotnet add package Joufflu.Navigation    # optional: navigation & overlays
   ```

2. Merge the control styles in `App.xaml` so the themed styles and the
   design-system keys (`joufflu:Brushes`, `joufflu:Dimensions`,
   `toolkit:Spacing`, `toolkit:Sizing`) are available:

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

4. Use the controls as shown in the pages above.

New to the library? The [Tutorial](tutorial.md) builds a full
navigable app shell — side menu, pages, modals and toasts — from scratch.

To explore everything interactively, run the **`Joufflu.Samples`** project.
