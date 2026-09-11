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

A source hands over the nodes it wants, of the disk or of the application, and a node
type of your own is templated like the others — see
[virtual nodes](sources.md#virtual-nodes).
