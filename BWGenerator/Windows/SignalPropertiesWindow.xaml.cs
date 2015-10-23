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
using System.Windows.Shapes;

using BWGenerator.Models;
using SharedLibrary.Models;

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
