using System;
using System.Numerics;

using Windows.Foundation;
using Windows.UI;
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
		private readonly Lazy<Map> _parentMap;
		private CanvasControl _canvas;

		public CanvasMapLayer()
		{
			_parentMap = new Lazy<Map>(this.GetParentMap);
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
		}

		protected virtual void OnCreateResource(CanvasControl sender, CanvasCreateResourcesEventArgs args)
		{
		}

		private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			Map parentMap = ParentMap;

			float w2 = (float)(sender.ActualWidth / 2f);
			float h2 = (float)(sender.ActualHeight / 2f);

			float zoomFactor = (float)parentMap.ViewPortProjection.GetZoomFactor(parentMap.ZoomLevel);
			Matrix3x2 transform = Matrix3x2.CreateTranslation(w2, h2);
			transform = Matrix3x2.CreateScale(zoomFactor) * transform;
			transform = Matrix3x2.CreateTranslation(-parentMap.ViewPortCenter.X, -parentMap.ViewPortCenter.Y) * transform;
			double heading = parentMap.Heading * Math.PI / 180.0;
			Vector2 center = parentMap.ViewPortCenter.ToVector();
			transform = Matrix3x2.CreateRotation((float)heading, center) * transform;

			CanvasDrawingSession canvasDrawingSession = args.DrawingSession;
			canvasDrawingSession.Transform = transform;

			DrawInternal(canvasDrawingSession, parentMap);
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

		protected Map ParentMap
		{
			get { return _parentMap.Value; }
		}

		public void Invalidate()
		{
			if (_canvas != null)
			{
				_canvas.Invalidate();
			}
		}
	}
}