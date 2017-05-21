using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using BWGenerator.Models;

using OxyPlot;
using OxyPlot.Wpf;

namespace BWGenerator
{
    public partial class MainWindow : Window
    {
        EditablePlotViewModel<SignalDataPoint>[] signalPlotModels = new EditablePlotViewModel<SignalDataPoint>[2];
        EditablePlotViewModel<NoiseDataPoint> noiseSmoothnessModel = null;
        EditablePlotViewModel<NoiseDataPoint> noiseVolumeModel = null;
        OverviewPlotViewModel overviewModel = null;

        public PresetModel Preset { get; set; }
        public PlotController EditablePlotsController { get; set; } = new PlotController();
        public PlotController OverviewPlotController { get; set; } = new PlotController();
        // private Playback playback = null;

        public double SignalCarrierGetter(BaseDataPoint point) => (point as SignalDataPoint).CarrierValue;
        public double SignalDifferenceGetter(BaseDataPoint point) => (point as SignalDataPoint).DifferenceValue;
        public double SignalVolumeGetter(BaseDataPoint point) => (point as SignalDataPoint).VolumeValue;
        public double NoiseSmoothnessGetter(BaseDataPoint point) => (point as NoiseDataPoint).SmoothnessValue;
        public double NoiseVolumeGetter(BaseDataPoint point) => (point as NoiseDataPoint).VolumeValue;

        public void SignalCarrierSetter(BaseDataPoint point, double value)
        {
            (point as SignalDataPoint).CarrierValue = value;
        }

        public void SignalDifferenceSetter(BaseDataPoint point, double value)
        {
            (point as SignalDataPoint).DifferenceValue = value;
        }

        public void SignalVolumeSetter(BaseDataPoint point, double value)
        {
            (point as SignalDataPoint).VolumeValue = value;
        }

        public void NoiseSmoothnessSetter(BaseDataPoint point, double value)
        {
            (point as NoiseDataPoint).SmoothnessValue = value;
        }

        public void NoiseVolumeSetter(BaseDataPoint point, double value)
        {
            (point as NoiseDataPoint).VolumeValue = value;
        }

        public BaseDataPoint SignalDataPointCreator(double time) => new SignalDataPoint { Time = time };
        public BaseDataPoint NoiseDataPointCreator(double time) => new NoiseDataPoint { Time = time };

        public MainWindow()
        {
            InitializeComponent();

            EditablePlotsController.UnbindMouseWheel();
            EditablePlotsController.UnbindMouseDown(OxyMouseButton.Left);
            EditablePlotsController.UnbindMouseDown(OxyMouseButton.Right);

            OverviewPlotController.UnbindMouseDown(OxyMouseButton.Left);

            Plot1.Controller = EditablePlotsController;
            Plot2.Controller = EditablePlotsController;
            NoiseSmoothnessPlot.Controller = EditablePlotsController;
            NoiseVolumePlot.Controller = EditablePlotsController;
            OverviewPlot.Controller = OverviewPlotController;

            Preset = new PresetModel();

            DataContext = Preset;
            // playback = new Playback(new ModelledSampleProvider());

            noiseSmoothnessModel = new EditablePlotViewModel<NoiseDataPoint>(Preset.noisePoints, NoiseSmoothnessGetter, NoiseSmoothnessSetter, NoiseDataPointCreator);
            noiseVolumeModel = new EditablePlotViewModel<NoiseDataPoint>(Preset.noisePoints, NoiseVolumeGetter, NoiseVolumeSetter, NoiseDataPointCreator);

            NoiseSmoothnessPlot.Model = noiseSmoothnessModel.Model;
            noiseSmoothnessModel.PointsUpdated += InvalidateNoisePlots;

            NoiseVolumePlot.Model = noiseVolumeModel.Model;
            noiseVolumeModel.PointsUpdated += InvalidateNoisePlots;

            overviewModel = new OverviewPlotViewModel(Preset, SignalCarrierGetter);
            OverviewPlot.Model = overviewModel.Model;
        }

        void SelectSignal(int signalId)
        {
            if (signalId < 0 || Plot1Type.SelectedIndex < 0 || Plot2Type.SelectedIndex < 0)
                return;

            SelectModel((Graphs)Plot1Type.SelectedIndex, Plot1, 0, signalId);
            SelectModel((Graphs)Plot2Type.SelectedIndex, Plot2, 1, signalId);
        }

        void SelectModel(Graphs type, PlotView plot, int modelIndex, int signalId)
        {
            EditablePlotViewModel<SignalDataPoint> newModel = null;

            if (type == Graphs.Carrier)
                newModel = new EditablePlotViewModel<SignalDataPoint>(Preset.Signals[signalId].points, SignalCarrierGetter, SignalCarrierSetter, SignalDataPointCreator);
            else if (type == Graphs.Difference)
                newModel = new EditablePlotViewModel<SignalDataPoint>(Preset.Signals[signalId].points, SignalDifferenceGetter, SignalDifferenceSetter, SignalDataPointCreator);
            else if (type == Graphs.Volume)
                newModel = new EditablePlotViewModel<SignalDataPoint>(Preset.Signals[signalId].points, SignalVolumeGetter, SignalVolumeSetter, SignalDataPointCreator);
            else
                throw new InvalidCastException();

            if (signalPlotModels[modelIndex] != null)
                signalPlotModels[modelIndex].PointsUpdated -= InvalidateSignalPlots;

            signalPlotModels[modelIndex] = newModel;
            plot.Model = newModel.Model;

            newModel.PointsUpdated += InvalidateSignalPlots;
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
            int signalsCount = Preset.Signals.Count + 1;
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
            InvalidateSignalPlots(this, null);
            InvalidateNoisePlots(this, null);
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

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PresetSignalsComboBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ComboBox target = sender as ComboBox;
            PresetModel value = e.NewValue as PresetModel;
            if (value.Signals.Count > 0)
                target.SelectedIndex = 0;
        }

        private void InvalidateSignalPlots(object sender, EventArgs e)
        {
            foreach (var model in signalPlotModels)
                model.InvalidateGraphs();
            overviewModel.InvalidateGraphs();
        }

        private void InvalidateNoisePlots(object sender, EventArgs e)
        {
            noiseSmoothnessModel.InvalidateGraphs();
            noiseVolumeModel.InvalidateGraphs();
            overviewModel.InvalidateGraphs();
        }
    }
}
