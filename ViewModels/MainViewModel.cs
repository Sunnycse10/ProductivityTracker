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

namespace ProductivityTracker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ProductivityData _service;
        private DateTime _lastKeyPress = DateTime.MinValue;
        private TimeSpan _treshold = TimeSpan.FromSeconds(30);
        private IKeyboardMouseEvents _globalHook;
        private int _typingMinutes;
        private System.Timers.Timer _timer;
        private string _productivityText;

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
            _service = new ProductivityData();
            _typingMinutes = _service.TypingMinutes;
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
