# Joufflu.FileExplorer

**A file explorer for [Joufflu](https://www.nuget.org/packages/Joufflu).**

A themed explorer of a folder tree — control bar, folder tree and file list — driven
by a source you can replace or extend with nodes of your own.

[![Joufflu.FileExplorer on NuGet](https://img.shields.io/nuget/v/Joufflu.FileExplorer?label=Joufflu.FileExplorer&logo=nuget)](https://www.nuget.org/packages/Joufflu.FileExplorer)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue)](https://github.com/ndegheselle/Joufflu/blob/main/LICENSE)

## What's inside

| Piece | Purpose |
|---|---|
| `Explorer` | Everything together: control bar, tree, list and a status bar counting the items and the selected ones. |
| `ExplorerList` | The nodes of the opened folder, with their date and size. Adds columns of your own through `ExtraColumns`. |
| `ExplorerTree` | The loaded hierarchy, folders only by default. |
| `ExplorerControlBar` | Breadcrumb of the opened path, each folder reopened by a click, plus a button going up. |
| `IExplorerSource` | What the controls display and navigate through. `FileSystemSource` reads a folder of this machine, `FileSystemWatcherSource` keeps up with the changes made to it. |
| `IExplorerNode` | A node of a source: `FileSystemFile`, `FileSystemDirectory`, or a type of your own. |

Every control shares the same `Source`, which is what keeps them in sync: opening a
folder anywhere opens it for the others.

Copying, moving, renaming and deleting are handed over to the Windows shell, so they
come with its progress window, its "replace or skip" prompt and its recycle bin. Nodes
are dragged and dropped between controls and with the Windows explorer, and the usual
shortcuts work: `F2` renames, `Ctrl+C` / `Ctrl+X` / `Ctrl+V` copy, cut and paste,
`Delete` removes.

## Getting started

`Joufflu.FileExplorer` builds on the core `Joufflu` package and on
`Joufflu.Feedback` (the sources report their errors as toasts). Both come along as
dependencies:

```sh
dotnet add package Joufflu.FileExplorer
```

Merge the styles in `App.xaml`, after the core ones, and initialize the theme manager
once at startup:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Joufflu;component/Resources.xaml" />
            <ResourceDictionary Source="pack://application:,,,/Joufflu.Feedback;component/Resources.xaml" />
            <ResourceDictionary Source="pack://application:,,,/Joufflu.FileExplorer;component/Resources.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

```csharp
// App.xaml.cs — OnStartup
ThemeManager.Instance.Initialize();
```

Then open a source in your view model:

```csharp
// using Joufflu.FileExplorer.Sources; — toasts is an injected IToastService, null is accepted
public IExplorerSource Source { get; } = new FileSystemSource(@"C:\Projects", toasts);
// ...
await Source.Open();
```

And bind it in your view:

```xml
xmlns:fileExplorer="clr-namespace:Joufflu.FileExplorer.Controls;assembly=Joufflu.FileExplorer"
```

```xml
<fileExplorer:Explorer Source="{Binding Source}" />
```

## Documentation

📖 Full documentation: <https://ndegheselle.github.io/Joufflu/>
