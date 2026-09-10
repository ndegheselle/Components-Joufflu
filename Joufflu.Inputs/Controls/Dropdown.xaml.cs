using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Joufflu.Toolkit;

namespace Joufflu.Inputs.Controls
{
    /// <summary>
    /// Corner of the popup that is anchored to the matching corner of the toggle button.
    /// </summary>
    public enum DropdownPlacement
    {
        /// <summary>Popup top-left at the button's bottom-left (default, opens downward).</summary>
        BottomLeft,
        /// <summary>Popup top-right at the button's bottom-right (right-aligned, opens downward).</summary>
        BottomRight,
        /// <summary>Popup bottom-left at the button's top-left (opens upward).</summary>
        TopLeft,
        /// <summary>Popup bottom-right at the button's top-right (right-aligned, opens upward).</summary>
        TopRight
    }

    /// <summary>
    /// Chrome hosting the dropdown content inside the popup. Only meant to be created by
    /// <see cref="Dropdown"/>; its default style lives in <c>Dropdown.xaml</c>.
    /// </summary>
    public class DropdownPopupHost : ContentControl
    {
        static DropdownPopupHost()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DropdownPopupHost),
                new FrameworkPropertyMetadata(typeof(DropdownPopupHost)));
        }

        /// <summary>
        /// The toggle button the popup hangs off. The popup tree is rooted in its own <c>PopupRoot</c>,
        /// so a <see cref="System.Windows.Data.RelativeSource"/> ancestor lookup or an
        /// <c>ElementName</c> cannot reach out of it; bind through this instead. The host is a plain
        /// ancestor inside the popup, so the lookup below stays within one tree.
        /// <example>
        /// <code>
        /// &lt;Button Command="{Binding PlacementTarget.DataContext.SaveCommand,
        ///     RelativeSource={RelativeSource AncestorType={x:Type inputs:DropdownPopupHost}}}" /&gt;
        /// </code>
        /// </example>
        /// <para>
        /// The plain <c>{Binding SaveCommand}</c> form already works: <see cref="FrameworkElement.DataContext"/>
        /// is relayed from the button. This is for everything a <c>DataContext</c> cannot carry.
        /// </para>
        /// </summary>
        public static readonly DependencyProperty PlacementTargetProperty = DependencyProperty.Register(
            nameof(PlacementTarget),
            typeof(UIElement),
            typeof(DropdownPopupHost),
            new FrameworkPropertyMetadata(null));

        /// <inheritdoc cref="PlacementTargetProperty"/>
        public UIElement? PlacementTarget
        {
            get { return (UIElement?)GetValue(PlacementTargetProperty); }
            set { SetValue(PlacementTargetProperty, value); }
        }
    }

    /// <summary>
    /// Turns any <see cref="ToggleButton"/> into a dropdown: the button stays yours (its own
    /// <see cref="FrameworkElement.Style"/>, <c>Sizing.IsSquare</c>, content, triggers and bindings)
    /// and <see cref="PopupProperty"/> hangs a themed popup off it, open while the button is checked.
    /// <example>
    /// <code>
    /// &lt;ToggleButton Content="Actions" inputs:Dropdown.Placement="BottomRight"&gt;
    ///     &lt;inputs:Dropdown.Popup&gt;
    ///         &lt;TextBlock Text="Anything." /&gt;
    ///     &lt;/inputs:Dropdown.Popup&gt;
    /// &lt;/ToggleButton&gt;
    /// </code>
    /// </example>
    /// </summary>
    public static class Dropdown
    {
        #region Popup
        /// <summary>Content shown in the popup. Setting it is what turns the button into a dropdown.</summary>
        public static readonly DependencyProperty PopupProperty = DependencyProperty.RegisterAttached(
            "Popup",
            typeof(object),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(null, OnPopupChanged));

        public static object? GetPopup(DependencyObject element) { return element.GetValue(PopupProperty); }

        public static void SetPopup(DependencyObject element, object? value) { element.SetValue(PopupProperty, value); }
        #endregion

        #region Placement
        /// <summary>Corner alignment of the popup relative to the toggle button.</summary>
        public static readonly DependencyProperty PlacementProperty = DependencyProperty.RegisterAttached(
            "Placement",
            typeof(DropdownPlacement),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(DropdownPlacement.BottomLeft, OnPlacementChanged));

        public static DropdownPlacement GetPlacement(DependencyObject element)
        {
            return (DropdownPlacement)element.GetValue(PlacementProperty);
        }

        public static void SetPlacement(DependencyObject element, DropdownPlacement value)
        {
            element.SetValue(PlacementProperty, value);
        }
        #endregion

        #region Offsets
        /// <summary>Extra horizontal offset applied on top of <see cref="PlacementProperty"/>.</summary>
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached(
            "HorizontalOffset",
            typeof(double),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(0d));

        public static double GetHorizontalOffset(DependencyObject element)
        {
            return (double)element.GetValue(HorizontalOffsetProperty);
        }

        public static void SetHorizontalOffset(DependencyObject element, double value)
        {
            element.SetValue(HorizontalOffsetProperty, value);
        }

        /// <summary>Extra vertical offset applied on top of <see cref="PlacementProperty"/>.</summary>
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached(
            "VerticalOffset",
            typeof(double),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(2d));

        public static double GetVerticalOffset(DependencyObject element)
        {
            return (double)element.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(DependencyObject element, double value)
        {
            element.SetValue(VerticalOffsetProperty, value);
        }
        #endregion

        #region PopupStyle
        /// <summary>
        /// Style of the <see cref="DropdownPopupHost"/> drawing the popup chrome — its
        /// <see cref="Control.Padding"/>, background, border and corner radius. A
        /// <see cref="System.Windows.Controls.Primitives.Popup"/> is only a positioning primitive and has
        /// nothing worth styling, so this targets the chrome instead. Base it on the default style to
        /// keep the theme:
        /// <example>
        /// <code>
        /// &lt;Style TargetType="inputs:DropdownPopupHost" BasedOn="{StaticResource {x:Type inputs:DropdownPopupHost}}"&gt;
        ///     &lt;Setter Property="Padding" Value="8" /&gt;
        /// &lt;/Style&gt;
        /// </code>
        /// </example>
        /// </summary>
        public static readonly DependencyProperty PopupStyleProperty = DependencyProperty.RegisterAttached(
            "PopupStyle",
            typeof(Style),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(null, OnPopupStyleChanged));

        public static Style? GetPopupStyle(DependencyObject element)
        {
            return (Style?)element.GetValue(PopupStyleProperty);
        }

        public static void SetPopupStyle(DependencyObject element, Style? value)
        {
            element.SetValue(PopupStyleProperty, value);
        }
        #endregion

        #region CloseOnClick
        /// <summary>
        /// Closes the popup when a button inside it is clicked, the way picking a command from a menu
        /// dismisses it. Defaults to <c>false</c>, the popup then staying open until a click outside.
        /// <para>
        /// This tracks <see cref="ButtonBase.ClickEvent"/> rather than any mouse click, so a
        /// <see cref="TextBox"/> or a <see cref="Slider"/> in the popup stays usable. A
        /// <see cref="CheckBox"/> or a nested <see cref="ToggleButton"/> is a
        /// <see cref="ButtonBase"/> too and does close it — keep those out of a popup that needs
        /// this on.
        /// </para>
        /// </summary>
        public static readonly DependencyProperty CloseOnClickProperty = DependencyProperty.RegisterAttached(
            "CloseOnClick",
            typeof(bool),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(false));

        public static bool GetCloseOnClick(DependencyObject element)
        {
            return (bool)element.GetValue(CloseOnClickProperty);
        }

        public static void SetCloseOnClick(DependencyObject element, bool value)
        {
            element.SetValue(CloseOnClickProperty, value);
        }
        #endregion

        /// <summary>The popup created for a button, kept alive by the button itself.</summary>
        private static readonly DependencyProperty PopupInstanceProperty = DependencyProperty.RegisterAttached(
            "PopupInstance",
            typeof(Popup),
            typeof(Dropdown),
            new PropertyMetadata(null));

        private static void OnPopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ToggleButton button)
                return;

            // Re-setting the content must not leave the previous popup hanging around.
            if (button.GetValue(PopupInstanceProperty) is Popup previous)
            {
                previous.IsOpen = false;
                BindingOperations.ClearAllBindings(previous);
                previous.Child = null;
                previous.PlacementTarget = null;
                previous.CustomPopupPlacementCallback = null;
                button.SetValue(PopupInstanceProperty, null);
            }

            if (e.NewValue == null)
                return;

            var host = new DropdownPopupHost { Content = e.NewValue, PlacementTarget = button };
            // A Popup sits outside the button logical tree, so nothing flows down to it on its own.
            host.SetBinding(FrameworkElement.DataContextProperty, Bind(button, FrameworkElement.DataContextProperty));
            host.SetBinding(Sizing.SizeProperty, Bind(button, Sizing.SizeProperty));
            ApplyPopupStyle(host, GetPopupStyle(button));

            // Read CloseOnClick when the click happens, so flipping it later needs no rewiring.
            host.AddHandler(
                ButtonBase.ClickEvent,
                new RoutedEventHandler((_, _) =>
                {
                    if (GetCloseOnClick(button))
                        button.IsChecked = false;
                }));

            var popup = new Popup
            {
                Child = host,
                PlacementTarget = button,
                Placement = PlacementMode.Custom,
                StaysOpen = false,
                AllowsTransparency = true
            };
            popup.CustomPopupPlacementCallback = (popupSize, targetSize, offset)
                => PlacePopup(button, popupSize, targetSize, offset);

            // Two way so a dismissal (StaysOpen=False) unchecks the button.
            popup.SetBinding(
                Popup.IsOpenProperty,
                new Binding(nameof(ToggleButton.IsChecked)) { Source = button, Mode = BindingMode.TwoWay });
            popup.SetBinding(Popup.HorizontalOffsetProperty, Bind(button, HorizontalOffsetProperty));
            popup.SetBinding(Popup.VerticalOffsetProperty, Bind(button, VerticalOffsetProperty));

            button.SetValue(PopupInstanceProperty, popup);
        }

        private static void OnPopupStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d.GetValue(PopupInstanceProperty) is Popup { Child: DropdownPopupHost host })
                ApplyPopupStyle(host, (Style?)e.NewValue);
        }

        private static void ApplyPopupStyle(DropdownPopupHost host, Style? style)
        {
            // A local null would shadow the implicit style and strip the chrome, so clear instead.
            if (style == null)
                host.ClearValue(FrameworkElement.StyleProperty);
            else
                host.Style = style;
        }

        private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // PlacePopup reads Placement live, the callback only needs to be re-run.
            if (d.GetValue(PopupInstanceProperty) is Popup popup && popup.IsOpen)
            {
                popup.IsOpen = false;
                popup.IsOpen = true;
            }
        }

        private static Binding Bind(DependencyObject source, DependencyProperty property)
        {
            return new Binding { Source = source, Path = new PropertyPath(property), Mode = BindingMode.OneWay };
        }

        private static CustomPopupPlacement[] PlacePopup(
            ToggleButton button,
            Size popupSize,
            Size targetSize,
            Point offset)
        {
            DropdownPlacement placement = GetPlacement(button);

            double x = placement is DropdownPlacement.BottomRight or DropdownPlacement.TopRight
                ? targetSize.Width - popupSize.Width
                : 0;
            double y = placement is DropdownPlacement.TopLeft or DropdownPlacement.TopRight
                ? -popupSize.Height
                : targetSize.Height;

            return new[]
            {
                new CustomPopupPlacement(new Point(x + offset.X, y + offset.Y), PopupPrimaryAxis.Vertical)
            };
        }
    }
}
