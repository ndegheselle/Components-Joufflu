using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Joufflu.Inputs.Controls
{
    /// <summary>
    /// Inspired from : https://stackoverflow.com/a/41986141/10404482 
    /// </summary>
    public partial class ComboBoxSearch : ComboBox, INotifyPropertyChanged
    {
        static ComboBoxSearch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxSearch), new FrameworkPropertyMetadata(typeof(ComboBoxSearch)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICollectionView? SourceView { get; private set; }

        private TextBox? _editableTextBox;
        /// <summary>
        /// Previous text used to filter the items.
        /// </summary>
        private string? _previousRefreshText;

        // True while DoesItemPassFilter is attached to SourceView, so attach/detach stay
        // idempotent across ItemsSource changes and Loaded/Unloaded cycles.
        private bool _filterAttached;

        public ComboBoxSearch()
        {
            // Set default options
            IsEditable = true;
            StaysOpenOnEdit = true;
            IsTextSearchEnabled = false;
            // Don't let selection follow the view's current item: refreshing the filter moves
            // the CollectionView's CurrentItem, which would otherwise raise spurious selection
            // changes the first time the filter runs.
            IsSynchronizedWithCurrentItem = false;

            AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnTextChanged));

            // Release the filter on a possibly caller-owned view while off the visual tree,
            // so the view cannot keep this control (and its subtree) alive.
            Loaded += (_, _) => AttachFilter();
            Unloaded += (_, _) => DetachFilter();
        }

        #region On changed

        public override void OnApplyTemplate()
        {
            _editableTextBox = (TextBox)GetTemplateChild("PART_EditableTextBox");
            _editableTextBox.FontStyle = FontStyles.Italic;

            base.OnApplyTemplate();
        }

        private void AttachFilter()
        {
            if (SourceView is null || _filterAttached)
                return;
            SourceView.Filter += DoesItemPassFilter;
            _filterAttached = true;
        }

        private void DetachFilter()
        {
            if (SourceView is null || !_filterAttached)
                return;
            SourceView.Filter -= DoesItemPassFilter;
            _filterAttached = false;
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            DetachFilter();
            SourceView = null;

            if (newValue is ICollectionView view)
            {
                // Dedicated per-instance view (created below) or a view supplied by the caller.
                SourceView = view;
                AttachFilter();
                base.OnItemsSourceChanged(oldValue, newValue);
            }
            else if (newValue != null)
            {
                // Wrap raw collections in a dedicated view so two controls bound to the same
                // collection don't share the singleton DefaultView (and its filter).
                // SetCurrentValue preserves the user's ItemsSource binding.
                ICollectionView dedicated = newValue is IList list
                    ? new ListCollectionView(list)
                    : new CollectionViewSource { Source = newValue }.View;
                SetCurrentValue(ItemsSourceProperty, dedicated); // re-enters via the branch above
            }
            else
            {
                base.OnItemsSourceChanged(oldValue, newValue);
            }
        }

        #endregion

        #region UI events
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (IsDropDownOpen == false)
                    {
                        IsDropDownOpen = true;
                    }
                    else if (SelectedItem == null)
                    {
                        SelectedIndex = Items.Count - 1;
                    }
                    break;
                case Key.Down:
                    if (IsDropDownOpen == false)
                    {
                        IsDropDownOpen = true;
                    }
                    else if (SelectedItem == null)
                    {
                        SelectedIndex = 0;
                    }
                    break;
                case Key.Tab:
                case Key.Enter:
                    IsDropDownOpen = false;
                    break;
                case Key.Escape:
                    IsDropDownOpen = false;
                    SelectedItem = null;
                    break;
            }
            base.OnPreviewKeyDown(e);
        }

        void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedItem != null && Text == GetTextFromItem(SelectedItem))
                return;

            SelectedIndex = -1;
            if (IsDropDownOpen == false && !string.IsNullOrEmpty(Text))
            {
                IsDropDownOpen = true;

                // HACK : prevent the default behavior of the combobox to select all the text when the dropdown is opened
                if (_editableTextBox != null)
                    _editableTextBox.SelectionStart = _editableTextBox.Text.Length;
            }
            RefreshFilter();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            // Prevent having a value that doesn't match any item (could be misleading)
            if (SelectedItem == null)
                Clear();
            else if (_editableTextBox != null)
                _editableTextBox.FontStyle = FontStyles.Normal;

            base.OnLostKeyboardFocus(e);
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            // Opened through the chevron (or the arrow keys) rather than by typing : the text is
            // then the item that is selected, not a search, so the whole list has to be shown back
            // instead of the single item the previous filter left.
            RefreshFilter(true);

            base.OnDropDownOpened(e);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (_editableTextBox == null)
                return;

            // Show italic text if no item is selected
            if (SelectedItem != null)
            {
                Text = GetTextFromItem(SelectedItem);
                _editableTextBox.FontStyle = FontStyles.Normal;
                _editableTextBox.SelectAll();
            }
            else
            {
                _editableTextBox.FontStyle = FontStyles.Italic;
            }

            e.Handled = true;
        }

        #endregion

        #region Search & filters

        [RelayCommand]
        private void Clear()
        {
            Text = null;
        }

        /// <param name="force">
        /// Refresh even if the text has not changed, for the cases where what the filter makes of
        /// that text did change (the drop down being opened on a selected item, typically).
        /// </param>
        private void RefreshFilter(bool force = false)
        {
            if (ItemsSource == null)
                return;

            // Prevent unnecessary refresh if the text has not changed
            if (!force && _previousRefreshText == Text)
                return;
            _previousRefreshText = Text;

            SourceView?.Refresh();
            SelectFromFilter();
        }

        private void SelectFromFilter()
        {
            if (Text == string.Empty)
                return;

            // Select item that matches user input exactly
            for (int i = 0; i < Items.Count; i++)
            {
                if (Text == GetTextFromItem(Items[i]))
                {
                    SelectedIndex = i;
                    return;
                }
            }
        }

        protected virtual bool DoesItemPassFilter(object value)
        {
            if (value == null)
                return false;
            if (string.IsNullOrEmpty(Text))
                return true;
            // Text left by a selection is not something the user searched for : the list stays whole
            // so that the other choices remain reachable once an item has been picked.
            if (SelectedItem != null && Text == GetTextFromItem(SelectedItem))
                return true;

            return DoesValueContainSearch(value);
        }

        private bool DoesValueContainSearch(object value)
        {
            return GetTextFromItem(value)?.ToLower().Contains(Text.ToLower()) == true;
        }

        private string? GetTextFromItem(object item)
        {
            if (item == null)
                return string.Empty;
            if (string.IsNullOrEmpty(DisplayMemberPath))
                return item.ToString();

            PropertyInfo? displayMemberProperty = item.GetType().GetProperty(DisplayMemberPath);
            if (displayMemberProperty != null)
                return displayMemberProperty.GetValue(item)?.ToString();
            return item.ToString();
        }
        #endregion
    }
}