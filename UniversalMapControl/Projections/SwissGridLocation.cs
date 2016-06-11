// The MIT License (MIT)
// 
// Copyright (c) 2014 Federal Office of Topography swisstopo, Wabern, CH
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// 


// Source: http://www.swisstopo.admin.ch/internet/swisstopo/en/home/topics/survey/sys/refsys/projections.html (see PDFs under "Documentation")
// Updated 9 dec 2014
// Please validate your results with NAVREF on-line service: http://www.swisstopo.admin.ch/internet/swisstopo/en/home/apps/calc/navref.html (difference ~ 1-2m)

using System;
using System.Globalization;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Projections
{
	public class SwissGridLocation : ILocation
	{
		public double X { get; private set; }
		public double Y { get; private set; }

		public SwissGridLocation() : this(600000, 200000)
		{
		}

		public SwissGridLocation(int x, int y)
		{
			X = x;
			Y = y;
		}

		public SwissGridLocation(double x, double y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0} // {1}", X, Y);
		}

		public ILocation ToWgs84Approx()
		{
			Wgs84Location location = new Wgs84Location(CHtoWGSlat(X, Y), CHtoWGSlng(X, Y));
			return location;
		}

		public static SwissGridLocation FromWgs84Approx(ILocation location)
		{
			SwissGridLocation sg = new SwissGridLocation();
			sg.X = WGStoCHy(location.Latitude, location.Longitude);
			sg.Y = WGStoCHx(location.Latitude, location.Longitude);
			return sg;
		}

		// Convert WGS lat/long (° dec) to CH y
		private static double WGStoCHy(double lat, double lng)
		{
			// Convert decimal degrees to sexagesimal seconds
			lat = DecToSexAngle(lat);
			lng = DecToSexAngle(lng);

			// Auxiliary values (% Bern)
			double latAux = (lat - 169028.66) / 10000.0;
			double lngAux = (lng - 26782.5) / 10000.0;

			// Process Y
			double y = 600072.37
			           + 211455.93 * lngAux
			           - 10938.51 * lngAux * latAux
			           - 0.36 * lngAux * Math.Pow(latAux, 2)
			           - 44.54 * Math.Pow(lngAux, 3);

			return y;
		}

		// Convert WGS lat/long (° dec) to CH x
		private static double WGStoCHx(double lat, double lng)
		{
			// Convert decimal degrees to sexagesimal seconds
			lat = DecToSexAngle(lat);
			lng = DecToSexAngle(lng);

			// Auxiliary values (% Bern)
			double latAux = (lat - 169028.66) / 10000.0;
			double lngAux = (lng - 26782.5) / 10000.0;

			// Process X
			double x = 200147.07
			           + 308807.95 * latAux
			           + 3745.25 * Math.Pow(lngAux, 2)
			           + 76.63 * Math.Pow(latAux, 2)
			           - 194.56 * Math.Pow(lngAux, 2) * latAux
			           + 119.79 * Math.Pow(latAux, 3);

			return x;
		}

		// Convert WGS lat/long (° dec) and height to CH h
		private static double WGStoCHh(double lat, double lng, double h)
		{
			// Convert decimal degrees to sexagesimal seconds
			lat = DecToSexAngle(lat);
			lng = DecToSexAngle(lng);

			// Auxiliary values (% Bern)
			double latAux = (lat - 169028.66) / 10000.0;
			double lngAux = (lng - 26782.5) / 10000.0;

			// Process h
			h = h - 49.55
			    + 2.73 * lngAux
			    + 6.94 * latAux;

			return h;
		}

		// Convert CH y/x to WGS lat
		private static double CHtoWGSlat(double y, double x)
		{
			// Converts military to civil and  to unit = 1000km
			// Auxiliary values (% Bern)
			double yAux = (y - 600000.0) / 1000000.0;
			double xAux = (x - 200000.0) / 1000000.0;

			// Process lat
			double lat = 16.9023892
			             + 3.238272 * xAux
			             - 0.270978 * Math.Pow(yAux, 2)
			             - 0.002528 * Math.Pow(xAux, 2)
			             - 0.0447 * Math.Pow(yAux, 2) * xAux
			             - 0.0140 * Math.Pow(xAux, 3);

			// Unit 10000" to 1 " and converts seconds to degrees (dec)
			lat = lat * 100 / 36;

			return lat;
		}

		// Convert CH y/x to WGS long
		private static double CHtoWGSlng(double y, double x)
		{
			// Converts military to civil and  to unit = 1000km
			// Auxiliary values (% Bern)
			double yAux = (y - 600000.0) / 1000000.0;
			double xAux = (x - 200000.0) / 1000000.0;

			// Process long
			double lng = 2.6779094
			             + 4.728982 * yAux
			             + 0.791484 * yAux * xAux
			             + 0.1306 * yAux * Math.Pow(xAux, 2)
			             - 0.0436 * Math.Pow(yAux, 3);

			// Unit 10000" to 1 " and converts seconds to degrees (dec)
			lng = lng * 100.0 / 36.0;

			return lng;
		}

		// Convert CH y/x/h to WGS height
		private static double CHtoWGSheight(double y, double x, double h)
		{
			// Converts military to civil and  to unit = 1000km
			// Auxiliary values (% Bern)
			double yAux = (y - 600000.0) / 1000000.0;
			double xAux = (x - 200000.0) / 1000000.0;

			// Process height
			h = h + 49.55 - 12.60 * yAux - 22.64 * xAux;

			return h;
		}

		// Convert angle in decimal degrees to sexagesimal seconds
		public static double DecToSexAngle(double dec)
		{
			int deg = (int)Math.Floor(dec);
			int min = (int)Math.Floor((dec - deg) * 60.0);
			double sec = ((dec - deg) * 60.0 - min) * 60.0;

			return sec + min * 60.0 + deg * 3600.0;
		}

		double ILocation.Latitude
		{
			get { return Y; }
		}

		double ILocation.Longitude
		{
			get { return X; }
		}

		ILocation ILocation.ChangeLatitude(double newLatitude)
		{
			return new SwissGridLocation(X, newLatitude);
		}

		ILocation ILocation.ChangeLongitude(double newLongitude)
		{
			return new SwissGridLocation(newLongitude, Y);
		}
	}
}