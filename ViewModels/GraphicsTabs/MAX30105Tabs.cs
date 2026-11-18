using Microsoft.VisualBasic.Devices;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using SuperVisionObjectDesktop.Devices;
using SuperVisionObjectDesktop.Network.DataClasses;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SuperVisionObjectDesktop.ViewModels.GraphicsTabs
{
    public class MAX30105Tabs : UserControl
    {
        private PlotView _plotView;
        private ListBox _logs;
        private SplitContainer _split;
        private PlotModel _model;

        IDevice _dataSource;

        public MAX30105Tabs(IDevice device)
        {
            _dataSource = device;
            if (_dataSource != null)
                _dataSource.DataReceived += DataReceived;
            BuildLayout();
            InitializePlot();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dataSource != null)
                    _dataSource.DataReceived -= DataReceived;
            }
            base.Dispose(disposing);
        }

        private readonly object _lock = new object();

        private void DataReceived(object? sender, SensorDataEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => DataReceived(sender, e)));
                return;
            }

            lock (_lock)
            {
                var series = (LineSeries)_model.Series[0];

                // 1. Единый формат X: если ось LinearAxis — работаем в секундах.
                double baseX = series.Points.Count == 0
                    ? 0
                    : series.Points[^1].X + 1;

                double dt = 1;

                for (int i = 0; i < e.Data[0].Count; i++)
                {
                    double y = e.Data[0][i];
                    if (double.IsNaN(y) || double.IsInfinity(y))
                        continue;

                    series.Points.Add(new DataPoint(baseX + i * dt, y));
                }

                const int maxPoints = 3000;
                int excess = series.Points.Count - maxPoints;
                if (excess > 0)
                    series.Points.RemoveRange(0, excess);

                // 2. Не трогаем Minimum/Maximum вручную — пусть ось сама подстраивается.
                _model.InvalidatePlot(true);
            }
        }

        private void BuildLayout()
        {
            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.Panel2,   // логам — фиксированная ширина
                Panel1MinSize = 200,
                Panel2MinSize = 120,
            };

            _plotView = new PlotView { Dock = DockStyle.Fill };
            _logs = new ListBox { Dock = DockStyle.Fill };

            _split.Panel1.Controls.Add(_plotView);
            _split.Panel2.Controls.Add(_logs);
            Controls.Add(_split);

            Load += (s, e) =>
            {
                try { _split.SplitterDistance = (int)(Width * 0.7); } catch { }
            };
        }

        private void InitializePlot()
        {
            _model = new PlotModel { Title = "Данные" };
            _model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = "X"
            });
            _model.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                Title = "Y"
            });

            var series = new LineSeries { Title = "Сигнал" };
            _model.Series.Add(series);

            // ВАЖНО: Model присваивается здесь, а не через конструктор с параметрами
            _plotView.Model = _model;
        }

        // Публичный API — то, ради чего вообще делают UserControl
        public void AppendLog(string message)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => AppendLog(message))); return; }
            _logs.Items.Add($"{DateTime.Now:HH:mm:ss}  {message}");
            _logs.TopIndex = _logs.Items.Count - 1;
        }

        public void ClearLogs() => _logs.Items.Clear();

    }
}
