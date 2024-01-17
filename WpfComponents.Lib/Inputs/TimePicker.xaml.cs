﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfComponents.Lib.Inputs
{
    public class FormatedTextBox : TextBox
    {
        public string InputRegex { get; set; }
        public event Action<List<string>> PartsChanged;

        public FormatedTextBox()
        {
            Parts = new List<string>();
        }     

        // Dependency property for List<string> Parts
        public static readonly DependencyProperty PartsProperty = DependencyProperty.Register(
            "Parts",
            typeof(List<string>),
            typeof(FormatedTextBox),
            new PropertyMetadata(null));


        public List<string> Parts
        {
            get { return (List<string>)GetValue(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if(InputRegex == null)
                return;

            // Get the parts based on the regex
            Regex regex = new Regex(InputRegex);
            Match match = regex.Match(Text);
            if(!match.Success)
                return;

            List<string> parts = new List<string>();
            foreach(Group group in match.Groups)
                parts.Add(group.Value);
            Parts = parts;
            Debug.WriteLine("Parts : " + string.Join(", ", parts));
        }
    }

    // XXX : use classic string format, only have to find a way to link the string format to the actual DateTime. or maybe using regex and group ?
    public partial class TimePicker : UserControl, INotifyPropertyChanged
    {
        #region Properties
        const string TIME_FORMAT_CHARS = "yMdHhms";

        public DateTime Time { get; set; } = DateTime.Now;

        public string Format { get; set; } = "HH:mm:ss";

        public string FormattedTime { get { return Time.ToString(Format); } }

        private bool _isEditing = false;

        public DateTime TestDateTime { get; set; } = DateTime.Now;
        #endregion

        // Select a part of the time
        // Then pressing key change number on right, then left, then swap to next block on the right
        // Check if next input is ok (31h for exemple)

        public TimePicker() { InitializeComponent(); }

        #region UI Events

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if(_isEditing)
                return;

            // Get current textbox
            TextBox textBox = (TextBox)sender;
            // Get current caret position
            int caretIndex = textBox.CaretIndex;

            if(caretIndex < 0 || caretIndex >= Format.Length)
            {
                e.Handled = true;
                return;
            }

            StringBuilder timeStringBuilder = new StringBuilder(Format);
            char searchChar = timeStringBuilder[caretIndex];

            if(TIME_FORMAT_CHARS.IndexOf(searchChar) == -1)
            {
                e.Handled = true;
                return;
            }

            // Find the same char on the left and on the right
            int leftIndex = caretIndex;
            int rightIndex = caretIndex;
            while(leftIndex > 0 && timeStringBuilder[leftIndex] == searchChar)
                leftIndex--;
            while(rightIndex < timeStringBuilder.Length && timeStringBuilder[rightIndex] == searchChar)
                rightIndex++;

            _isEditing = true;
            // Set the textbox selection
            textBox.Select(leftIndex + 1, rightIndex - leftIndex - 1);
            _isEditing = false;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
        }
        #endregion

        private bool GetTimePart(string time, int index, out int? result)
        {
            if(index > time.Length)
            {
                result = null;
                return false;
            }

            StringBuilder timeStringBuilder = new StringBuilder(time);
            if(timeStringBuilder[index] == '1')
                timeStringBuilder[index] = '2';
            else
                timeStringBuilder[index] = '1';

            DateTime newTime;
            try
            {
                newTime = DateTime.ParseExact(timeStringBuilder.ToString(), Format, CultureInfo.InvariantCulture);
            } catch(Exception e)
            {
                result = null;
                return true;
            }

            if(Time.Year != newTime.Year)
            {
                result = Time.Year;
                return true;
            } else if(Time.Month != newTime.Month)
            {
                result = Time.Month;
                return true;
            } else if(Time.Day != newTime.Day)
            {
                result = Time.Day;
                return true;
            } else if(Time.Hour != newTime.Hour)
            {
                result = Time.Hour;
                return true;
            } else if(Time.Minute != newTime.Minute)
            {
                result = Time.Minute;
                return true;
            } else if(Time.Second != newTime.Second)
            {
                result = Time.Second;
                return true;
            } else
            {
                result = null;
                return false;
            }
        }
    }
}
