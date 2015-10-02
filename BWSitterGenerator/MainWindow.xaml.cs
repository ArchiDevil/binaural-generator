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

using BWSitterGenerator.Models;

namespace BWSitterGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SignalModel[] signalModels;
        SharedContent.AudioProviders.Playback playback = null;

        public MainWindow()
        {
            InitializeComponent();

            signalModels = new SignalModel[4];
            for (int i = 0; i < 4; ++i)
                signalModels[i] = new SignalModel();

            Channel1.DataContext = signalModels[0];
            Channel2.DataContext = signalModels[1];
            Channel3.DataContext = signalModels[2];
            Channel4.DataContext = signalModels[3];

            playback = new SharedContent.AudioProviders.Playback(new SharedContent.AudioProviders.ConstantSampleProvider(signalModels));
        }

        private void PlayMenu_Click(object sender, RoutedEventArgs e)
        {
            playback.Play();
        }

        private void PauseMenu_Click(object sender, RoutedEventArgs e)
        {
            playback.Pause();
        }

        private void StopMenu_Click(object sender, RoutedEventArgs e)
        {
            playback.Stop();
        }

        private void ResetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var signal in signalModels)
            {
                signal.Enabled = false;
                signal.Difference = 10.0f;
                signal.Frequency = 440.0f;
                signal.Gain = 100.0f;
            }

            signalModels[0].Enabled = true;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
