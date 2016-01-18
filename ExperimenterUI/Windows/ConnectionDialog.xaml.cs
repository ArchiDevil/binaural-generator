using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Windows;

namespace ExperimenterUI.Windows
{
    /// <summary>
    /// Логика взаимодействия для ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            var e = PropertyChanged;
            if (e != null)
                e(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _connectionAddress = "";

        public string ConnectionAddress
        {
            get { return _connectionAddress; }
            set { _connectionAddress = value; RaisePropertyChanged("ConnectionAddress"); }
        }

        private ExperimenterApplicationModel _appModel = null;

        public ConnectionDialog(ExperimenterApplicationModel appModel)
        {
            InitializeComponent();
            DataContext = this;
            Contract.Requires(appModel != null, "appModel mustn't be null");
            _appModel = appModel;
        }

        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            _appModel.Connect(_connectionAddress);
            Close();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
