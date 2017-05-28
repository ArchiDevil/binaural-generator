using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using SharedLibrary.Code;

namespace BWGenerator.Models
{
    public sealed class OverviewPlotViewModel
    {
        public delegate double ValueGetterDelegate(BaseDataPoint point);
        private ValueGetterDelegate getter = null;

        public PlotModel Model { get; private set; }
        PresetModel preset = null;

        public OverviewPlotViewModel(PresetModel preset, ValueGetterDelegate getter)
        {
            this.getter = getter;
            this.preset = preset;
            CreatePlotModel();
        }

        public void InvalidateGraphs()
        {
            CreateSeries();

            Model.InvalidatePlot(true);
            Model.ResetAllAxes();
        }

        private void CreateSeries()
        {
            Model.Series.Clear();

            OxyColor[] colors = new OxyColor[]
            {
                OxyColors.MediumVioletRed,
                OxyColors.Teal,
                OxyColors.Olive,
            };

            int index = 0;

            foreach (var serie in preset.Signals)
            {
                var lineSerie = new LineSeries
                {
                    Color = colors[index % colors.Length],
                    MarkerType = MarkerType.Triangle,
                    MarkerSize = 6,
                    MarkerStroke = OxyColors.Teal,
                    MarkerFill = OxyColors.SkyBlue,
                    MarkerStrokeThickness = 1
                };

                foreach (var point in serie.points)
                {
                    lineSerie.Points.Add(new DataPoint(point.Time, getter(point)));
                }

                Model.Series.Add(lineSerie);
                ++index;
            }
        }

        private void CreatePlotModel()
        {
            Model = new PlotModel();

            // Y-axis (value)
            Model.Axes.Add(new LinearAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
            });

            // X-axis (time)
            Model.Axes.Add(new LinearAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                Position = AxisPosition.Bottom,
            });

            CreateSeries();
        }
    }
}
