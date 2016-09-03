using System;
using System.Globalization;
using System.Text.RegularExpressions;

using Windows.Foundation;

using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Interfaces;
using UniversalMapControl.Projections;

namespace UniversalMapControl.Tiles.Default
{
    public class DefaultWebTiler : ITiler
    {
        // Add 8 because 2^8 = 256; which is the TileWidth in Pixels
        private const int ZoomExponent = Wgs84WebMercatorProjection.MaxZoomLevel + 8;

        private static readonly Regex PatternMatcher = new Regex("{(?<key>[^}]+)}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly Random _random = new Random();

        private string MatchEvaluator(Match match, int x, int y, int z)
        {
            Group keyGroup = match.Groups["key"];
            if ((keyGroup == null) || string.IsNullOrWhiteSpace(keyGroup.Value))
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

        public string UrlPattern { get; set; }

        public int GetTileSetForZoomFactor(double zoomFactor)
        {
            if (zoomFactor == 0)
            {
                return Wgs84WebMercatorProjection.MaxZoomLevel;
            }
            return SanitizeTileSet((int)(Wgs84WebMercatorProjection.MaxZoomLevel - Math.Log(1 / zoomFactor, 2)));
        }

        public bool IsPointOnValidTile(CartesianPoint point, int tileSet)
        {
            // All X coordinates are valid, since they wrap around, but Y coordinates do not.
            if ((point.Y < -Wgs84WebMercatorProjection.HalfCartesianMapWidth) || (point.Y >= Wgs84WebMercatorProjection.HalfCartesianMapWidth))
            {
                return false;
            }
            return true;
        }

        public CartesianPoint GetTilePositionForPoint(CartesianPoint point, int tileSet)
        {
            tileSet = SanitizeTileSet(tileSet);
            int tileSize = 1 << (ZoomExponent - tileSet);

            long x = (long)Math.Floor((double)point.X / tileSize) * tileSize;
            long y = (long)Math.Floor((double)point.Y / tileSize) * tileSize;
            if (tileSet == 0)
            {
                x -= Wgs84WebMercatorProjection.HalfCartesianMapWidth;
                y -= Wgs84WebMercatorProjection.HalfCartesianMapWidth;
            }
            return new CartesianPoint(x, y);
        }

        public Size GetTileSize(int tileSet)
        {
            tileSet = SanitizeTileSet(tileSet);
            double tileSize = Math.Pow(2, ZoomExponent - tileSet);
            return new Size(tileSize, tileSize);
        }

        public ICanvasBitmapTile CreateTile(int tileSet, Rect tileBounds, CanvasControl canvas)
        {
            return new CanvasBitmapTile(SanitizeTileSet(tileSet), tileBounds, canvas);
        }

        public Uri GetUrl(ICanvasBitmapTile tile)
        {
            int tileCount = 1 << tile.TileSet;
            int halfTileCount = tileCount / 2;
            int tileSize = 1 << (ZoomExponent - tile.TileSet);
            int x = SanitizeIndex((int)Math.Floor(tile.Bounds.X / tileSize) + halfTileCount, tileCount);
            int y = SanitizeIndex((int)Math.Floor(tile.Bounds.Y / tileSize) + halfTileCount, tileCount);

            string uriString = PatternMatcher.Replace(UrlPattern, m => MatchEvaluator(m, x, y, tile.TileSet));
            return new Uri(uriString);
        }

        private int SanitizeTileSet(int tileSet)
        {
            if (tileSet > Wgs84WebMercatorProjection.MaxZoomLevel)
            {
                return Wgs84WebMercatorProjection.MaxZoomLevel;
            }
            if (tileSet < 0)
            {
                return 0;
            }
            return tileSet;
        }

        protected virtual int SanitizeIndex(int index, int tileCount)
        {
            index = index % tileCount;
            if (index < 0)
            {
                index += tileCount;
            }
            return index;
        }
    }
}