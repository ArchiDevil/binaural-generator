using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SharedLibrary.UserControls
{
    /// <summary>
    /// Логика взаимодействия для EditableTextBox.xaml
    /// </summary>
    public partial class EditableTextBox : UserControl
    {
        public EditableTextBox()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBox), new UIPropertyMetadata(string.Empty));

        private void EditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Deactivate();
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Activate();
        }

        private void EditBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                TextBox tBox = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null) { binding.UpdateSource(); }

                Deactivate();
                e.Handled = true;
            }
        }

        private void Activate()
        {
            TextBlock.Visibility = Visibility.Collapsed;
            EditBox.Visibility = Visibility.Visible;
            EditBox.Height = TextBlock.Height;
            EditBox.Width = TextBlock.Width;
            EditBox.Focus();
            EditBox.SelectAll();
        }

        private void Deactivate()
        {
            TextBlock.Visibility = Visibility.Visible;
            EditBox.Visibility = Visibility.Collapsed;
        }
    }
}
