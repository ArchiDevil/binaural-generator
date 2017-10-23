using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using SharedLibrary.Code;

namespace BWGenerator.Models
{
    public sealed class OverviewPlotViewModel
    {
        public delegate double ValueGetterDelegate(SignalDataPoint point);
        private ValueGetterDelegate getter = null;

        public PlotModel Model { get; }
        PresetModel preset = null;

        public OverviewPlotViewModel(PresetModel preset, ValueGetterDelegate getter)
        {
            this.getter = getter;
            this.preset = preset;
            Model = CreatePlotModel(preset, getter);
        }

        public void InvalidateGraphs()
        {
            CreateSeries(Model, preset, getter);

            Model.InvalidatePlot(true);
            Model.ResetAllAxes();
        }

        private static void CreateSeries(PlotModel model, PresetModel preset, ValueGetterDelegate getter)
        {
            model.Series.Clear();

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

                foreach (SignalDataPoint point in serie.points)
                {
                    lineSerie.Points.Add(new DataPoint(point.Time, getter(point)));
                }

                model.Series.Add(lineSerie);
                ++index;
            }
        }

        private static PlotModel CreatePlotModel(PresetModel preset, ValueGetterDelegate getter)
        {
            PlotModel model = new PlotModel();

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

            CreateSeries(model, preset, getter);

            return model;
        }
    }
}
