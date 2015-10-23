using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using BWGenerator.Models;
using SharedContent.AudioProviders;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace BWGenerator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            PlotsController = new PlotController();
            PlotsController.UnbindMouseWheel();
            PlotsController.UnbindMouseDown(OxyMouseButton.Left);
            PlotsController.UnbindMouseDown(OxyMouseButton.Right);

            Plot1.Controller = PlotsController;
            Plot2.Controller = PlotsController;

            // creating some default preset
            Preset = new PresetModel();
            Preset.Name = "Some name";
            Preset.Description = "Some description";
            // creating default signal with default params
            Preset.Signals.Add(new PresetModel.Signal { Name = "Signal 1" });
            Preset.Signals[0].SignalPoints.Add(new PresetModel.Signal.SignalPoint { Time = 0.0, DifferenceValue = 2.0, CarrierValue = 440.0 });
            Preset.Signals[0].SignalPoints.Add(new PresetModel.Signal.SignalPoint { Time = 30.0, DifferenceValue = 10.0, CarrierValue = 440.0 });

            DataContext = Preset;
            playback = new Playback(new ModelledSampleProvider());
        }

        public PresetModel Preset { get; set; }
        public PlotController PlotsController { get; set; }
        private Playback playback = null;

        private static PlotModel CreateCarrierModel()
        {
            var model = new PlotModel { LegendSymbolLength = 40 };
            model.Axes.Add(new LinearAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid
            });
            model.Axes.Add(new LinearAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                Position = AxisPosition.Bottom
            });

            // Add a line series
            var s1 = new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Square,
                MarkerSize = 6,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
                MarkerStrokeThickness = 1.5
            };
            s1.Points.Add(new DataPoint(0, 10));
            s1.Points.Add(new DataPoint(10, 40));
            s1.Points.Add(new DataPoint(40, 20));
            s1.Points.Add(new DataPoint(60, 30));
            model.Series.Add(s1);

            int indexOfPointToMove = -1;

            // Subscribe to the mouse down event on the line series
            s1.MouseDown += (s, e) =>
            {
                // only handle the left mouse button (right button can still be used to pan)
                if (e.ChangedButton != OxyMouseButton.Left)
                    return;

                int indexOfNearestPoint = (int)Math.Round(e.HitTestResult.Index);
                var nearestPoint = s1.Transform(s1.Points[indexOfNearestPoint]);

                // Check if we are near a point
                if ((nearestPoint - e.Position).Length < 10)
                {
                    // Start editing this point
                    indexOfPointToMove = indexOfNearestPoint;
                }
                else
                {
                    // otherwise create a point on the current line segment
                    int i = (int)e.HitTestResult.Index + 1;
                    s1.Points.Insert(i, s1.InverseTransform(e.Position));
                    indexOfPointToMove = i;
                }

                // Remember to refresh/invalidate of the plot
                model.InvalidatePlot(false);

                // Set the event arguments to handled - no other handlers will be called.
                e.Handled = true;
            };

            s1.MouseMove += (s, e) =>
            {
                if (indexOfPointToMove == -1)
                    return;

                // Move the point being edited.
                s1.Points[indexOfPointToMove] = s1.InverseTransform(e.Position);
                model.InvalidatePlot(false);
                e.Handled = true;
            };

            s1.MouseUp += (s, e) =>
            {
                // Stop editing
                indexOfPointToMove = -1;
                s1.LineStyle = LineStyle.Solid;
                model.InvalidatePlot(false);
                e.Handled = true;
            };
            return model;
        }

        private void PresetSignalsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            PresetModel.Signal lastSignal = Preset.Signals.Last();
            string lastSignalName = lastSignal.Name;
            int lastIndex = int.Parse(lastSignalName.Substring(lastSignalName.Length - 1));
            lastIndex++;
            Preset.Signals.Add(new PresetModel.Signal { Name = "Signal " + lastIndex.ToString() });

            PresetSignalsComboBox.SelectedIndex = Preset.Signals.Count - 1;
        }

        private void RemoveSignalButton_Click(object sender, RoutedEventArgs e)
        {
            if (Preset.Signals.Count == 1)
                return;

            int selectedSignal = PresetSignalsComboBox.SelectedIndex;
            Preset.Signals.RemoveAt(selectedSignal);
            PresetSignalsComboBox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SignalPropertiesWindow sp = new SignalPropertiesWindow(Preset.Signals.First());
            sp.ShowDialog();
        }

        //private static PlotModel CreateDifferenceModel()
        //{
        //    //PlotBaseModel model = new PlotBaseModel();

        //    //// Add a line series
        //    //var s1 = new LineSeries
        //    //{
        //    //    Color = OxyColors.SkyBlue,
        //    //    MarkerType = MarkerType.Square,
        //    //    MarkerSize = 6,
        //    //    MarkerStroke = OxyColors.White,
        //    //    MarkerFill = OxyColors.SkyBlue,
        //    //    MarkerStrokeThickness = 1.5
        //    //};
        //    //s1.Points.Add(new DataPoint(0.0, 5.0));
        //    //s1.Points.Add(new DataPoint(10.0, 5.0));
        //    //s1.Points.Add(new DataPoint(20.0, 7.0));
        //    //s1.Points.Add(new DataPoint(30.0, 8.0));
        //    //s1.Points.Add(new DataPoint(40.0, 10.0));
        //    //model.Series.Add(s1);

        //    //int indexOfPointToMove = -1;

        //    //// Subscribe to the mouse down event on the line series
        //    //s1.MouseDown += (s, e) =>
        //    //{
        //    //// only handle the left mouse button (right button can still be used to pan)
        //    //if (e.ChangedButton != OxyMouseButton.Left)
        //    //        return;

        //    //    int indexOfNearestPoint = (int)Math.Round(e.HitTestResult.Index);
        //    //    var nearestPoint = s1.Transform(s1.Points[indexOfNearestPoint]);

        //    //// Check if we are near a point
        //    //if ((nearestPoint - e.Position).Length < 10)
        //    //    {
        //    //    // Start editing this point
        //    //    indexOfPointToMove = indexOfNearestPoint;
        //    //    }
        //    //    else
        //    //    {
        //    //    // otherwise create a point on the current line segment
        //    //    int i = (int)e.HitTestResult.Index + 1;
        //    //        s1.Points.Insert(i, s1.InverseTransform(e.Position));
        //    //        indexOfPointToMove = i;
        //    //    }

        //    //// Remember to refresh/invalidate of the plot
        //    //model.InvalidatePlot(false);

        //    //// Set the event arguments to handled - no other handlers will be called.
        //    //e.Handled = true;
        //    //};

        //    //s1.MouseMove += (s, e) =>
        //    //{
        //    //    if (indexOfPointToMove == -1)
        //    //        return;

        //    //// Move the point being edited.
        //    //s1.Points[indexOfPointToMove] = s1.InverseTransform(e.Position);
        //    //    model.InvalidatePlot(false);
        //    //    e.Handled = true;
        //    //};

        //    //s1.MouseUp += (s, e) =>
        //    //{
        //    //// Stop editing
        //    //indexOfPointToMove = -1;
        //    //    s1.LineStyle = LineStyle.Solid;
        //    //    model.InvalidatePlot(false);
        //    //    e.Handled = true;
        //    //};
        //    //return model;
        //}
    }
}
