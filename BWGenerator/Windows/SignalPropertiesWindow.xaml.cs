using System.Collections.Generic;
using System.Windows;

using BWGenerator.Models;
using SharedLibrary.Code;

namespace BWGenerator
{
    /// <summary>
    /// Interaction logic for SignalPropertiesWindow.xaml
    /// </summary>
    public partial class SignalPropertiesWindow : Window
    {
        SignalViewModel viewModel = null;

        public SignalPropertiesWindow(Signal currentSignal, List<NoiseDataPoint> noiseDataPoints)
        {
            InitializeComponent();
            viewModel = new SignalViewModel(currentSignal, noiseDataPoints);
            DataContext = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
