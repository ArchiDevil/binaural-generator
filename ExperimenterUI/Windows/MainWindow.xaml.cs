using System;
using System.Windows;
using System.Windows.Controls;
using NetworkLayer.Protocol;

namespace ExperimenterUI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable
    {
        private ClientProtocol _protocol = new ClientProtocol("TEST TEST TEST");
        private ExperimenterApplicationModel _model = null;

        public MainWindow()
        {
            InitializeComponent();

            _model = new ExperimenterApplicationModel(_protocol);
            DataContext = _model;
            signalComboBox.SelectedIndex = 0;
            _protocol.ChatMessageReceive += ChatMessageReceiveHandler;
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
        }

        private void ChatMessageReceiveHandler(object sender, ClientChatMessageEventArgs e)
        {
            chatWindow.PushChatMessage(e.message, e.sentTime);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _model.SelectChannel((sender as ComboBox).SelectedIndex);
        }
    }
}
