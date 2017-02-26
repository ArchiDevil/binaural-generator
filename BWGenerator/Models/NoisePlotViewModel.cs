using System;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace BWGenerator.Models
{
    public class NoisePlotViewModel
    {
        public delegate double NoiseValueDelegateGetter(NoisePoint point);
        public delegate void NoiseValueDelegateSetter(NoisePoint point, double value);
        public PlotModel Model { get; }

        private NoiseValueDelegateGetter getter = null;
        private NoiseValueDelegateSetter setter = null;
        private LineSeries currentSerie = null;
        private PresetModel currentPreset = null;

        public NoisePlotViewModel(PresetModel currentPreset, NoiseValueDelegateGetter getter, NoiseValueDelegateSetter setter)
        {
            this.getter = getter;
            this.setter = setter;
            this.currentPreset = currentPreset;
            Model = CreatePlotModel();
        }

        private PlotModel CreatePlotModel()
        {
            var model = new PlotModel();

            // Y-axis (value)
            model.Axes.Add(new LinearAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
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
                MarkerStrokeThickness = 1.5,
            };

            foreach (var point in currentPreset.noisePoints)
                s1.Points.Add(new DataPoint(point.Time, getter(point)));

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

            for(int i = 0; i < currentSerie.Points.Count; ++i)
            {
                currentPreset.noisePoints[i].Time = currentSerie.Points[i].X;
                setter(currentPreset.noisePoints[i], currentSerie.Points[i].Y);
            }
        }

        void MouseMoveHandler(object sender, OxyMouseEventArgs e)
        {
            if (indexOfPointToMove == -1)
                return;

            DataPoint modifiedPosition = currentSerie.InverseTransform(e.Position);
            DataPoint currentPoint = currentSerie.Points[indexOfPointToMove];
            if (indexOfPointToMove == 0 || indexOfPointToMove == currentSerie.Points.Count - 1)
            {
                currentSerie.Points[indexOfPointToMove] = new DataPoint(currentPoint.X, modifiedPosition.Y);
            }
            else
            {
                currentSerie.Points[indexOfPointToMove] = modifiedPosition;
            }
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
            //else
            //{
            //    int i = (int)e.HitTestResult.Index + 1;
            //    currentSerie.Points.Insert(i, currentSerie.InverseTransform(e.Position));
            //    indexOfPointToMove = i;
            //}

            Model.InvalidatePlot(false);
            e.Handled = true;
        }
    }
}
