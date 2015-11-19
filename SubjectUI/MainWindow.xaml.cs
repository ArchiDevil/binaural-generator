using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SubjectUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SubjectUIModel model = new SubjectUIModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = model;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e == null || e.Key != Key.Enter)
                return;

            string message = chatType.Text;
            DateTime messageTime = DateTime.Now;

            // does it need to be checked?
            model.SendChatMessage(message, messageTime);
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
            
            e.Handled = true;
        }
    }
}
