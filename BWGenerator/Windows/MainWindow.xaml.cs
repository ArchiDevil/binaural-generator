using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using BWGenerator.Models;

using SharedLibrary.AudioProviders;

using OxyPlot;

namespace BWGenerator
{
    public partial class MainWindow : Window
    {
        SignalPlotViewModel[] models = new SignalPlotViewModel[2];
        NoisePlotViewModel noiseSmoothnessModel = null;
        NoisePlotViewModel noiseVolumeModel = null;

        public double CarrierSelector(SignalPoint point)
        {
            return point.CarrierValue;
        }

        public double DifferenceSelector(SignalPoint point)
        {
            return point.DifferenceValue;
        }

        public double VolumeSelector(SignalPoint point)
        {
            return point.VolumeValue;
        }

        public double NoiseSmoothnessSelector(NoisePoint point)
        {
            return point.SmoothnessValue;
        }

        public double NoiseVolumeSelector(NoisePoint point)
        {
            return point.VolumeValue;
        }

        public MainWindow()
        {
            InitializeComponent();

            PlotsController = new PlotController();
            PlotsController.UnbindMouseWheel();
            PlotsController.UnbindMouseDown(OxyMouseButton.Left);
            PlotsController.UnbindMouseDown(OxyMouseButton.Right);

            Plot1.Controller = PlotsController;
            Plot2.Controller = PlotsController;

            Preset = new PresetModel();
            Preset.Name = "New preset";
            Preset.Description = "Write your description here";
            Preset.Signals.Add(new Signal { Name = "Signal 0" });

            DataContext = Preset;
            playback = new Playback(new ModelledSampleProvider());
        }

        public PresetModel Preset { get; set; }
        public PlotController PlotsController { get; set; }
        private Playback playback = null;

        private void SelectSignal(int signalId)
        {
            if (signalId < 0 || Plot1Type.SelectedIndex < 0 || Plot2Type.SelectedIndex < 0)
                return;

            switch ((Graphs)Plot1Type.SelectedIndex)
            {
                case Graphs.Carrier:
                    models[0] = new SignalPlotViewModel(Preset.Signals[signalId], CarrierSelector);
                    break;
                case Graphs.Difference:
                    models[0] = new SignalPlotViewModel(Preset.Signals[signalId], DifferenceSelector);
                    break;
                case Graphs.Volume:
                    models[0] = new SignalPlotViewModel(Preset.Signals[signalId], VolumeSelector);
                    break;
                default:
                    throw new InvalidCastException();
            }

            switch ((Graphs)Plot2Type.SelectedIndex)
            {
                case Graphs.Carrier:
                    models[1] = new SignalPlotViewModel(Preset.Signals[signalId], CarrierSelector);
                    break;
                case Graphs.Difference:
                    models[1] = new SignalPlotViewModel(Preset.Signals[signalId], DifferenceSelector);
                    break;
                case Graphs.Volume:
                    models[1] = new SignalPlotViewModel(Preset.Signals[signalId], VolumeSelector);
                    break;
                default:
                    throw new InvalidCastException();
            }
            Plot1.Model = models[0].model;
            Plot2.Model = models[1].model;

            noiseSmoothnessModel = new NoisePlotViewModel(Preset, NoiseSmoothnessSelector);
            noiseVolumeModel = new NoisePlotViewModel(Preset, NoiseVolumeSelector);
            NoiseSmth.Model = noiseSmoothnessModel.model;
            NoiseVol.Model = noiseVolumeModel.model;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            playback.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            playback.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            playback.Stop();
        }

        private void AddSignalButton_Click(object sender, RoutedEventArgs e)
        {
            int signalsCount = Preset.Signals.Count();
            Preset.Signals.Add(new Signal { Name = "Signal " + signalsCount.ToString() });
            PresetSignalsComboBox.SelectedIndex = Preset.Signals.Count - 1;
        }

        private void RemoveSignalButton_Click(object sender, RoutedEventArgs e)
        {
            if (Preset.Signals.Count == 1)
                return;

            Preset.Signals.RemoveAt(PresetSignalsComboBox.SelectedIndex);
            if (Preset.Signals.Count() < PresetSignalsComboBox.SelectedIndex)
                PresetSignalsComboBox.SelectedIndex = Preset.Signals.Count() - 1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = PresetSignalsComboBox.SelectedIndex;
            if (selectedIndex < 0)
                return;

            SignalPropertiesWindow window = new SignalPropertiesWindow(Preset.Signals[selectedIndex]);
            window.ShowDialog();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PresetSignalsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectSignal(PresetSignalsComboBox.SelectedIndex);
        }

        private void Plot1Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = PresetSignalsComboBox.SelectedIndex;
            if (selectedIndex < 0)
                return;

            SignalPlotViewModel modelToCheck = models[0];

            switch ((Graphs)Plot1Type.SelectedIndex)
            {
                case Graphs.Carrier:
                    modelToCheck = new SignalPlotViewModel(Preset.Signals[selectedIndex], CarrierSelector);
                    break;
                case Graphs.Difference:
                    modelToCheck = new SignalPlotViewModel(Preset.Signals[selectedIndex], DifferenceSelector);
                    break;
                case Graphs.Volume:
                    modelToCheck = new SignalPlotViewModel(Preset.Signals[selectedIndex], VolumeSelector);
                    break;
                default:
                    throw new InvalidCastException();
            }

            Plot1.Model = modelToCheck.model;
        }

        private void Plot2Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = PresetSignalsComboBox.SelectedIndex;
            if (selectedIndex < 0)
                return;

            SignalPlotViewModel modelToCheck = models[1];

            switch ((Graphs)Plot2Type.SelectedIndex)
            {
                case Graphs.Carrier:
                    modelToCheck = new SignalPlotViewModel(Preset.Signals[selectedIndex], CarrierSelector);
                    break;
                case Graphs.Difference:
                    modelToCheck = new SignalPlotViewModel(Preset.Signals[selectedIndex], DifferenceSelector);
                    break;
                case Graphs.Volume:
                    modelToCheck = new SignalPlotViewModel(Preset.Signals[selectedIndex], VolumeSelector);
                    break;
                default:
                    throw new InvalidCastException();
            }

            Plot2.Model = modelToCheck.model;
        }
    }
}
