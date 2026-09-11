---
title: File explorer
nav_order: 7
has_children: true
---

# File explorer

A themed file explorer from the optional
[`Joufflu.FileExplorer`](https://www.nuget.org/packages/Joufflu.FileExplorer) package.

- **Explorer** — everything together: control bar, tree, list and a status bar.
- **List** — the nodes of the opened folder, with extra columns of your own.
- **Tree** — the loaded hierarchy, folders only by default.
- **Sources** — what the controls display: the disk, the disk watched live, or a source of your own.

Add the package (`dotnet add package Joufflu.FileExplorer`) and merge its
`Resources.xaml` after the core one and after `Joufflu.Feedback` (the sources report
their errors as toasts). The snippets use these namespaces:

```xml
xmlns:fileExplorer="clr-namespace:Joufflu.FileExplorer.Controls;assembly=Joufflu.FileExplorer"
xmlns:base="clr-namespace:Joufflu.FileExplorer.Controls.Base;assembly=Joufflu.FileExplorer"
xmlns:data="clr-namespace:Joufflu.FileExplorer.Data;assembly=Joufflu.FileExplorer"
xmlns:converters="clr-namespace:Joufflu.FileExplorer.Converters;assembly=Joufflu.FileExplorer"
xmlns:toolkit="clr-namespace:Joufflu.Toolkit;assembly=Joufflu"
```

## Sources

Every control binds a `Source`, an `IExplorerSource`: it holds the opened directory
(`Current`), the loaded hierarchy (`Root`) and the commands acting on the nodes.
`FileSystemSource` reads a folder of this machine:

```csharp
// using Joufflu.FileExplorer.Sources; — toasts is an injected IToastService, null is accepted
public IExplorerSource Source { get; } = new FileSystemSource(@"C:\Projects", toasts);
// ...
await Source.Open();
```

Controls sharing the same source stay in sync: selecting a folder in the tree, double
clicking one in the list or using the breadcrumb opens it for all of them.

{: .note }
> `FileSystemSource` hands copying, moving, renaming and deleting over to the Windows
> shell, so they come with its progress window, its "replace or skip" prompt and its
> recycle bin.

`FileSystemWatcherSource` reads the same disk and keeps up with it, and a source of
your own displays whatever you hand over — see [sources](sources.md).

## Nodes

A source hands over `IExplorerNode`s: a `Name`, a `Path`, a `ModifiedAt` and the
`Parent` directory. `IExplorerDirectory` adds its `Children`, `IExplorerFile` a `Size`.
`FileSystemFile` and `FileSystemDirectory` implement them for the disk, and a type of
your own is displayed like the others — see
[virtual nodes](sources.md#virtual-nodes).

## Interactions

Both the list and the tree come with:

- **Context menus** — keyed on the node type, so a type of your own gets a menu of its own.
- **Drag and drop** — between the controls, and with the Windows explorer.
- **Shortcuts** — `F2` renames, `Ctrl+C` / `Ctrl+X` / `Ctrl+V` copy, cut and paste, `Delete` removes.
- **`VisibleNodes`** — the kinds of node shown: `All`, `Files` or `Directories`.
