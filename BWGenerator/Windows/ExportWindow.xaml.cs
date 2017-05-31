using System.Windows;

using BWGenerator.Models;
using Microsoft.Win32;

namespace BWGenerator
{
    /// <summary>
    /// Логика взаимодействия для ExportWindow.xaml
    /// </summary>
    public partial class ExportWindow : Window
    {
        ExportWindowViewModel viewModel = null;

        public ExportWindow(PresetModel preset)
        {
            InitializeComponent();
            viewModel = new ExportWindowViewModel(preset);
            DataContext = viewModel;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog()
            {
                AddExtension = true,
                Filter = "WAV file (*.wav)|*.wav"
            };

            if (saveDialog.ShowDialog(this) == true)
                viewModel.FileName = saveDialog.FileName;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.Export();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
