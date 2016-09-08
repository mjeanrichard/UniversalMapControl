using System;

using Windows.Foundation;

using Microsoft.Graphics.Canvas;

namespace UniversalMapControl.Utils
{
    /// <summary>
    /// BatchSprites are not supported on all device. This Wrapper helps to abstract this behavior away.
    /// </summary>
    public abstract class BatchSpriteWrapper : IDisposable
    {
        public static BatchSpriteWrapper Create(CanvasDrawingSession drawingSession)
        {
            return Create(CanvasSpriteBatch.IsSupported(drawingSession.Device), drawingSession);
        }

        public static BatchSpriteWrapper Create(bool isBatchSpriteSupported, CanvasDrawingSession drawingSession)
        {
            if (isBatchSpriteSupported)
            {
                return new SpriteBatchWrapper(drawingSession);
            }
            return new DrawingSessionWrapper(drawingSession);
        }

        public abstract void Draw(CanvasBitmap canvasBitmap, Rect destRect);

        public abstract void Dispose();

        private sealed class SpriteBatchWrapper : BatchSpriteWrapper
        {
            private readonly CanvasSpriteBatch _spriteBatch;

            public SpriteBatchWrapper(CanvasDrawingSession drawingSession)
            {
                _spriteBatch = drawingSession.CreateSpriteBatch(CanvasSpriteSortMode.None, CanvasImageInterpolation.Linear, CanvasSpriteOptions.ClampToSourceRect);
            }

            public override void Draw(CanvasBitmap canvasBitmap, Rect destRect)
            {
                _spriteBatch.Draw(canvasBitmap, destRect);
            }

            public override void Dispose()
            {
                _spriteBatch.Dispose();
            }
        }

        private sealed class DrawingSessionWrapper : BatchSpriteWrapper
        {
            private readonly CanvasDrawingSession _drawingSession;

            public DrawingSessionWrapper(CanvasDrawingSession drawingSession)
            {
                _drawingSession = drawingSession;
            }

            public override void Draw(CanvasBitmap canvasBitmap, Rect destRect)
            {
                _drawingSession.DrawImage(canvasBitmap, destRect);
            }

            public override void Dispose()
            {
            }
        }
    }
}