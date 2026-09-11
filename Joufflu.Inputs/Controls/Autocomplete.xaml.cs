using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Joufflu.Inputs.Controls
{
    /// <summary>
    /// Text input completing what is typed with an item of <see cref="ItemsSource"/> : the rest of the
    /// most likely one is drawn greyed after the caret, <kbd>Tab</kbd> takes it, and the items starting
    /// with what is typed are listed under the box, each drawn with <see cref="ItemTemplate"/>.
    /// <para>
    /// Nothing forces the text into the list : <see cref="SelectedItem"/> holds the item whose text was
    /// typed, and the typed <see cref="string"/> itself when no item matches it.
    /// </para>
    /// <para>
    /// The text of an item is read from its <see cref="TextMemberPath"/> property, or from its
    /// <see cref="object.ToString"/> when no path is given.
    /// </para>
    /// </summary>
    [TemplatePart(Name = TextBoxPart, Type = typeof(TextBox))]
    [TemplatePart(Name = PopupPart, Type = typeof(Popup))]
    [TemplatePart(Name = SuggestionsPart, Type = typeof(ListBox))]
    public partial class Autocomplete : Control
    {
        private const string TextBoxPart = "PART_EditableTextBox";
        private const string PopupPart = "PART_Popup";
        private const string SuggestionsPart = "PART_Suggestions";

        static Autocomplete()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Autocomplete), new FrameworkPropertyMetadata(typeof(Autocomplete)));
            // The text box inside is what is typed in : it takes the focus and the tab stop, the
            // control around it is only its chrome.
            FocusableProperty.OverrideMetadata(typeof(Autocomplete), new FrameworkPropertyMetadata(false));
        }

        private TextBox? _textBox;
        private ListBox? _suggestionList;

        // True while the control writes Text and SelectedItem itself, so the two stay one value
        // instead of answering each other.
        private bool _syncing;

        // True while listening to an observable ItemsSource, so subscribe/unsubscribe stay
        // idempotent across ItemsSource changes and Loaded/Unloaded cycles.
        private bool _observing;

        public Autocomplete()
        {
            // A collection owned by a view model outlives the control : listen to it only while on
            // the visual tree, so it cannot keep this control (and its subtree) alive.
            Loaded += (_, _) => Subscribe(ItemsSource);
            Unloaded += (_, _) =>
            {
                Unsubscribe(ItemsSource);
                CloseDropDown();
            };
        }

        #region Items

        /// <summary>Items the completion and the drop down are drawn from. Any list, like a combo box.</summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(Autocomplete),
            new PropertyMetadata(null, (o, e) => ((Autocomplete)o).OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable)));

        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>Template each suggestion of the drop down is drawn with.</summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(Autocomplete),
            new PropertyMetadata(null));

        public DataTemplate? ItemTemplate
        {
            get => (DataTemplate?)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        /// <summary>
        /// Property of an item holding the text that is typed and completed. The item's
        /// <see cref="object.ToString"/> when left empty.
        /// </summary>
        public static readonly DependencyProperty TextMemberPathProperty = DependencyProperty.Register(
            nameof(TextMemberPath),
            typeof(string),
            typeof(Autocomplete),
            new PropertyMetadata(null, (o, _) => ((Autocomplete)o).RefreshSuggestions()));

        public string? TextMemberPath
        {
            get => (string?)GetValue(TextMemberPathProperty);
            set => SetValue(TextMemberPathProperty, value);
        }

        /// <summary>
        /// Items starting with what is typed, in the order of <see cref="ItemsSource"/> : what the drop
        /// down lists, the first one being the most likely and the one the completion comes from.
        /// </summary>
        public ObservableCollection<object> Suggestions { get; } = new();

        #endregion

        #region Value

        /// <summary>What is typed, without the greyed completion.</summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(Autocomplete),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, _) => ((Autocomplete)o).OnTextChanged()));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// The item whose text is the one typed, or that typed text as a <see cref="string"/> when the
        /// list holds nothing matching it. Null while the box is empty.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(Autocomplete),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((Autocomplete)o).OnSelectedItemChanged(e.NewValue)));

        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>The greyed part drawn after the caret : what the highlighted item adds to the text.</summary>
        private static readonly DependencyPropertyKey CompletionPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Completion),
            typeof(string),
            typeof(Autocomplete),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CompletionProperty = CompletionPropertyKey.DependencyProperty;

        public string Completion => (string)GetValue(CompletionProperty);

        /// <summary>The suggestion the completion is taken from, and the one <kbd>Tab</kbd> accepts.</summary>
        private static readonly DependencyPropertyKey HighlightedItemPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(HighlightedItem),
            typeof(object),
            typeof(Autocomplete),
            new PropertyMetadata(null));

        public static readonly DependencyProperty HighlightedItemProperty = HighlightedItemPropertyKey.DependencyProperty;

        public object? HighlightedItem => GetValue(HighlightedItemProperty);

        #endregion

        #region Drop down

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(Autocomplete),
            new PropertyMetadata(false));

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(
            nameof(MaxDropDownHeight),
            typeof(double),
            typeof(Autocomplete),
            new PropertyMetadata(200d));

        public double MaxDropDownHeight
        {
            get => (double)GetValue(MaxDropDownHeightProperty);
            set => SetValue(MaxDropDownHeightProperty, value);
        }

        #endregion

        #region On changed

        public override void OnApplyTemplate()
        {
            if (_suggestionList != null)
                _suggestionList.PreviewMouseLeftButtonUp -= OnSuggestionClicked;

            base.OnApplyTemplate();

            _textBox = GetTemplateChild(TextBoxPart) as TextBox;
            _suggestionList = GetTemplateChild(SuggestionsPart) as ListBox;

            if (_suggestionList == null)
                return;

            _suggestionList.PreviewMouseLeftButtonUp += OnSuggestionClicked;
            // A new template comes with a new list : hand it back the highlight it has to draw.
            _suggestionList.SelectedItem = HighlightedItem;
        }

        private void OnItemsSourceChanged(IEnumerable? oldValue, IEnumerable? newValue)
        {
            Unsubscribe(oldValue);
            if (IsLoaded)
                Subscribe(newValue);

            RefreshSuggestions();
        }

        private void Subscribe(IEnumerable? items)
        {
            if (_observing || items is not INotifyCollectionChanged observable)
                return;
            observable.CollectionChanged += OnItemsChanged;
            _observing = true;
        }

        private void Unsubscribe(IEnumerable? items)
        {
            if (!_observing || items is not INotifyCollectionChanged observable)
                return;
            observable.CollectionChanged -= OnItemsChanged;
            _observing = false;
        }

        private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshSuggestions();

        private void OnTextChanged()
        {
            // The control is writing the value itself, it already knows what to show for it.
            if (_syncing)
                return;

            RefreshSuggestions();
            SelectItemFromText();

            // Typing opens the drop down, a text pushed by a binding while the box is not being
            // edited must not pop anything up.
            if (IsKeyboardFocusWithin)
                SetCurrentValue(IsDropDownOpenProperty, Suggestions.Count > 0 && Text?.Length > 0);
        }

        private void OnSelectedItemChanged(object? value)
        {
            if (_syncing)
                return;

            // Set from the outside : show it, without taking it for something being typed.
            SetTextAndSelection(value == null ? string.Empty : TextOf(value) ?? string.Empty, value);
            RefreshSuggestions();
            SetHighlighted(null);
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            if ((bool)e.NewValue)
                return;

            // Leaving the box keeps what is written as it is, only spelling it the way the list does
            // when the list holds it.
            object? match = FindItemByText(Text);
            if (match != null)
                Accept(match);

            SetHighlighted(null);
            CloseDropDown();
        }

        #endregion

        #region UI events

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Tab:
                    // Nothing greyed out to take : let the focus move on, as a text box would.
                    if (Completion.Length > 0)
                        e.Handled = AcceptHighlighted();
                    break;
                case Key.Enter:
                    if (IsDropDownOpen)
                    {
                        if (AcceptHighlighted() == false)
                            CloseDropDown();
                        e.Handled = true;
                    }
                    break;
                case Key.Escape:
                    if (IsDropDownOpen)
                    {
                        SetHighlighted(null);
                        CloseDropDown();
                        e.Handled = true;
                    }
                    break;
                case Key.Down:
                    MoveHighlight(1);
                    e.Handled = true;
                    break;
                case Key.Up:
                    MoveHighlight(-1);
                    e.Handled = true;
                    break;
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (_textBox == null)
                return;
            // The box itself places the caret where it was clicked, nothing to do.
            if (e.OriginalSource is Visual source && (ReferenceEquals(source, _textBox) || _textBox.IsAncestorOf(source)))
                return;

            // A click landing beside the text, on the chrome around it, still edits the text.
            _textBox.Focus();
            _textBox.CaretIndex = _textBox.Text.Length;
            e.Handled = true;
        }

        private void OnSuggestionClicked(object sender, MouseButtonEventArgs e)
        {
            if (_suggestionList == null || e.OriginalSource is not DependencyObject source)
                return;
            if (ItemsControl.ContainerFromElement(_suggestionList, source) is not ListBoxItem container)
                return;

            object item = _suggestionList.ItemContainerGenerator.ItemFromContainer(container);
            if (item == DependencyProperty.UnsetValue)
                return;

            Accept(item);
            e.Handled = true;
        }

        #endregion

        #region Completion

        /// <summary>Opens the drop down, then walks the suggestions, the completion following the highlight.</summary>
        private void MoveHighlight(int offset)
        {
            if (Suggestions.Count == 0)
                return;

            if (IsDropDownOpen == false)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
                // Typing already picked the most likely one, opening the list doesn't move it.
                if (HighlightedItem != null)
                    return;
            }

            object? highlighted = HighlightedItem;
            int index = highlighted == null ? -1 : Suggestions.IndexOf(highlighted);
            SetHighlighted(Suggestions[Math.Clamp(index + offset, 0, Suggestions.Count - 1)]);
        }

        /// <summary>Takes the highlighted suggestion, when the list has one to take.</summary>
        private bool AcceptHighlighted()
        {
            object? item = HighlightedItem;
            if (item == null)
                return false;

            Accept(item);
            return true;
        }

        /// <summary>Takes an item as the value : its text replaces what is typed and the drop down closes.</summary>
        private void Accept(object item)
        {
            SetTextAndSelection(TextOf(item) ?? string.Empty, item);

            RefreshSuggestions();
            SetHighlighted(null);
            CloseDropDown();

            if (_textBox != null)
                _textBox.CaretIndex = _textBox.Text.Length;
        }

        /// <summary>Writes both halves of the value at once, so neither answers the other.</summary>
        private void SetTextAndSelection(string text, object? selected)
        {
            _syncing = true;
            try
            {
                SetCurrentValue(TextProperty, text);
                SetCurrentValue(SelectedItemProperty, selected);
            }
            finally
            {
                _syncing = false;
            }
        }

        private void SelectItemFromText()
        {
            object? item = FindItemByText(Text);
            // Nothing in the list spelled that way : the text itself is the value.
            object? value = item ?? (string.IsNullOrEmpty(Text) ? null : Text);

            _syncing = true;
            try
            {
                SetCurrentValue(SelectedItemProperty, value);
            }
            finally
            {
                _syncing = false;
            }
        }

        private void CloseDropDown() => SetCurrentValue(IsDropDownOpenProperty, false);

        private void RefreshSuggestions()
        {
            Suggestions.Clear();
            string text = Text ?? string.Empty;

            if (ItemsSource != null)
            {
                foreach (object? item in ItemsSource)
                {
                    if (item != null && StartsWithText(item, text))
                        Suggestions.Add(item);
                }
            }

            // The first match is the most likely one : it carries the completion until the arrow keys
            // move on. An empty box has nothing to complete, so it highlights nothing.
            SetHighlighted(text.Length > 0 && Suggestions.Count > 0 ? Suggestions[0] : null);
        }

        private void SetHighlighted(object? item)
        {
            SetValue(HighlightedItemPropertyKey, item);

            if (_suggestionList != null)
            {
                _suggestionList.SelectedItem = item;
                if (item != null)
                    _suggestionList.ScrollIntoView(item);
            }

            UpdateCompletion();
        }

        private void UpdateCompletion()
        {
            string text = Text ?? string.Empty;
            object? highlighted = HighlightedItem;
            string? itemText = highlighted == null ? null : TextOf(highlighted);

            // The rest of an item only reads as the rest of what is typed when it is drawn where the
            // caret is : editing the middle of the text completes nothing.
            bool caretAtEnd = _textBox == null || _textBox.CaretIndex == _textBox.Text.Length;
            string completion = caretAtEnd
                && itemText != null
                && itemText.StartsWith(text, StringComparison.CurrentCultureIgnoreCase)
                    ? itemText.Substring(text.Length)
                    : string.Empty;

            SetValue(CompletionPropertyKey, completion);
        }

        private bool StartsWithText(object item, string text)
            => text.Length == 0 || TextOf(item)?.StartsWith(text, StringComparison.CurrentCultureIgnoreCase) == true;

        private object? FindItemByText(string? text)
        {
            if (ItemsSource == null || string.IsNullOrEmpty(text))
                return null;

            foreach (object? item in ItemsSource)
            {
                if (item != null && string.Equals(TextOf(item), text, StringComparison.CurrentCultureIgnoreCase))
                    return item;
            }
            return null;
        }

        /// <summary>Text of an item : its <see cref="TextMemberPath"/> property, or what it says it is.</summary>
        protected virtual string? TextOf(object? item)
        {
            if (item == null)
                return null;
            if (string.IsNullOrEmpty(TextMemberPath))
                return item.ToString();

            PropertyInfo? property = item.GetType().GetProperty(TextMemberPath);
            return property != null ? property.GetValue(item)?.ToString() : item.ToString();
        }

        #endregion
    }
}
