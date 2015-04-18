using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Windows.Foundation;

namespace WinRtMap.Tiles
{
	public class WebTileLayer : BaseTileLayer<WebTile>
	{
		private static readonly Regex PatternMatcher = new Regex("{(?<key>[a-z-;]+)}", RegexOptions.IgnoreCase);
		private Random _random = new Random();

		public string UrlPattern { get; set; }

		protected override WebTile CreateNewTile(int x, int z, int y, Point location)
		{
			Uri uri = new Uri(PatternMatcher.Replace(UrlPattern, m => MatchEvaluator(m, x, y, z)));
			return new WebTile(x, y, z, location, uri);
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

		public WebTileLayer() : base(new WebTileLoader())
		{
			UrlPattern = "http://{RND-a;b;c}.tile.openstreetmap.org/{z}/{x}/{y}.png";

		}
	}
}