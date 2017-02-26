using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using NetworkLayer;
using NetworkLayer.ProtocolShared;

namespace ExperimenterUI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable
    {
        private ClientProtocol                  _protocol = new ClientProtocol("TEST TEST TEST");
        private ExperimenterApplicationModel    _model = null;

        public MainWindow()
        {
            InitializeComponent();

            _model = new ExperimenterApplicationModel(_protocol);
            DataContext = _model;
            signalComboBox.SelectedIndex = 0;
            _protocol.ChatMessageReceived += ChatMessageReceiveHandler;
        }

        private void ChatWindow_ChatMessage(string message, DateTime time)
        {
            _model.SendChatMessage(message, time);
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ConnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConnectionDialog dialog = new ConnectionDialog(_model);
            dialog.ShowDialog();
        }

        public void Dispose()
        {
            _model.Dispose();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _model.Dispose();
            _model = null;

            _protocol.Disconnect();
            _protocol = null;
        }

        private void ChatMessageReceiveHandler(object sender, ClientChatMessageEventArgs e)
        {
            chatWindow.PushChatMessage(e.message, e.sentTime);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _model.SelectChannel((sender as ComboBox).SelectedIndex);
        }

        private void NewSessionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _model.StartNewSession();
        }

        private void SaveSessionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                Filter = "Session file (*.seslog)|*.seslog",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (dialog.ShowDialog(this) == true)
                _model.CloseSession(dialog.FileName);
        }
    }
}
