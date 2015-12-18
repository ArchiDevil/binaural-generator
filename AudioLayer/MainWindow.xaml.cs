using System.Windows;
using AudioCore;
using SharedLibrary.AudioProviders;
using NetworkLayer.Protocol;

namespace AudioLayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Record record = null;
        Playback playback = null;
        bool recordState = false;

        ServerProtocol server = null;
        ClientProtocol client = null;

        public MainWindow()
        {
            InitializeComponent();

            server = new ServerProtocol("MyServer");
            server.Bind("localhost");
            server.VoiceWindowReceive += VoiceWindowReceiveHandler;

            client = new ClientProtocol("MyClient");
            client.Connect("localhost");

            record = new Record(client);
            playback = new Playback(8000, 16, 1);
        }

        ~MainWindow()
        {
            client.Disconnect();
            server.Stop();
        }

        private void Recordbutton_Click(object sender, RoutedEventArgs e)
        {
            if (recordState)
            {
                Recordbutton.Content = "Record";
                record.StopRecording();
                playback.Stop();
                recordState = false;
            }
            else
            {
                Recordbutton.Content = "Mute";
                record.StartRecording();
                playback.Play();
                recordState = true;
            }
        }

        private void VoiceWindowReceiveHandler(object sender, VoiceWindowDataEventArgs e)
        {
            playback.AddSamples(e.data);
        }
    }
}
