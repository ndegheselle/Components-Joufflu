using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Joufflu.FileExplorer.Data;
using Joufflu.FileExplorer.Sources;
using Joufflu.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Joufflu.FileExplorer.Controls.Base;

/// <summary>
/// Kinds of node a control displays, combinable so a control can show files, directories or both.
/// </summary>
[Flags]
public enum ExplorerNodeKinds
{
    None = 0,
    Files = 1,
    Directories = 2,
    All = Files | Directories
}

public static class ExplorerNodeKindsExtensions
{
    /// <summary>Whether <paramref name="node"/> is one of the kinds in <paramref name="kinds"/>.</summary>
    public static bool Includes(this ExplorerNodeKinds kinds, IExplorerNode node)
        => node is IExplorerDirectory
            ? kinds.HasFlag(ExplorerNodeKinds.Directories)
            : kinds.HasFlag(ExplorerNodeKinds.Files);
}

/// <summary>
/// Behaviour shared by the controls displaying the nodes of a <see cref="ExplorerControl.Source"/>
/// (<see cref="ExplorerList"/>, <see cref="ExplorerTree"/>) : the kinds of node shown, the edition of a name, the
/// context menu of a node and the drag and drop of files. A derived control only provides the
/// <see cref="ItemsControl"/> template part displaying the nodes, and tells how to read its selection and its item
/// containers.
/// </summary>
[ObservableObject]
[TemplatePart(Name = PartItemsHost, Type = typeof(ItemsControl))]
public abstract partial class ExplorerNodesControl : ExplorerControl, IExplorerUi
{
    #region Dependency Properties

