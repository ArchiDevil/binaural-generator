using System.Windows;

using BWSitterGenerator.Models;

using AudioCore.Layers;

namespace BWSitterGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int signalsCount = 3;
        SignalModel[] signalModels = null;
        NoiseModel noiseModel = null;
        LocalAudioLayer audioLayer = null;

        public MainWindow()
        {
            InitializeComponent();

            signalModels = new SignalModel[signalsCount];
            for (int i = 0; i < signalsCount; ++i)
                signalModels[i] = new SignalModel();

            noiseModel = new NoiseModel();

            Channel1.DataContext = signalModels[0];
            Channel2.DataContext = signalModels[1];
            Channel3.DataContext = signalModels[2];
            NoiseChannel.DataContext = noiseModel;

            audioLayer = new LocalAudioLayer();
            audioLayer.SetSignalSettings(signalModels, noiseModel);

            ResetSignals();
        }

        private void PlayMenu_Click(object sender, RoutedEventArgs e)
        {
            audioLayer.PlaybackEnabled = true;
        }

        private void StopMenu_Click(object sender, RoutedEventArgs e)
        {
            audioLayer.PlaybackEnabled = false;
        }

        private void ResetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ResetSignals();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ResetSignals()
        {
            foreach (var signal in signalModels)
            {
                signal.Enabled = false;
                signal.Difference = 10.0f;
                signal.Frequency = 440.0f;
                signal.Gain = 50.0f;
            }

            signalModels[0].Enabled = true;

            noiseModel.Enabled = false;
            noiseModel.Gain = 50.0f;
            noiseModel.Smoothness = 0.5;
        }
    }
}
