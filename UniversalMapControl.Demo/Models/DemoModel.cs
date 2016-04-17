using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace UniversalMapControl.Demo.Models
{
    public class DemoModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Point _movingTarget;
        private DispatcherTimer _timer;
	    private Location _mouseCoordinates;

	    public DemoModel()
        {
            Cities = new Collection<CityMarker>();
            Cities.Add(new CityMarker {Location = new Location(48.8567, 2.3508), Label = "Paris"});
            Cities.Add(new CityMarker {Location = new Location(47.4000, 8.0500), Label = "Aarau"});
            Cities.Add(new CityMarker {Location = new Location(41.9000, 12.5000), Label = "Rome"});
            Cities.Add(new CityMarker {Location = new Location(52.5167, 13.3833), Label = "Berlin"});

            Peaks = new Collection<PeakMarker>();
            Peaks.Add(new PeakMarker { PeakLocation = new Location(42.6322, 0.6578), PeakName = "Aneto" });
            Peaks.Add(new PeakMarker { PeakLocation = new Location(46.4870, 9.5617), PeakName = "Piz Platta" });
            Peaks.Add(new PeakMarker { PeakLocation = new Location(40.8167, 14.4333), PeakName = "Monte Vesuvio" });
            Peaks.Add(new PeakMarker { PeakLocation = new Location(37.05, -3.3167), PeakName = "Pico de Mulhacén" });

            MovingTarget = new Location(47, 15);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(0.01);
            _timer.Tick += UpdateMovingTarget;
            _timer.Start();
        }

        public ICollection<CityMarker> Cities { get; set; }

        public ICollection<PeakMarker> Peaks { get; set; }

        public Location MovingTarget
        {
            get { return _movingTarget; }
            set
            {
                _movingTarget = value;
                OnPropertyChanged();
            }
        }

	    public Location MouseCoordinates
	    {
		    get { return _mouseCoordinates; }
		    set
		    {
			    _mouseCoordinates = value;
				OnPropertyChanged();
		    }
	    }

	    public Location MapCenter
	    {
		    get { return new Location(47, 15); }
	    }



	    private void UpdateMovingTarget(object sender, object e)
        {
            double lon = MovingTarget.Longitude + 0.01;
            if (lon > 180)
            {
                lon = -180;
            }
            MovingTarget = new Location(MovingTarget.Latitude, lon);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}