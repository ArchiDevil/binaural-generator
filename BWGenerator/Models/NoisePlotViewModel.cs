using System;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace BWGenerator.Models
{
    public class NoisePlotViewModel
    {
        public delegate double SelectNoiseValueDelegate(NoisePoint point);
        public PlotModel model { get; }
        
        private SelectNoiseValueDelegate selector = null;
        private LineSeries currentSerie = null;
        private PresetModel currentPreset = null;

        public NoisePlotViewModel(PresetModel currentPreset, SelectNoiseValueDelegate selector)
        {
            this.selector = selector;
            this.currentPreset = currentPreset;
            model = CreatePlotModel();
        }

        private PlotModel CreatePlotModel()
        {
            var model = new PlotModel();

            // Y-axis (value)
            model.Axes.Add(new LinearAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid
            });

            // X-axis (time)
            model.Axes.Add(new LinearAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                Position = AxisPosition.Bottom,
            });

            var s1 = new LineSeries
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Square,
                MarkerSize = 6,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
                MarkerStrokeThickness = 1.5
            };

            foreach(var point in currentPreset.noisePoints)
                s1.Points.Add(new DataPoint { X = point.Time, Y = selector(point) });

            model.Series.Add(s1);
            s1.MouseDown += MouseDownHandler;
            s1.MouseMove += MouseMoveHandler;
            s1.MouseUp += MouseUpHandler;
            currentSerie = s1;
            return model;
        }

        private int indexOfPointToMove = -1;

        void MouseUpHandler(object sender, OxyMouseEventArgs e)
        {
            indexOfPointToMove = -1;
            model.InvalidatePlot(false);
            e.Handled = true;
        }

        void MouseMoveHandler(object sender, OxyMouseEventArgs e)
        {
            if (indexOfPointToMove == -1)
                return;

            currentSerie.Points[indexOfPointToMove] = currentSerie.InverseTransform(e.Position);
            model.InvalidatePlot(false);
            e.Handled = true;
        }

        void MouseDownHandler(object sender, OxyMouseDownEventArgs e)
        {
            if (e.ChangedButton != OxyMouseButton.Left)
                return;

            int indexOfNearestPoint = (int)Math.Round(e.HitTestResult.Index);
            var nearestPoint = currentSerie.Transform(currentSerie.Points[indexOfNearestPoint]);

            if ((nearestPoint - e.Position).Length < 10)
            {
                indexOfPointToMove = indexOfNearestPoint;
            }
            else
            {
                int i = (int)e.HitTestResult.Index + 1;
                currentSerie.Points.Insert(i, currentSerie.InverseTransform(e.Position));
                indexOfPointToMove = i;
            }

            model.InvalidatePlot(false);
            e.Handled = true;
        }
    }
}
