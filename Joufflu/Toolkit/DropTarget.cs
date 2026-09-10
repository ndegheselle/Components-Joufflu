using System.Windows;
using System.Windows.Input;

namespace Joufflu.Toolkit;

/// <summary>
/// Turns any element into a drop target from a single attached property : set
/// <see cref="CommandProperty"/> to the command handling the drop.
/// <para>
/// <see cref="UIElement.AllowDrop"/> is set for you and the <c>DragEnter</c> / <c>DragOver</c> /
/// <c>DragLeave</c> / <c>Drop</c> events are all handled : the dragged data is passed to
/// <see cref="ICommand.CanExecute"/> to know whether it is accepted, the drop effect (see
/// <see cref="EffectProperty"/>) is reported to the source so the cursor shows whether the drop is
/// allowed, and the command is executed with that same data once it lands. <c>CanExecute</c> is
/// called on every mouse move of the drag, so keep it cheap and side effect free.
/// </para>
/// </summary>
public static class DropTarget
{
    /// <summary>
    /// Command handling the drop, with a <see cref="DropData"/> as parameter : the dragged data,
    /// which it is an <see cref="IDataObject"/> of, and where the pointer is on the element. Its
    /// <see cref="ICommand.CanExecute"/> is what decides which data the element accepts : data it
    /// refuses can't be dropped, and doesn't light <see cref="IsDragOverProperty"/> up.
    /// </summary>
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(DropTarget),
            new PropertyMetadata(null, OnCommandChanged));

    public static ICommand? GetCommand(DependencyObject obj) => (ICommand?)obj.GetValue(CommandProperty);
    public static void SetCommand(DependencyObject obj, ICommand? value) => obj.SetValue(CommandProperty, value);

    /// <summary>
    /// Effect reported to the drag source for an accepted drop, which drives the mouse cursor
    /// (defaults to <see cref="DragDropEffects.Copy"/>). Data the source doesn't allow this effect
    /// for is rejected.
    /// </summary>
    public static readonly DependencyProperty EffectProperty =
        DependencyProperty.RegisterAttached(
            "Effect",
            typeof(DragDropEffects),
            typeof(DropTarget),
            new PropertyMetadata(DragDropEffects.Copy));

    public static DragDropEffects GetEffect(DependencyObject obj) => (DragDropEffects)obj.GetValue(EffectProperty);
    public static void SetEffect(DependencyObject obj, DragDropEffects value) => obj.SetValue(EffectProperty, value);

    /// <summary>
    /// <c>true</c> while accepted data hovers the element ; data that is rejected never sets it,
    /// so a trigger on it highlights valid drops only. Inherits, so template and content children
    /// see it too.
    /// </summary>
    private static readonly DependencyPropertyKey IsDragOverKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "IsDragOver",
            typeof(bool),
            typeof(DropTarget),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <inheritdoc cref="IsDragOverKey"/>
    public static readonly DependencyProperty IsDragOverProperty = IsDragOverKey.DependencyProperty;

    public static bool GetIsDragOver(DependencyObject obj) => (bool)obj.GetValue(IsDragOverProperty);
    private static void SetIsDragOver(DependencyObject obj, bool value) => obj.SetValue(IsDragOverKey, value);

    // Holds the per-element watcher so its event handlers stay removable.
    private static readonly DependencyProperty WatcherProperty =
        DependencyProperty.RegisterAttached(
            "Watcher",
            typeof(DropWatcher),
            typeof(DropTarget),
            new PropertyMetadata(null));

    // The command is what makes the element a drop target : without one there is nothing to drop
    // into, whatever Effect says.
    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        var watcher = (DropWatcher?)element.GetValue(WatcherProperty);
        bool isTarget = e.NewValue != null;

        if (isTarget == (watcher != null))
            return;

        if (isTarget)
        {
            element.SetValue(WatcherProperty, new DropWatcher(element));
            element.AllowDrop = true;
        }
        else
        {
            watcher!.Detach();
            element.SetValue(WatcherProperty, null);
            element.AllowDrop = false;
            SetIsDragOver(element, false);
        }
    }

    /// <summary>Holds the drag handlers of a single element, and its drag depth.</summary>
    private sealed class DropWatcher
    {
        private readonly UIElement _element;

        // DragEnter and DragLeave bubble up from the children too, so entering a child raises a
        // leave for the one before it : the leaves are counted against the enters to only drop the
        // highlight once the drag left the element itself.
        private int _enterCount;

        public DropWatcher(UIElement element)
        {
            _element = element;
            _element.DragEnter += OnDragEnter;
            _element.DragOver += OnDragOver;
            _element.DragLeave += OnDragLeave;
            _element.Drop += OnDrop;
        }

        public void Detach()
        {
            _element.DragEnter -= OnDragEnter;
            _element.DragOver -= OnDragOver;
            _element.DragLeave -= OnDragLeave;
            _element.Drop -= OnDrop;
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            _enterCount++;
            OnDragOver(sender, e);
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            // Only the effects the source allows can be reported back : asking for a copy of data
            // that may only be moved would show a cursor the drop can't honor.
            DragDropEffects effect = GetEffect(_element) & e.AllowedEffects;
            bool isAccepted = effect != DragDropEffects.None && Accepts(GetDropData(e));

            SetIsDragOver(_element, isAccepted);
            e.Effects = isAccepted ? effect : DragDropEffects.None;
            // Handled either way : the element answered for the data, and an ancestor drop target
            // overwriting the effects would show a cursor that doesn't match what happens here.
            e.Handled = true;
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            _enterCount = Math.Max(0, _enterCount - 1);
            if (_enterCount == 0)
                SetIsDragOver(_element, false);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            // A drop ends the drag wherever it came from, no leave is raised for it.
            _enterCount = 0;
            SetIsDragOver(_element, false);

            DropData data = GetDropData(e);
            if (!Accepts(data))
                return;

            GetCommand(_element)?.Execute(data);
            e.Handled = true;
        }

        /// <summary>What the command is given : the dragged data, and where the drag is on the element.</summary>
        private DropData GetDropData(DragEventArgs e) => new(e.Data, _element, e.GetPosition(_element));

        private bool Accepts(DropData data) => GetCommand(_element)?.CanExecute(data) ?? false;
    }
}
