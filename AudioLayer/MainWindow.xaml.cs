using System.Windows;
using AudioCoreLib;
using NetworkLayer;
using NetworkLayer.Protocol;

namespace AudioLayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Record record = null;
        bool recordState = false;

        InternetServerConnectionInterface server = null;
        InternetClientConnectionInterface client = null;

        public MainWindow()
        {
            InitializeComponent();

            server = new InternetServerConnectionInterface();
            server.StartListening("localhost", 11000);

            ClientProtocol protocol = new ClientProtocol("MyClient");
            protocol.Connect("localhost");
            //client = new InternetClientConnectionInterface();
            //client.Connect("localhost", 11000);

            record = new Record(protocol);
        }

        ~MainWindow()
        {
            //client.Disconnect();
            server.Shutdown();
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
                record.StartRecording();
                recordState = true;
            }
        }

        private void RecordInput(object sender, WaveInEventArgs e)
        {

        }
    }
}
