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

namespace ExperimenterUI.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable
    {
        ExperimenterApplicationModel model = new ExperimenterApplicationModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = model;
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
    }
}
