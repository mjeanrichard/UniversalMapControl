using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

using UniversalMapControl.Interfaces;

namespace UniversalMapControl.Tiles
{
	public class WebTileLoader : BaseTileLoader
	{
		private static readonly Regex PatternMatcher = new Regex("{(?<key>[^}]+)}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private readonly HttpClient _client;
		private readonly Random _random = new Random();

		public WebTileLoader(ILayerConfiguration layerConfiguration) : base(layerConfiguration)
		{
			_client = new HttpClient();
		}

		protected override async Task<InMemoryRandomAccessStream> LoadTileImage(ICanvasBitmapTile tile)
		{
			if (DesignMode.DesignModeEnabled)
			{
				return await CreateEmptyImage();
			}
			using (HttpRequestMessage tileRequest = BuildRequest(tile))
			{
				using (HttpResponseMessage response = await _client.SendRequestAsync(tileRequest))
				{
					if (!response.IsSuccessStatusCode)
					{
						return null;
					}
					if (response.StatusCode == HttpStatusCode.NoContent)
					{
						tile.IsCachable = false;
						return await CreateEmptyImage();
					}
					InMemoryRandomAccessStream ras = new InMemoryRandomAccessStream();
					await response.Content.WriteToStreamAsync(ras);
					ras.Seek(0);
					tile.IsCachable = false;
					return ras;
				}
			}
		}

		private static async Task<InMemoryRandomAccessStream> CreateEmptyImage()
		{
			Uri appUri = new Uri("ms-appx:///Assets/EmptyTile.png");
			StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(appUri).AsTask().ConfigureAwait(false);
			IBuffer buffer = await FileIO.ReadBufferAsync(file).AsTask().ConfigureAwait(false);
			InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
			await stream.WriteAsync(buffer).AsTask().ConfigureAwait(false);
			stream.Seek(0);
			return stream;
		}

		protected virtual HttpRequestMessage BuildRequest(ICanvasBitmapTile tile)
		{
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, CreateUri(tile));
			request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3)");
			return request;
		}

		protected virtual Uri CreateUri(ITile tile)
		{
			string uriString = PatternMatcher.Replace(LayerConfiguration.UrlPattern, m => MatchEvaluator(m, tile));
			return new Uri(uriString);
		}

		private string MatchEvaluator(Match match, ITile tile)
		{
			Group keyGroup = match.Groups["key"];
			if (keyGroup == null || string.IsNullOrWhiteSpace(keyGroup.Value))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The URL-Pattern '{0}' is invalid.", match.Value));
			}
			string key = keyGroup.Value.ToLowerInvariant();
			if (key == "x")
			{
				return tile.X.ToString(CultureInfo.InvariantCulture);
			}
			if (key == "y")
			{
				return tile.Y.ToString(CultureInfo.InvariantCulture);
			}
			if (key == "z")
			{
				return tile.TileSet.ToString(CultureInfo.InvariantCulture);
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