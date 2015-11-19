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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e == null)
                return;

            if (e.Key == Key.Enter)
            {
                Paragraph timeString = new Paragraph(new Run(DateTime.Now.ToLongTimeString()));
                Paragraph messageString = new Paragraph(new Run(chatType.Text));

                timeString.FontSize = 11;
                timeString.FontFamily = new FontFamily("Arial");

                messageString.FontSize = 12;
                messageString.FontFamily = new FontFamily("Arial");

                TableRow row = new TableRow();
                TableCell timeCell = new TableCell();
                TableCell messageCell = new TableCell();

                timeCell.Blocks.Add(timeString);
                messageCell.Blocks.Add(messageString);

                row.Cells.Add(timeCell);
                row.Cells.Add(messageCell);

                table.RowGroups.First().Rows.Add(row);
                chatType.Text = "";
                e.Handled = true;
            }
        }
    }
}
