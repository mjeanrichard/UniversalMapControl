using System;
using System.Numerics;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

using UniversalMapControl.Utils;

namespace UniversalMapControl
{
	public class CanvasMapLayer : UserControl
	{
		private const float OffsetFactor = 100000F;

		private Map _parentMap;
		private CanvasControl _canvas;

		private float _xOffset = 0;
		private float _yOffset = 0;
		private double _currentZoomFactor;

		public CanvasMapLayer()
		{
			Loaded += OnLayerLoaded;
			Unloaded += OnLayerUnloaded;
		}

		public CanvasControl Canvas
		{
			get { return _canvas; }
		}

		protected virtual void OnLayerLoaded(object sender, RoutedEventArgs e)
		{
			_canvas = new CanvasControl();

			_canvas.Draw += OnDraw;
			_canvas.CreateResources += OnCreateResource;
			Content = _canvas;

			_parentMap = this.GetParentMap();
			_parentMap.ProjectionChanged += (s, args) => Invalidate();
		}

		protected virtual void OnCreateResource(CanvasControl sender, CanvasCreateResourcesEventArgs args)
		{
		}

		protected virtual void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			Map parentMap = ParentMap;

			float w2 = (float)(sender.ActualWidth / 2f);
			float h2 = (float)(sender.ActualHeight / 2f);

			double zoomFactor = parentMap.ViewPortProjection.GetZoomFactor(parentMap.ZoomLevel);
			int factor = (int)(parentMap.ViewPortCenter.X * zoomFactor / OffsetFactor);
			float newXOffset = factor * OffsetFactor;
			factor = (int)(parentMap.ViewPortCenter.Y * zoomFactor / OffsetFactor);
			float newYOffset = factor * OffsetFactor;

			if (zoomFactor != _currentZoomFactor || newXOffset != _xOffset || newYOffset != _yOffset)
			{
				_currentZoomFactor = zoomFactor;
				_xOffset = newXOffset;
				_yOffset = newYOffset;
				InvalidateScaledValues();
			}

			float tx = Convert.ToSingle(_xOffset + -parentMap.ViewPortCenter.X * zoomFactor);
			float ty = Convert.ToSingle(_yOffset + -parentMap.ViewPortCenter.Y * zoomFactor);

			Matrix3x2 transform = Matrix3x2.CreateTranslation(w2 + tx, h2 + ty);

			float heading = Convert.ToSingle(parentMap.Heading * Math.PI / 180.0);
			transform = Matrix3x2.CreateRotation(heading, new Vector2(-tx, -ty)) * transform;

			CanvasDrawingSession canvasDrawingSession = args.DrawingSession;
			canvasDrawingSession.Transform = transform;

			DrawInternal(canvasDrawingSession, parentMap);
		}

		protected virtual void InvalidateScaledValues()
		{
		}

		protected virtual void DrawInternal(CanvasDrawingSession drawingSession, Map parentMap)
		{
		}

		private void OnLayerUnloaded(object sender, RoutedEventArgs e)
		{
			// Explicitly remove references to allow the Win2D controls to get garbage collected
			if (_canvas != null)
			{
				_canvas.RemoveFromVisualTree();
				_canvas = null;
			}
		}

		public Vector2 Scale(CartesianPoint point)
		{
			double zoomFactor = ParentMap.ViewPortProjection.GetZoomFactor(ParentMap.ZoomLevel);
			float x = (float)(point.X * zoomFactor - _xOffset);
			float y = (float)(point.Y * zoomFactor - _yOffset);
			return new Vector2(x, y);
		}

		public Rect Scale(Rect rect)
		{
			Rect result = new Rect();
			double zoomFactor = ParentMap.ViewPortProjection.GetZoomFactor(ParentMap.ZoomLevel);
			result.X = rect.X * zoomFactor - _xOffset;
			result.Y = rect.Y * zoomFactor - _yOffset;

			result.Width = rect.Width * zoomFactor;
			result.Height = rect.Height * zoomFactor;

			return result;
		}

		public float Scale(double value)
		{
			double zoomFactor = ParentMap.ViewPortProjection.GetZoomFactor(ParentMap.ZoomLevel);
			return (float)(value * zoomFactor);
		}

		public Map ParentMap
		{
			get { return _parentMap; }
		}

		public virtual void Invalidate()
		{
			if (_canvas != null)
			{
				_canvas.Invalidate();
			}
		}
	}
}