    private static readonly DependencyPropertyKey SelectedNodesPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(SelectedNodes),
        typeof(IReadOnlyList<IExplorerNode>),
        typeof(ExplorerNodesControl),
        new PropertyMetadata(Array.Empty<IExplorerNode>()));
    public static readonly DependencyProperty SelectedNodesProperty = SelectedNodesPropertyKey.DependencyProperty;

    public static readonly DependencyProperty VisibleNodesProperty = DependencyProperty.Register(
        nameof(VisibleNodes),
        typeof(ExplorerNodeKinds),
        typeof(ExplorerNodesControl),
        new FrameworkPropertyMetadata(ExplorerNodeKinds.All, OnVisibleNodesChanged));

    public static readonly DependencyPropertyKey IsDragOverKey = DependencyProperty.RegisterReadOnly(
        nameof(IsDragOver),
        typeof(bool),
        typeof(ExplorerNodesControl),
        new FrameworkPropertyMetadata(false));
    public static readonly DependencyProperty IsDragOverProperty = IsDragOverKey.DependencyProperty;

    #endregion

    /// <summary>
    /// Nodes selected in <see cref="ItemsHost"/>, empty when nothing is selected. Bindable, so that a status bar can
    /// show how many of them there are.
    /// </summary>
    public IReadOnlyList<IExplorerNode> SelectedNodes
        => (IReadOnlyList<IExplorerNode>)GetValue(SelectedNodesProperty);

    /// <summary>
    /// Kinds of node the control shows, <see cref="ExplorerNodeKinds.All"/> by default. Set it to
    /// <see cref="ExplorerNodeKinds.Directories"/> or <see cref="ExplorerNodeKinds.Files"/> to display only one.
    /// </summary>
    public ExplorerNodeKinds VisibleNodes
    {
        get => (ExplorerNodeKinds)GetValue(VisibleNodesProperty);
        set => SetValue(VisibleNodesProperty, value);
    }

    public bool IsDragOver
    {
        get => (bool)GetValue(IsDragOverProperty);
        private set => SetValue(IsDragOverKey, value);
    }

    protected const string PartItemsHost = "PART_ItemsHost";

    /// <summary>
    /// Control displaying the nodes, taken from the <see cref="PartItemsHost"/> template part.
    /// </summary>
    protected ItemsControl? ItemsHost { get; private set; }

    /// <summary>
    /// Node whose name is being edited in the control, null while none is. Held by the control and not by its
    /// <see cref="ExplorerControl.Source"/> : the edition belongs to the control it was started in, so another control displaying the
    /// same node doesn't open a box of its own.
    /// </summary>
    [ObservableProperty]
    private IExplorerNode? renamedNode;

    ICommand IExplorerUi.RenamingCommand => RenamingCommand;

    protected ExplorerNodesControl()
    {
        // Default context menu to fix the first right click
        this.ContextMenu = new ContextMenu();
        ContextMenuOpening += ExplorerNodesControl_ContextMenuOpening;
        MouseDoubleClick += ExplorerNodesControl_MouseDoubleClick;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (ItemsHost != null)
        {
            ItemsHost.Drop -= ItemsHost_Drop;
            ItemsHost.MouseMove -= ItemsHost_MouseMove;
            ItemsHost.PreviewMouseLeftButtonDown -= ItemsHost_PreviewMouseLeftButtonDown;
            ItemsHost.PreviewMouseLeftButtonUp -= ItemsHost_PreviewMouseLeftButtonUp;
            ItemsHost.QueryContinueDrag -= ItemsHost_QueryContinueDrag;

            ItemsHost.DragEnter -= ItemsHost_DragEnter;
            ItemsHost.DragOver -= ItemsHost_DragOver;
            ItemsHost.DragLeave -= ItemsHost_DragLeave;
        }

        ItemsHost = GetTemplateChild(PartItemsHost) as ItemsControl;

        if (ItemsHost != null)
        {
            ItemsHost.Drop += ItemsHost_Drop;
            ItemsHost.MouseMove += ItemsHost_MouseMove;
            ItemsHost.PreviewMouseLeftButtonDown += ItemsHost_PreviewMouseLeftButtonDown;
            ItemsHost.PreviewMouseLeftButtonUp += ItemsHost_PreviewMouseLeftButtonUp;
            ItemsHost.QueryContinueDrag += ItemsHost_QueryContinueDrag;

            ItemsHost.DragEnter += ItemsHost_DragEnter;
            ItemsHost.DragOver += ItemsHost_DragOver;
            ItemsHost.DragLeave += ItemsHost_DragLeave;
        }

        UpdateSelectedNodes();
    }

    #region Derived control

    /// <summary>
    /// Reads the selection of <see cref="ItemsHost"/>, which only a derived control knows how to reach.
    /// </summary>
    protected abstract IReadOnlyList<IExplorerNode> GetSelectedNodes();

    /// <summary>
    /// Publishes the selection of <see cref="ItemsHost"/> in <see cref="SelectedNodes"/>. A derived control calls it
    /// whenever its host reports a selection change.
    /// </summary>
    protected void UpdateSelectedNodes() => SetValue(SelectedNodesPropertyKey, GetSelectedNodes());

    /// <summary>
    /// Item container displaying a node, from an element inside of it, null when <paramref name="source"/> is outside
    /// of any of them.
    /// </summary>
    protected abstract FrameworkElement? GetContainerAt(DependencyObject? source);

    /// <summary>
    /// Whether an element opens no context menu at all, a column header of a list for instance.
    /// </summary>
    protected virtual bool IsMenuIgnored(DependencyObject? source) => false;

    /// <summary>
    /// Reacts to a double click on a node, opening it by default. Returns whether the double click has been handled.
    /// </summary>
    /// <param name="node">Node the double click happened on.</param>
    /// <param name="container">Item container displaying <paramref name="node"/>.</param>
    protected virtual bool OnNodeDoubleClick(IExplorerNode node, FrameworkElement container)
    {
        Source.Open(node);
        return true;
    }

    #endregion

    #region On dependency property changed

    /// <summary>
    /// Tracks the directory opened by the source, the nodes displayed by the control coming from it.
    /// </summary>
    protected override void OnSourceChanged(IExplorerSource? previous, IExplorerSource? source)
    {
        void OnSourcePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IExplorerSource.Current))
                return;

            // Navigating away gives up the edition in progress, its node not being displayed anymore.
            RenamedNode = null;
            OnCurrentChanged();
        }

        // The nodes of the previous source are gone, so is any edition of one of them.
        RenamedNode = null;

        // Update the displayed nodes and track then source change
        if (previous != null)
            previous.PropertyChanged -= OnSourcePropertyChanged;
        if (source != null)
        {
            source.PropertyChanged += OnSourcePropertyChanged;
            OnCurrentChanged();
        }
    }

    /// <summary>
    /// The <see cref="ExplorerControl.Source"/>, or the directory it has opened, changed ; a derived control overrides it to rebuild
    /// what it displays.
    /// </summary>
    protected virtual void OnCurrentChanged() { }

    private static void OnVisibleNodesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((ExplorerNodesControl)d).OnVisibleNodesChanged();

    /// <summary>Re-applies <see cref="VisibleNodes"/> when it changes ; a derived control overrides it as needed.</summary>
    protected virtual void OnVisibleNodesChanged() { }

    /// <summary>Keeps only the nodes whose kind is in <see cref="VisibleNodes"/>.</summary>
    protected bool FilterNode(object item) => item is IExplorerNode node && VisibleNodes.Includes(node);

    #endregion

    #region Rename

    /// <summary>
    /// Starts the edition of the name of a node, null giving up the one in progress : the control displays an editable
    /// name in place of that node until <see cref="Rename"/> ends it.
    /// </summary>
    [RelayCommand]
    private void Renaming(IExplorerNode? node) => RenamedNode = node;

    /// <summary>
    /// Ends the edition, <paramref name="rename"/> being null when it has been given up : the control closes its
    /// editable name in either case, and only hands a validated one over to the <see cref="ExplorerControl.Source"/>.
    /// </summary>
    [RelayCommand]
    private void Rename(ExplorerNodeRename? rename)
    {
        RenamedNode = null;
        if (rename == null)
            return;

        Source?.RenameCommand.Execute(rename);
    }

    #endregion

    #region Keyboard shortcuts

    /// <summary>
    /// Runs the shortcuts of the node operations, the ones the context menu displays as its input gestures : F2
    /// renames, Ctrl+C copies, Ctrl+X cuts, Ctrl+V pastes and Delete removes.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // An already handled key has been acted on by the host (the navigation keys of a list for instance), and the
        // keys typed in a name being edited belong to that edition.
        if (e.Handled || IsInRenameBox(e.OriginalSource))
            return;

        // Shortcuts
        IReadOnlyList<IExplorerNode> nodes = GetSelectedNodes();

        if (nodes.Count > 0)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C){
                Source.CopyCommand.Execute(nodes); 
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X){
                Source.CutCommand.Execute(nodes); 
                e.Handled = true;
            }
            else if(e.Key == Key.Delete){
                Source.RemoveCommand.Execute(nodes); 
                e.Handled = true;
            }
            else if(e.Key == Key.F2){
                Renaming(nodes.First()); 
                e.Handled = true;
            }

        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V){
            Source.PasteCommand.Execute(Source.Current); 
            e.Handled = true;
        }
    }
    #endregion

    #region UI events
    private void ExplorerNodesControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // A double click inside the name being renamed selects a word, it doesn't open the node.
        if (IsInRenameBox(e.OriginalSource))
            return;

        var container = GetContainerAt(e.OriginalSource as DependencyObject);
        if (container?.DataContext is not IExplorerNode node)
            return;

        e.Handled = OnNodeDoubleClick(node, container);
    }

    private static bool IsInRenameBox(object source)
        => MoreVisualTreeHelper.FindSelfOrParent(source as DependencyObject, typeof(TextBox)) != null;

    private void ExplorerNodesControl_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (ItemsHost == null || IsMenuIgnored(e.OriginalSource as DependencyObject))
        {
            e.Handled = true;
            return;
        }

        // The menu acts on the node it is opened on and not on the selection : a right click doesn't select, so the
        // node under the pointer is rarely the selected one (a tree keeps its single root selected for instance).
        IExplorerNode? target = GetContainerAt(e.OriginalSource as DependencyObject)?.DataContext as IExplorerNode;
        IReadOnlyList<IExplorerNode> nodes;
        MenuScope scope;

        if (target != null)
        {
            nodes = GetMenuNodes(target);
            scope = nodes.Count > 1 ? MenuScope.Multiple : MenuScope.Single;
        }
        else
        {
            // Outside of any node : the menu of the opened folder itself.
            target = Source.Current;
            nodes = target == null ? [] : [target];
            scope = MenuScope.None;
        }

        if (target == null)
        {
            e.Handled = true;
            return;
        }

        var template = FindContextMenuTemplate(target.GetType(), scope);

        if (template?.LoadContent() is not ContextMenu menu)
        {
            e.Handled = true;
            return;
        }

        var element = (FrameworkElement)sender;
        menu.DataContext = new ExplorerMenuContext(Source, this, nodes);
        element.ContextMenu = menu;
    }
    #endregion

    #region Context menu
    /// <summary>
    /// Selected nodes with the one the menu was opened on first, or only that node when it isn't selected : a menu
    /// opened on a node outside of the selection acts on that node alone, as the Windows explorer does.
    /// </summary>
    private IReadOnlyList<IExplorerNode> GetMenuNodes(IExplorerNode node)
    {
        var nodes = GetSelectedNodes().ToList();
        if (!nodes.Remove(node))
            return [node];

        nodes.Insert(0, node);
        return nodes;
    }

    /// <summary>
    /// Searches the context menu template of a node type, from the most specific type to <see cref="object"/>.
    /// </summary>
    private DataTemplate? FindContextMenuTemplate(Type nodeType, MenuScope scope)
    {
        foreach (var type in GetTypeCandidates(nodeType))
        {
            if (TryFindResource(new ContextMenuTemplateKey(type) { Scope = scope }) is DataTemplate template)
                return template;
        }

        return null;
    }

    private static IEnumerable<Type> GetTypeCandidates(Type nodeType)
    {
        for (Type? type = nodeType; type != null && type != typeof(object); type = type.BaseType)
            yield return type;

        foreach (var interfaceType in nodeType.GetInterfaces())
            yield return interfaceType;

        yield return typeof(object);
    }
    #endregion

    #region Drag and Drop
    private void ItemsHost_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _clickPosition = e.GetPosition(null);
    }
    private void ItemsHost_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isCanceled = false;
    }

    private void ItemsHost_Drop(object sender, DragEventArgs e)
    {
        if (!TryGetDrop(e, out IExplorerDirectory? target, out IReadOnlyList<string> files))
            return;

        Source.Transfer(files, target, isMove: false);
    }

    /// <summary>
    /// Directory a drop would land in : the one under the pointer, or the opened one anywhere else.
    /// </summary>
    private IExplorerDirectory? GetDropTarget(DragEventArgs e)
        => (e.OriginalSource as FrameworkElement)?.DataContext as IExplorerDirectory ?? Source.Current;

    /// <summary>
    /// Target and dropped paths of a drag, false when it has nothing to transfer : the paths already in the target
    /// directory are left out, so that dragging a node around the folder it is displayed in doesn't copy it next to
    /// itself.
    /// </summary>
    private bool TryGetDrop(DragEventArgs e, [NotNullWhen(true)] out IExplorerDirectory? target, out IReadOnlyList<string> files)
    {
        target = null;
        files = [];

        if (e.Data.GetDataPresent(DataFormats.FileDrop) == false)
            return false;

        IExplorerDirectory? directory = GetDropTarget(e);
        if (directory == null)
            return false;

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths)
            return false;

        target = directory;
        files = [.. paths.Where(path => !IsInDirectory(path, directory.Path))];
        return files.Count > 0;
    }

    /// <summary>
    /// Whether a path is a node of a directory, the file system of Windows being case insensitive.
    /// </summary>
    private static bool IsInDirectory(string path, string directoryPath)
    {
        try
        {
            return string.Equals(
                Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(Path.GetFullPath(path))),
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(directoryPath)),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            // An invalid path is left to the transfer, which ignores what doesn't exist.
            return false;
        }
    }

    private void ItemsHost_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isCanceled || e.LeftButton != MouseButtonState.Pressed)
            return;

        if (ItemsHost == null) return;
        // Nothing to drag from outside of a node.
        if (GetContainerAt(e.OriginalSource as DependencyObject) == null) return;

        Point position = e.GetPosition(null);
        if (!HasExceededMinimumDistance(position)) return;

        IReadOnlyList<IExplorerNode> nodes = GetSelectedNodes();
        if (nodes.Count == 0) return;

        DataObject data = new DataObject(DataFormats.FileDrop, nodes.Select(x => x.Path).ToArray());
        DragDrop.DoDragDrop(ItemsHost, data, DragDropEffects.Copy | DragDropEffects.Move);
    }

    private void ItemsHost_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
    {
        if (e.EscapePressed)
        {
            _isCanceled = true;
            e.Action = DragAction.Cancel;
        }
    }

    private int _enterCount;
    private void ItemsHost_DragEnter(object sender, DragEventArgs e)
    {
        _enterCount++;
        ItemsHost_DragOver(sender, e);
    }
    private void ItemsHost_DragOver(object sender, DragEventArgs e)
    {
        if (!TryGetDrop(e, out _, out _))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            IsDragOver = false;
            return;
        }

        IsDragOver = true;
    }
    private void ItemsHost_DragLeave(object sender, DragEventArgs e)
    {
        _enterCount = Math.Max(0, _enterCount - 1);
        if (_enterCount == 0)
            IsDragOver = false;
    }

    /// <summary>
    /// Prevent another drag to start after on the same click after one have been canceled.
    /// </summary>
    private bool _isCanceled = false;
    private Point _clickPosition;
    private bool HasExceededMinimumDistance(Point position)
    {
        return Math.Abs(position.X - _clickPosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(position.Y - _clickPosition.Y) >= SystemParameters.MinimumVerticalDragDistance;
    }
    #endregion
}