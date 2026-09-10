using System.Windows;
using System.Windows.Input;

namespace Joufflu.Toolkit;

/// <summary>
/// Turns any element into a drag source from a single attached property : set
/// <see cref="DataProperty"/> to what the drag carries.
/// <para>
/// The mouse events are handled for you : pressing the left button on the element arms the drag,
/// which only starts once the pointer moved past the system drag threshold, so clicks keep working.
/// The data is wrapped in a <see cref="DataObject"/> unless it already is an
/// <see cref="IDataObject"/>, and offered to the drop targets with the effects of
/// <see cref="AllowedEffectsProperty"/>. <see cref="IsDraggingProperty"/> is set for the whole
/// duration of the drag, which is a blocking call : nothing else happens on the element until the
/// drop lands or the drag is cancelled.
/// </para>
/// <para>
/// A wrapped data is offered under every type it is : its exact type, but also the base classes and
/// the interfaces of it, so a target can ask for the type it handles without having to know the
/// exact kind it is given.
/// </para>
/// </summary>
public static class DragSource
{
    /// <summary>
    /// Data the drag carries, given to the drop targets as is when it is an
    /// <see cref="IDataObject"/>, wrapped in a <see cref="DataObject"/> otherwise. A
    /// <c>null</c> value means nothing to drag, and the element behaves as if it had never been a
    /// drag source.
    /// </summary>
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.RegisterAttached(
            "Data",
            typeof(object),
            typeof(DragSource),
            new PropertyMetadata(null, OnDataChanged));

    public static object? GetData(DependencyObject obj) => obj.GetValue(DataProperty);
    public static void SetData(DependencyObject obj, object? value) => obj.SetValue(DataProperty, value);

    /// <summary>
    /// Effects the source allows the targets to answer with (defaults to
    /// <see cref="DragDropEffects.Copy"/>) : a target asking for anything else is refused, and its
    /// drop can't happen.
    /// </summary>
    public static readonly DependencyProperty AllowedEffectsProperty =
        DependencyProperty.RegisterAttached(
            "AllowedEffects",
            typeof(DragDropEffects),
            typeof(DragSource),
            new PropertyMetadata(DragDropEffects.Copy));

    public static DragDropEffects GetAllowedEffects(DependencyObject obj) => (DragDropEffects)obj.GetValue(AllowedEffectsProperty);
    public static void SetAllowedEffects(DependencyObject obj, DragDropEffects value) => obj.SetValue(AllowedEffectsProperty, value);

    /// <summary>
    /// <c>true</c> while the element is being dragged, so a trigger on it can fade the original out
    /// while it travels. Inherits, so template and content children see it too.
    /// </summary>
    private static readonly DependencyPropertyKey IsDraggingKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "IsDragging",
            typeof(bool),
            typeof(DragSource),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    /// <inheritdoc cref="IsDraggingKey"/>
    public static readonly DependencyProperty IsDraggingProperty = IsDraggingKey.DependencyProperty;

    public static bool GetIsDragging(DependencyObject obj) => (bool)obj.GetValue(IsDraggingProperty);
    private static void SetIsDragging(DependencyObject obj, bool value) => obj.SetValue(IsDraggingKey, value);

    // Holds the per-element watcher so its event handlers stay removable.
    private static readonly DependencyProperty WatcherProperty =
        DependencyProperty.RegisterAttached(
            "Watcher",
            typeof(DragWatcher),
            typeof(DragSource),
            new PropertyMetadata(null));

    // The data is what makes the element a drag source : without it there is nothing to drag,
    // whatever AllowedEffects says.
    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        var watcher = (DragWatcher?)element.GetValue(WatcherProperty);
        bool isSource = e.NewValue != null;

        if (isSource == (watcher != null))
            return;

        if (isSource)
        {
            element.SetValue(WatcherProperty, new DragWatcher(element));
        }
        else
        {
            watcher!.Detach();
            element.SetValue(WatcherProperty, null);
            SetIsDragging(element, false);
        }
    }

    /// <summary>
    /// The data as the drop targets get it : given as is when it already is an
    /// <see cref="IDataObject"/>, wrapped otherwise.
    /// <para>
    /// A <see cref="DataObject"/> only holds what it is given under its exact type, which a target
    /// would have to know to ask for it : it is also stored under the base classes and the
    /// interfaces of that type, so asking for the type the target handles is enough.
    /// </para>
    /// </summary>
    private static IDataObject Wrap(object data)
    {
        if (data is IDataObject dataObject)
            return dataObject;

        // Holds the exact type, plus the conversions WPF knows of it (a string as text...).
        var wrapper = new DataObject(data);

        for (Type? type = data.GetType().BaseType; type != null && type != typeof(object); type = type.BaseType)
            wrapper.SetData(type, data);
        foreach (Type contract in data.GetType().GetInterfaces())
            wrapper.SetData(contract, data);

        return wrapper;
    }

    /// <summary>Holds the mouse handlers of a single element, and its armed drag origin.</summary>
    private sealed class DragWatcher
    {
        private readonly UIElement _element;

        // Where the button went down, as long as the drag is armed but didn't pass the threshold
        // yet ; null the rest of the time.
        private Point? _origin;

        public DragWatcher(UIElement element)
        {
            _element = element;
            // Preview, so a drag can start from elements handling the press themselves (buttons,
            // list items, text boxes...) : arming is harmless, and the press is left untouched.
            _element.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            _element.PreviewMouseMove += OnMouseMove;
            _element.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        public void Detach()
        {
            _element.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
            _element.PreviewMouseMove -= OnMouseMove;
            _element.PreviewMouseLeftButtonUp -= OnMouseLeftButtonUp;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _origin = e.GetPosition(_element);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_origin is not Point origin || e.LeftButton != MouseButtonState.Pressed)
            {
                // The button was released outside of the element, or a capture stole the up event :
                // nothing is armed anymore.
                _origin = null;
                return;
            }

            // Below the threshold the user is still clicking, not dragging.
            Vector moved = e.GetPosition(_element) - origin;
            if (Math.Abs(moved.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(moved.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            _origin = null;

            object? data = GetData(_element);
            if (data == null)
                return;

            SetIsDragging(_element, true);
            try
            {
                // Blocks until the drop lands or the drag is cancelled ; the mouse events of the
                // element are the system's until then.
                DragDrop.DoDragDrop(_element, Wrap(data), GetAllowedEffects(_element));
            }
            finally
            {
                SetIsDragging(_element, false);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _origin = null;
        }
    }
}
