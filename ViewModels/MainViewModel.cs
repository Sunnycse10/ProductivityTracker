using Gma.System.MouseKeyHook;
using ProductivityTracker.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Series;
using System.Drawing;
using OxyPlot.Axes;

namespace ProductivityTracker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ProductivityDataModel _service;
        private DateTime _lastKeyPress = DateTime.MinValue;
        private TimeSpan _treshold = TimeSpan.FromSeconds(30);
        private IKeyboardMouseEvents _globalHook;
        private int _typingMinutes;
        private System.Timers.Timer _timer;
        private string _productivityText;
        public Dictionary<string, int> PData = new();
        public IList<int> points {  get; set; }
        public IList<string> categories { get; set; }
        private PlotModel _productivityPlot;

        public void DrawProductivityPlot()
        {

            PData = _service.ProductivityData;
            var pData7Days = PData
                .OrderByDescending(kvp => DateTime.ParseExact(kvp.Key, "dd/MM/yyyy", null))
                .Take(7)
                .OrderBy(kvp => DateTime.ParseExact(kvp.Key, "dd/MM/yyyy", null))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            points = pData7Days.Values.ToList();
            categories = pData7Days.Keys.ToList();
            _productivityPlot = new PlotModel { Title = "Typing duration of last 7 days" };
            var barSeries = new List<BarItem>();
            foreach (var p in points)
            {
                barSeries.Add(new BarItem { Value = p });
            }
            _productivityPlot.Series.Add(new BarSeries { ItemsSource = barSeries, LabelPlacement = LabelPlacement.Outside , LabelFormatString= "{00}min" });
            _productivityPlot.Axes.Add(new CategoryAxis { Position = AxisPosition.Left, ItemsSource = categories });

        }

        public PlotModel ProductivityPlot
        {
            get {
                DrawProductivityPlot();
                return _productivityPlot; }
        }

        public string ProductivityText
        {
            get
            {
                if (TypingMinutes < 60)
                {
                    _productivityText = $"Total duration of typing today is {TypingMinutes} minutes";
                }
                else
                {
                    _productivityText = $"Total duration of typing today is {TypingMinutes / 60} hour {TypingMinutes % 60} minutes";
                }
                return _productivityText;
            }

        }
        public MainViewModel()
        {
            _service = new ProductivityDataModel();
            TypingMinutes = _service.TypingMinutes;
            DrawProductivityPlot();
            StartTimer();
            Subscribe();

        }

        public int TypingMinutes
        {
            get
            {
                return _typingMinutes;
            }
            set
            {
                if (_typingMinutes == value) return;
                _typingMinutes = value;
                _service.TypingMinutes = _typingMinutes;
                OnPropertyChanged(nameof(TypingMinutes));
                OnPropertyChanged(nameof(ProductivityText));
                OnPropertyChanged(nameof(ProductivityPlot));
            }
        }

        private void Subscribe()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHookOnKeyDown;
        }

        private void GlobalHookOnKeyDown(object? sender, KeyEventArgs e)
        {
            _lastKeyPress = DateTime.Now;
            if (e.KeyCode == Keys.Escape)
            {
                Unsubscribe();
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void Unsubscribe()
        {
            _timer.Stop();
            _timer.Dispose();
            _service.SaveData();
            _globalHook.KeyDown -= GlobalHookOnKeyDown;
            _globalHook.Dispose();

        }
        private void StartTimer()
        {
            _timer = new System.Timers.Timer(60000);
            _timer.Elapsed += (s, e) =>
            {
                if ((DateTime.Now - _lastKeyPress) < _treshold)
                {
                    TypingMinutes++;
                }
            };
            _timer.Start();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

    }
}
