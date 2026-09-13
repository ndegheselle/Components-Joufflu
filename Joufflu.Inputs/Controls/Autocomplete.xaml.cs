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
    [TemplatePart(Name = TextBoxPart, Type = typeof(TextBox))]
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

        public IList Suggestions { get; set; }

        public Autocomplete()
        {}

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        // Get text
        // Filter list
        // Get most likely
        // truncate the rest to display it
    }
}
