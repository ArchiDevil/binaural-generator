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

        private void ChatWindow_ChatMessage(object sender, SharedLibrary.UserControls.ChatMessageEventArgs args)
        {
            _model.SendChatMessage(args.Message, args.Time);
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
