---
title: Sources
parent: File explorer
nav_order: 4
---

# Sources

Every control binds a `Source`, an `IExplorerSource`: it holds the opened directory
(`Current`), the loaded hierarchy (`Root`) and the commands the menus and the shortcuts
act with. Controls sharing the same source stay in sync — selecting a folder in the
tree, double clicking one in the list or using the breadcrumb opens it for all of them.

```csharp
// using Joufflu.FileExplorer.Sources; — toasts is an injected IToastService, null is accepted
public IExplorerSource Source { get; } = new FileSystemSource(@"C:\Projects", toasts);
// ...
await Source.Open();
```

## FileSystemSource

A directory of this machine, read when it is opened: a directory is loaded along with
two levels of sub directories, so the tree shows an expander without reading the disk
again.

{: .note }
> Copying, moving, renaming and deleting are handed over to the Windows shell, so they
> come with its progress window, its "replace or skip" prompt and its recycle bin. A
> whole selection goes in one call, which is what keeps the windows of the shell to one
> for the batch.

What it reads is a snapshot: the nodes are read again when a directory is opened, or
after an operation made through the explorer. A change made outside of it — by another
application, or by the Windows explorer — shows up the next time that directory is
opened.

## FileSystemWatcherSource

The same source, watching the disk: a file or a directory created, deleted, renamed or
moved outside of the explorer shows up in it without a refresh.

```csharp
public IExplorerSource Source { get; } = new FileSystemWatcherSource(@"C:\Projects", toasts);
// ...
await Source.Open();
```

The opened directory follows what happens to it: renamed, it is opened again under its
new path; deleted, the directory that held it is opened instead, the way the Windows
explorer does.

{: .note }
> It is `IDisposable`: dispose it to stop watching. Only the loaded nodes are
> maintained, an event about a directory that has never been opened being dropped — it
> will be read from the disk when it is opened. The content of a file isn't watched, a
> node only carrying the size and the date it has been read with.

## Virtual nodes

A source hands over the nodes it wants: this one adds a `notes.md` of its own in every
directory, associated to a path no file of the disk stands behind. A node type of the
application is templated like the others — its visual and its context menu are keyed on
its type, and an extra column shows the state it carries.

```csharp
// A node type of the application : it carries a path, and a state no file of the disk has.
// An IExplorerNode and not an IExplorerFile : it has no size, so the size column stays empty on its row.
public class VirtualFile : ObservableObject, IExplorerNode
{
    public string Path { get; set; }
    public string Name { get; }
    public DateTime ModifiedAt { get; }
    public IExplorerDirectory? Parent { get; set; }

    public bool IsPinned { get => isPinned; set => SetProperty(ref isPinned, value); }
    private bool isPinned;

    public VirtualFile(IExplorerDirectory parent, string name)
    {
        Parent = parent;
        Name = name;
        Path = System.IO.Path.Combine(parent.Path, name);
        ModifiedAt = parent.ModifiedAt;
    }
}

// A source hands its own nodes over along with the ones it reads.
public class VirtualFilesSource : FileSystemSource
{
    protected override void LoadDirectory(IExplorerDirectory directory, int depth)
    {
        base.LoadDirectory(directory, depth);
        directory.Children.Add(GetVirtualFile(directory));
    }

    // Nothing to hand over to the shell : a virtual file is opened by the application itself.
    public override Task Open(IExplorerNode node) { ... }
}
```

The context menu of a node type is a `DataTemplate` keyed with
`ContextMenuTemplateKey`. Its data context is an `ExplorerMenuContext`: `Source` for
the commands, `Node` for the node the menu was opened on, `Nodes` for the whole
selection.

```xml
<fileExplorer:Explorer Source="{Binding VirtualSource}">
    <fileExplorer:Explorer.Resources>
        <conv:TypeConverter x:Key="NodeTypeConverter" />

        <!-- Visual of the virtual nodes, implicit as the ones of the library are -->
        <DataTemplate DataType="{x:Type local:VirtualFile}">
            <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="4">
                <fonts:FontIcon Text="{x:Static fonts:LucideFontIcons.StickyNote}" />
                <TextBlock Text="{Binding Name}" />
            </StackPanel>
        </DataTemplate>

        <!-- Context menu keyed on the node type, replaces the one of the files nowhere else -->
        <DataTemplate x:Key="{base:ContextMenuTemplateKey local:VirtualFile}">
            <ContextMenu>
                <MenuItem Header="Pinned" IsCheckable="True"
                          IsChecked="{Binding Node.IsPinned, Mode=TwoWay}" />
                <MenuItem Header="Copy path" Command="{Binding Source.CopyPathCommand}"
                          CommandParameter="{Binding Node}" />
            </ContextMenu>
        </DataTemplate>
    </fileExplorer:Explorer.Resources>
    <fileExplorer:Explorer.ExtraColumns>
        <GridViewColumn Header="Pinned" Width="70">
            <GridViewColumn.CellTemplate>
                <DataTemplate>
                    <!-- The column is shared by every row : only the virtual nodes show a box -->
                    <CheckBox IsChecked="{Binding IsPinned, Mode=OneWay}" IsHitTestVisible="False">
                        <CheckBox.Style>
                            <Style TargetType="CheckBox" BasedOn="{StaticResource {x:Type CheckBox}}">
                                <Setter Property="Visibility" Value="Collapsed" />
                                <Style.Triggers>
                                    <DataTrigger Value="{x:Type local:VirtualFile}"
                                                 Binding="{Binding Converter={StaticResource NodeTypeConverter}}">
                                        <Setter Property="Visibility" Value="Visible" />
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </CheckBox.Style>
                    </CheckBox>
                </DataTemplate>
            </GridViewColumn.CellTemplate>
        </GridViewColumn>
    </fileExplorer:Explorer.ExtraColumns>
</fileExplorer:Explorer>
```

{: .note }
> A context menu template is searched from the most specific type of the node up to
> `object`, and on the scope of the menu (`Single`, `Multiple`, `None` outside of any
> node), so a type without a menu of its own falls back on the default ones.

## A source of something else

A source that reads none of the disk implements `IExplorerSource` itself: `Root` and
`Current`, `Open()` and `Open(node)`, `Transfer(paths, target, isMove)` for what is
dropped or pasted into it, and the commands the menus and the shortcuts bind
(`RenameCommand`, `RemoveCommand`, `CreateDirectoryCommand`, `CopyCommand`,
`CutCommand`, `PasteCommand`, `OpenCommand`, `OpenInExplorerCommand`,
`CopyPathCommand`, `OpenWithDefaultCommand`).

The nodes it hands over are `IExplorerNode`s: a `Name`, a `Path`, a `ModifiedAt` and
the `Parent` directory, `IExplorerDirectory` adding its `Children` and `IExplorerFile`
a `Size`.
