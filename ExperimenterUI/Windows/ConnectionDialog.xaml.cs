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
using System.Windows.Shapes;

namespace ExperimenterUI.Windows
{
    /// <summary>
    /// Логика взаимодействия для ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        public string ConnectionAddress { get; set; } = "localhost";

        ExperimenterApplicationModel appModel = null;

        public ConnectionDialog(ExperimenterApplicationModel appModel)
        {
            InitializeComponent();
            if (appModel == null)
                throw new ArgumentNullException("appModel");

            this.appModel = appModel;
        }

        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            appModel.Connect(ConnectionAddress);
            Close();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
