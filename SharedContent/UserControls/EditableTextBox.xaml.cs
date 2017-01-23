using System.Windows;
using System.Windows.Controls;
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
            DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBox), new UIPropertyMetadata());

        private void EditBox_LostFocus(object sender, RoutedEventArgs e)
        {
            textBlock.Text = editBox.Text;
            textBlock.Visibility = Visibility.Visible;
            editBox.Visibility = Visibility.Collapsed;
        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            textBlock.Visibility = Visibility.Collapsed;
            editBox.Text = textBlock.Text;
            editBox.Visibility = Visibility.Visible;
            editBox.Height = textBlock.Height;
            editBox.Width = textBlock.Width;
            editBox.Focus();
        }
    }
}
