using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace WinRtMap.Demo.Models
{
    public class DemoModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Point _movingTarget;
        private DispatcherTimer _timer;

        public DemoModel()
        {
            Cities = new Collection<CityMarker>();
            Cities.Add(new CityMarker {Location = new Point(48.8567, 2.3508), Label = "Paris"});
            Cities.Add(new CityMarker {Location = new Point(47.4000, 8.0500), Label = "Aarau"});
            Cities.Add(new CityMarker {Location = new Point(41.9000, 12.5000), Label = "Rome"});
            Cities.Add(new CityMarker {Location = new Point(52.5167, 13.3833), Label = "Berlin"});

            Peaks = new Collection<PeakMarker>();
            Peaks.Add(new PeakMarker { PeakLocation = new Point(42.6322, 0.6578), PeakName = "Aneto" });
            Peaks.Add(new PeakMarker { PeakLocation = new Point(46.4870, 9.5617), PeakName = "Piz Platta" });
            Peaks.Add(new PeakMarker { PeakLocation = new Point(40.8167, 14.4333), PeakName = "Monte Vesuvio" });
            Peaks.Add(new PeakMarker { PeakLocation = new Point(37.05, -3.3167), PeakName = "Pico de Mulhacén" });

            MovingTarget = new Point(47, 15);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.01);
            _timer.Tick += UpdateMovingTarget;
            _timer.Start();
        }

        public ICollection<CityMarker> Cities { get; set; }

        public ICollection<PeakMarker> Peaks { get; set; }

        public Point MovingTarget
        {
            get { return _movingTarget; }
            set
            {
                _movingTarget = value;
                OnPropertyChanged();
            }
        }

        private void UpdateMovingTarget(object sender, object e)
        {
            double y = MovingTarget.Y + 0.01;
            if (y > 180)
            {
                y = -180;
            }
            MovingTarget = new Point(MovingTarget.X, y);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}