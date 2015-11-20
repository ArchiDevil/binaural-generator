using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharedLibrary.UserControls
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : UserControl
    {
        public delegate void ChatMessageHandler(string message, DateTime time);

        public event ChatMessageHandler ChatMessage = delegate
        { };

        public ChatWindow()
        {
            InitializeComponent();
        }

        private void chatType_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || e.Key != Key.Enter)
                return;

            e.Handled = SendMessage();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = SendMessage();
        }

        bool SendMessage()
        {
            string message = chatType.Text;
            if (message.Length == 0)
                return false;
            DateTime messageTime = DateTime.Now;

            ChatMessage(chatType.Text, DateTime.Now);
            chatType.Text = "";

            Paragraph timeString = new Paragraph(new Run(messageTime.ToLongTimeString()))
            {
                FontSize = 11,
                FontFamily = new FontFamily("Arial")
            };

            Paragraph messageString = new Paragraph(new Run(message))
            {
                FontSize = 12,
                FontFamily = new FontFamily("Arial")
            };

            TableRow row = new TableRow();
            TableCell timeCell = new TableCell();
            TableCell messageCell = new TableCell();

            timeCell.Blocks.Add(timeString);
            messageCell.Blocks.Add(messageString);

            row.Cells.Add(timeCell);
            row.Cells.Add(messageCell);

            table.RowGroups.First().Rows.Add(row);

            return true;
        }
    }
}
