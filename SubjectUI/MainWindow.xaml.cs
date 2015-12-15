﻿using System;
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
    public sealed partial class MainWindow : Window, IDisposable
    {
        SubjectApplicationModel model = new SubjectApplicationModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = model;
            model.ChatMessageReceivedEvent += Model_ChatMessageReceivedEvent;
        }

        private void Model_ChatMessageReceivedEvent(string message, DateTime time)
        {
            chatWindow.PushChatMessage(message, time);
        }

        public void Dispose()
        {
            model.Dispose();
        }

        private void ChatWindow_ChatMessage(string message, DateTime time)
        {
            model.SendChatMessage(message, time);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            model.CheckSystems();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            model.Dispose();
            model = null;
        }
    }
}
