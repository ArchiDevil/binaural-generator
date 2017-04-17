using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ExperimenterUI.Windows
{
    /// <summary>
    /// Логика взаимодействия для ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _connectionAddress = "";
        private ExperimenterApplicationModel _appModel = null;

        public string ConnectionAddress
        {
            get { return _connectionAddress; }
            set { _connectionAddress = value; RaisePropertyChanged(); }
        }

        public ConnectionDialog(ExperimenterApplicationModel appModel)
        {
            InitializeComponent();
            DataContext = this;
            _appModel = appModel ?? throw new ArgumentException("appModel must not be null");
        }

        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            _appModel.ConnectAsync(_connectionAddress);
            Close();
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
