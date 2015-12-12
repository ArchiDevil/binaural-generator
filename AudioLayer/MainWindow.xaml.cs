using System.Windows;
using AudioCoreLib;

namespace AudioLayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Record record = null;
        bool recordState = false;

        public MainWindow()
        {
            InitializeComponent();
            record = new Record();
        }

        private void Recordbutton_Click(object sender, RoutedEventArgs e)
        {
            if (recordState)
            {
                Recordbutton.Content = "Record";
                record.StopRecording();
                recordState = false;
            }
            else
            {
                Recordbutton.Content = "Mute";
                record.StartRecording("record.wav");
                recordState = true;
            }
        }
    }
}
