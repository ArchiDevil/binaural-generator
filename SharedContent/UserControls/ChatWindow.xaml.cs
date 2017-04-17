using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SharedLibrary.UserControls
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public class ChatMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public DateTime Time { get; set; }

        public ChatMessageEventArgs(string message, DateTime time)
        {
            Message = message;
            Time = time;
        }
    }

    public partial class ChatWindow : UserControl
    {
        public delegate void ChatMessageHandler(object sender, ChatMessageEventArgs e);

        public event ChatMessageHandler ChatMessage;

        public ChatWindow()
        {
            InitializeComponent();
        }

        public void PushChatMessage(string message, DateTime time)
        {
            // Checking if this thread has access to the object.
            if (Dispatcher.CheckAccess())
            {
                // This thread has access so it can update the UI thread.
                AddMessage(message, time, true);
            }
            else
            {
                // This thread does not have access to the UI thread.
                // Place the update method on the Dispatcher of the UI thread.
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    AddMessage(message, time, true);
                }), DispatcherPriority.ContextIdle);
            }
        }

        private void ChatType_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || e.Key != Key.Enter)
                return;

            e.Handled = SendMessage();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = SendMessage();
        }

        private bool SendMessage()
        {
            string message = chatType.Text;
            if (message.Length == 0)
                return false;
            DateTime messageTime = DateTime.Now;

            ChatMessage?.Invoke(this, new ChatMessageEventArgs(chatType.Text, DateTime.Now));
            chatType.Text = "";

            AddMessage(message, messageTime, false);
            return true;
        }

        private void AddMessage(string message, DateTime time, bool received)
        {
            Paragraph timeString = new Paragraph(new Run(time.ToLongTimeString()))
            {
                FontSize = 11,
                FontFamily = new FontFamily("Arial"),
                Foreground = Brushes.Gray
            };

            Paragraph messageString = new Paragraph(new Run(message))
            {
                FontSize = 12,
                FontFamily = new FontFamily("Arial"),
                Foreground = received
                            ? (SolidColorBrush)(new BrushConverter().ConvertFrom("#aa4444"))
                            : (SolidColorBrush)(new BrushConverter().ConvertFrom("#4444aa"))
            };

            TableRow row = new TableRow();
            TableCell timeCell = new TableCell();
            TableCell messageCell = new TableCell();

            timeCell.Blocks.Add(timeString);
            messageCell.Blocks.Add(messageString);

            row.Cells.Add(timeCell);
            row.Cells.Add(messageCell);

            table.RowGroups.First().Rows.Add(row);
        }
    }
}
