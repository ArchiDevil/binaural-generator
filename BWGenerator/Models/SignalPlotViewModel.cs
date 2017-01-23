using System;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace BWGenerator.Models
{
    public class SignalPlotViewModel
    {
        public delegate double SelectValueDelegate(SignalPoint point);

        public PlotModel Model { get; }

        private SelectValueDelegate selector = null;
        private Signal signal = null;
        private LineSeries currentSerie = null;

        public SignalPlotViewModel(Signal signal, SelectValueDelegate selector)
        {
            this.selector = selector;
            this.signal = signal;
            Model = CreatePlotModel();
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

            foreach(var point in signal.points)
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
            Model.InvalidatePlot(false);
            e.Handled = true;
        }

        void MouseMoveHandler(object sender, OxyMouseEventArgs e)
        {
            if (indexOfPointToMove == -1)
                return;

            currentSerie.Points[indexOfPointToMove] = currentSerie.InverseTransform(e.Position);
            Model.InvalidatePlot(false);
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

            Model.InvalidatePlot(false);
            e.Handled = true;
        }
    }
}
