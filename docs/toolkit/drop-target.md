---
title: Drag and drop
parent: Toolkit
nav_order: 4
---

# Drag and drop

## DropTarget.Command

`DropTarget.Command` makes **any** element a drop target: `AllowDrop` and the four
drag events (`DragEnter`, `DragOver`, `DragLeave`, `Drop`) are handled for you, and
the command is executed with the dropped `IDataObject` as parameter.

The command's `CanExecute` is what decides the accepted data: what it refuses can't
be dropped — the cursor shows a no-drop sign — and never lights the zone up.

```xml
<!-- AllowDrop and the drag events are handled by the behavior -->
<Border toolkit:DropTarget.Command="{Binding DropFilesCommand}"
        BorderThickness="{DynamicResource {x:Static joufflu:Dimensions.BorderThickness}}">
    <TextBlock VerticalAlignment="Center" Text="Drop .pdf files here" />
</Border>
```

```csharp
// The command's CanExecute is the whole filter: what it refuses can't be dropped
public IRelayCommand DropFilesCommand { get; }
    = new RelayCommand<IDataObject>(DropFiles, CanDropFiles);

private static bool CanDropFiles(IDataObject? data)
{
    string[]? files = data?.GetData(DataFormats.FileDrop) as string[];
    return files?.Length > 0 && files.All(f => Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase));
}

// Same IDataObject, once the drop landed
private static void DropFiles(IDataObject? data) { ... }
```

{: .note }
> `CanExecute` is called on every mouse move of the drag, so keep it cheap and side
> effect free — look at the paths, not at the files.

## Where the drop landed

The parameter is a `DropData`: the dragged data — which it is an `IDataObject` of, so a
command only interested in what was dropped keeps taking one and reads it as above — and
where the pointer is on the target, for the drops that land *somewhere* rather than just on
something: a canvas placing what it receives under the cursor, a list inserting it at an
index.

```csharp
// Position is relative to Target, the element holding DropTarget.Command
private void DropNode(IDataObject? data)
{
    if (data is not DropData drop)
        return;

    Point onCanvas = drop.Target.TranslatePoint(drop.Position, Canvas);
    ...
}
```

{: .note }
> `Position` is where the drop landed for the command itself, and where the pointer is for
> `CanExecute`, which is called all along the drag: a target can accept a drag over one of
> its areas and refuse it over another.

## DropTarget.IsDragOver

`IsDragOver` is `true` while **accepted** data hovers the element, which is all a
trigger needs to highlight a valid drop. It inherits, so template and content
children see it too.

```xml
<Border toolkit:DropTarget.Command="{Binding DropFilesCommand}">
    <!-- Background and BorderBrush are styled, not set on the Border, so the trigger can override them -->
    <Border.Style>
        <Style TargetType="Border">
            <Setter Property="Background" Value="Transparent" />
            <Style.Triggers>
                <!-- True only while accepted data hovers: refused files never highlight -->
                <Trigger Property="toolkit:DropTarget.IsDragOver" Value="True">
                    <Setter Property="Background" Value="{DynamicResource {x:Static joufflu:Brushes.Primary100Brush}}" />
                    <Setter Property="BorderBrush" Value="{DynamicResource {x:Static joufflu:Brushes.PrimaryBrush}}" />
                </Trigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
    <TextBlock VerticalAlignment="Center" Text="Drop .pdf files here" />
</Border>
```

## DropTarget.Effect

`Effect` is the drop effect reported to the drag source for an accepted drop, which
drives the mouse cursor — `Copy` by default. Data the source doesn't allow this
effect for is rejected, so a `Move` target only accepts drags that can be moved.

```xml
<Border toolkit:DropTarget.Command="{Binding MoveCommand}"
        toolkit:DropTarget.Effect="Move" />
```

## DragSource.Data

`DragSource.Data` is the counterpart of `DropTarget.Command`: it makes **any** element
a drag source, carrying that data to the drop targets. The mouse events are handled
for you, and the drag only starts once the pointer moved past the system drag
threshold, so clicks keep working — a draggable button is still clickable.

The data is given to the targets as is when it already is an `IDataObject`, and
wrapped in a `DataObject` otherwise: a `string` arrives as text, a view model under
its own type — and under every base class and interface of it, so a target can ask for
what the data **is** instead of having to know the exact kind it is given.

```xml
<!-- The mouse events are handled by the behavior: the drag starts past the system threshold -->
<Border toolkit:DragSource.Data="{Binding}"
        toolkit:DragSource.AllowedEffects="Move">
    <TextBlock Text="{Binding}" />
</Border>
```

```csharp
// The target reads what the source carried, wrapped in a DataObject
private static string? GetTag(IDataObject? data) => data?.GetData(DataFormats.UnicodeText) as string;

// Whatever kind of node was dragged, the target only asks for the base it can handle
private static BaseNode? GetNode(IDataObject? data) => data?.GetData(typeof(BaseNode)) as BaseNode;
```

## DragSource.AllowedEffects

`AllowedEffects` is what the source lets the targets answer with — `Copy` by default.
A target asking for anything else is refused, so `AllowedEffects` and
`DropTarget.Effect` must agree for the drop to happen.

```xml
<!-- Both agree on Move: the item leaves the source -->
<Border toolkit:DragSource.Data="{Binding}" toolkit:DragSource.AllowedEffects="Move" />
<Border toolkit:DropTarget.Command="{Binding TakeCommand}" toolkit:DropTarget.Effect="Move" />
```

## DragSource.IsDragging

`IsDragging` is `true` for the whole duration of the drag, which is all a trigger
needs to fade the original out while it travels. It inherits, so template and content
children see it too.

```xml
<Border toolkit:DragSource.Data="{Binding}">
    <Border.Style>
        <Style TargetType="Border">
            <Style.Triggers>
                <!-- True for the whole drag: the original fades out while it travels -->
                <Trigger Property="toolkit:DragSource.IsDragging" Value="True">
                    <Setter Property="Opacity" Value="0.4" />
                </Trigger>
            </Style.Triggers>
        </Style>
    </Border.Style>
    <TextBlock Text="{Binding}" />
</Border>
```

{: .note }
> The drag is a blocking call: nothing else happens on the element until the drop
> lands or the drag is cancelled.
