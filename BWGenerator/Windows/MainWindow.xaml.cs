using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using BWGenerator.Models;

using AudioCore;
using AudioCore.SampleProviders;

using OxyPlot;
using OxyPlot.Wpf;

namespace BWGenerator
{
    public partial class MainWindow : Window
    {
        SignalPlotViewModel[] signalPlotModels = new SignalPlotViewModel[2];
        NoisePlotViewModel noiseSmoothnessModel = null;
        NoisePlotViewModel noiseVolumeModel = null;

        // SIGNAL GETTERS
        public double SignalCarrierGetter(SignalPoint point)
        {
            return point.CarrierValue;
        }

        public double SignalDifferenceGetter(SignalPoint point)
        {
            return point.DifferenceValue;
        }

        public double SignalVolumeGetter(SignalPoint point)
        {
            return point.VolumeValue;
        }

        //SIGNAL SETTERS
        public void SignalCarrierSetter(SignalPoint point, double value)
        {
            point.CarrierValue = value;
        }

        public void SignalDifferenceSetter(SignalPoint point, double value)
        {
            point.DifferenceValue = value;
        }

        public void SignalVolumeSetter(SignalPoint point, double value)
        {
            point.VolumeValue = value;
        }

        // NOISE GETTERS
        public double NoiseSmoothnessGetter(NoisePoint point)
        {
            return point.SmoothnessValue;
        }

        public double NoiseVolumeGetter(NoisePoint point)
        {
            return point.VolumeValue;
        }

        // NOISE SETTERS
        public void NoiseSmoothnessSetter(NoisePoint point, double value)
        {
            point.SmoothnessValue = value;
        }

        public void NoiseVolumeSetter(NoisePoint point, double value)
        {
            point.VolumeValue = value;
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
            NoiseSmoothnessPlot.Controller = PlotsController;
            NoiseVolumePlot.Controller = PlotsController;

            Preset = new PresetModel();

            DataContext = Preset;
            // playback = new Playback(new ModelledSampleProvider());
        }

        public PresetModel Preset { get; set; }
        public PlotController PlotsController { get; set; }
        // private Playback playback = null;

        void SelectSignal(int signalId)
        {
            if (signalId < 0 || Plot1Type.SelectedIndex < 0 || Plot2Type.SelectedIndex < 0)
                return;

            SelectModel((Graphs)Plot1Type.SelectedIndex, Plot1, 0, signalId);
            SelectModel((Graphs)Plot2Type.SelectedIndex, Plot2, 1, signalId);

            //TODO: should be done once =)
            noiseSmoothnessModel = new NoisePlotViewModel(Preset, NoiseSmoothnessGetter, NoiseSmoothnessSetter);
            noiseVolumeModel = new NoisePlotViewModel(Preset, NoiseVolumeGetter, NoiseVolumeSetter);

            NoiseSmoothnessPlot.Model = noiseSmoothnessModel.model;
            NoiseVolumePlot.Model = noiseVolumeModel.model;
        }

        void SelectModel(Graphs type, PlotView plot, int modelIndex, int signalId)
            {
            SignalPlotViewModel newModel = null;

            switch (type)
            {
                case Graphs.Carrier:
                    newModel = new SignalPlotViewModel(Preset.Signals[signalId], SignalCarrierGetter);
                    break;
                case Graphs.Difference:
                    newModel = new SignalPlotViewModel(Preset.Signals[signalId], SignalDifferenceGetter);
                    break;
                case Graphs.Volume:
                    newModel = new SignalPlotViewModel(Preset.Signals[signalId], SignalVolumeGetter);
                    break;
                default:
                    throw new InvalidCastException();
            }

            signalPlotModels[modelIndex] = newModel;
            plot.Model = newModel.model;
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            // playback.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            // playback.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            // playback.Stop();
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
            int signalId = PresetSignalsComboBox.SelectedIndex;
            if (signalId < 0)
                return;

            SelectModel((Graphs)Plot1Type.SelectedIndex, Plot1, 0, signalId);
        }

        private void Plot2Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int signalId = PresetSignalsComboBox.SelectedIndex;
            if (signalId < 0)
                return;

            SelectModel((Graphs)Plot2Type.SelectedIndex, Plot2, 1, signalId);
        }
    }
}
