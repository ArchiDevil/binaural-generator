using System.Windows;

using BWGenerator.Models;

namespace BWGenerator
{
    /// <summary>
    /// Interaction logic for SignalPropertiesWindow.xaml
    /// </summary>
    public partial class SignalPropertiesWindow : Window
    {
        SignalViewModel viewModel = null;
        public SignalPropertiesWindow(Signal currentSignal)
        {
            InitializeComponent();
            viewModel = new SignalViewModel(currentSignal);
            DataContext = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
