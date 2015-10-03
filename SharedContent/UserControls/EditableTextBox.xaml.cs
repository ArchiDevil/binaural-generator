using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharedContent.UserControls
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

        private void textBlock_MouseRightButtonUp(object sender
                                       , MouseButtonEventArgs e)
        {
            textBlock.Visibility = Visibility.Collapsed;
            editBox.Text = textBlock.Text;
            editBox.Visibility = Visibility.Visible;
            editBox.Height = textBlock.Height;
            editBox.Width = textBlock.Width;
            editBox.Focus();
        }

        private void editBox_LostFocus(object sender, RoutedEventArgs e)
        {
            textBlock.Text = editBox.Text;
            textBlock.Visibility = Visibility.Visible;
            editBox.Visibility = Visibility.Collapsed;
        }

    }
}
