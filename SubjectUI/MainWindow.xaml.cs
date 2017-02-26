using System;
using System.Windows;

namespace SubjectUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        SubjectApplicationModel _model = new SubjectApplicationModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _model;
            _model.ChatMessageReceivedEvent += Model_ChatMessageReceivedEvent;
        }

        private void Model_ChatMessageReceivedEvent(string message, DateTime time)
        {
            chatWindow.PushChatMessage(message, time);
        }

        private void ChatWindow_ChatMessage(string message, DateTime time)
        {
            _model.SendChatMessage(message, time);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _model.CheckSystems();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _model.ProtocolStop();
            _model = null;
        }
    }
}
