using System;
using System.Windows;

using NetworkLayer.Protocol;

namespace ExperimenterUI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable
    {
        private ClientProtocol protocol = new ClientProtocol("TEST TEST TEST");
        private ExperimenterApplicationModel model = null;

        public MainWindow()
        {
            InitializeComponent();

            model = new ExperimenterApplicationModel(protocol);
            DataContext = model;
            protocol.ChatMessageReceive += ChatMessageReceiveHandler;
        }

        private void ChatWindow_ChatMessage(string message, DateTime time)
        {
            model.SendChatMessage(message, time);
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ConnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ConnectionDialog dialog = new ConnectionDialog(model);
            dialog.ShowDialog();
        }

        public void Dispose()
        {
            model.Dispose();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            model.Dispose();
            model = null;
        }

        private void ChatMessageReceiveHandler(object sender, ClientChatMessageEventArgs e)
        {
            chatWindow.PushChatMessage(e.message, e.sentTime);
        }
    }
}
