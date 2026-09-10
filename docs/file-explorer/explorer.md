---
title: Explorer
parent: File explorer
nav_order: 1
---

# Explorer

## Explorer

Everything together: the control bar with the breadcrumb of the opened path, the tree
of the loaded hierarchy next to the list of the opened folder, and a status bar
counting its items and the selected ones.

```xml
<fileExplorer:Explorer Source="{Binding Source}" />
```

The parts share the `Source` of the explorer, which is what keeps them in sync.
`ExtraColumns` is handed over to its list, see
[extra columns](list.md#extra-columns).

## Custom node template

The node visuals are implicit templates keyed by node type: redeclaring one in the
resources of a control replaces it there only. Here `ExplorerIconConverter` trades the
font icons for the icons the system associates with each file type.

```xml
<fileExplorer:Explorer Source="{Binding Source}">
    <fileExplorer:Explorer.Resources>
        <DataTemplate x:Key="NodeWithSystemIcon">
            <StackPanel Orientation="Horizontal" toolkit:Spacing.Gap="4">
                <Image Width="16" Height="16"
                       Source="{Binding Converter={x:Static converters:ExplorerIconConverter.Small}}" />
                <TextBlock Text="{Binding Name}" />
            </StackPanel>
        </DataTemplate>
        <!-- Replaces the implicit template of the node type, in this control only -->
        <DataTemplate DataType="{x:Type data:FileSystemFile}">
            <ContentPresenter Content="{Binding}" ContentTemplate="{StaticResource NodeWithSystemIcon}" />
        </DataTemplate>
        <DataTemplate DataType="{x:Type data:FileSystemDirectory}">
            <ContentPresenter Content="{Binding}" ContentTemplate="{StaticResource NodeWithSystemIcon}" />
        </DataTemplate>
    </fileExplorer:Explorer.Resources>
</fileExplorer:Explorer>
```

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
