using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace UniversalMapControl.Tiles
{
	public class WebTileLayer : TileLayer<WebTile>
	{
		private static readonly Regex PatternMatcher = new Regex("{(?<key>[^}]+)}", RegexOptions.IgnoreCase);
		private Random _random = new Random();

		public WebTileLayer() : base(new WebTileLoader())
		{
			UrlPattern = "http://{RND-a;b;c}.tile.openstreetmap.org/{z}/{x}/{y}.png";
			LayerName = "OSM";
		}

		/// <summary>
		/// URL Patter to load the Tile from. The following Patter are supported:
		/// {x}/{y}/{z} : Coordinates
		/// {RND-a;b;c} : Randomly picks one of the supplied values (separated by semicolon)
		/// </summary>
		public string UrlPattern { get; set; }

		/// <summary>
		/// Name of the Layer. This is used to create a unique folder for the Filesystem Cache.
		/// </summary>
		public string LayerName { get; set; }

		protected override WebTile CreateNewTile(int x, int z, int y, Location location)
		{
			string uriString = PatternMatcher.Replace(UrlPattern, m => MatchEvaluator(m, x, y, z));
			Uri uri = new Uri(uriString);
			return new WebTile(x, y, z, location, uri, LayerName, Canvas);
		}

		private string MatchEvaluator(Match match, int x, int y, int z)
		{
			Group keyGroup = match.Groups["key"];
			if (keyGroup == null || string.IsNullOrWhiteSpace(keyGroup.Value))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The URL-Pattern '{0}' is invalid.", match.Value));
			}
			string key = keyGroup.Value.ToLowerInvariant();
			if (key == "x")
			{
				return x.ToString(CultureInfo.InvariantCulture);
			}
			if (key == "y")
			{
				return y.ToString(CultureInfo.InvariantCulture);
			}
			if (key == "z")
			{
				return z.ToString(CultureInfo.InvariantCulture);
			}
			if (key.StartsWith("rnd-"))
			{
				string[] parts = key.Substring(4).Split(';');
				if (parts.Length <= 0)
				{
					throw new InvalidOperationException("The Random-Pattern must have at least one value ({{RND-a}}).");
				}
				int index = _random.Next(parts.Length);
				return parts[index];
			}
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The URL-Pattern '{0}' is invalid.", key));
		}
	}
}