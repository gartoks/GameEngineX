using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using GameEngineX.Game.GameObjects;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Utility.DataStructures;
using GameEngineX.Utility.Math;

namespace GameEngineX.Graphics {
    public class Renderer : IDisposable {

        internal static Color BACKGROUND_COLOR;
        internal static string DEFAULT_FONT_NAME;

        private static readonly Matrix IDENTITY = new Matrix();

        private readonly IRenderTarget renderTarget;

        public readonly Color ClearColor;

        private BufferedGraphics bufferedGraphics;
        private BufferedGraphicsContext bufferedGraphicsContext;

        private readonly Stack<Matrix> transformStack;

        private readonly ObjectPool<Matrix> matrixPool;

        private System.Drawing.Graphics graphics;
        private readonly List<(Matrix transformationMatrix, int renderLayer, Action renderAction)> renderQueue;
        private int renderQueueSize;

        private readonly List<RectangleF> rectangleCache;
        private int rectangleCasheNextAvailableIndex;

        internal Renderer(Color clearColor, IRenderTarget renderTarget) {
            //IsScreenRenderer = renderTarget is ScreenRenderTarget;
            this.renderTarget = renderTarget;

            ClearColor = clearColor;

            this.bufferedGraphicsContext = new BufferedGraphicsContext();
            this.bufferedGraphics = this.bufferedGraphicsContext.Allocate(renderTarget.Graphics, renderTarget.TargetRectangle);

            this.transformStack = new Stack<Matrix>();
            this.matrixPool = new ObjectPool<Matrix>(() => new Matrix(), m => { m.Reset(); return m; });

            this.renderQueue = new List<(Matrix, int, Action)>();

            this.rectangleCache = new List<RectangleF>();
            ResetRectangleCache();
        }

        internal void BeginRendering() {
            if (IsRendering)
                return;

            this.graphics = this.bufferedGraphics.Graphics;
            this.graphics.SmoothingMode = SmoothingMode.HighQuality;
            this.graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            this.graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            this.transformStack.Clear();
            this.renderQueue.Clear();
            this.renderQueueSize = 0;
            ResetRectangleCache();

            int rTw = this.renderTarget.TargetRectangle.Width;
            int rTh = this.renderTarget.TargetRectangle.Height;
            int minSize = Math.Min(rTw, rTh);

            //ApplyScaling(minSize / 2f, minSize / 2f);
            //ApplyTranslation(rTw / (float)minSize, -rTh / (float)minSize);
        }

        internal void FinishRendering(Viewport viewport) {
            if (!IsRendering)
                return;

            foreach (Matrix m in this.transformStack) {
                this.matrixPool.Put(m);
            }

            Matrix viewprojectionMatrix = viewport.ViewProjectionMatrix;

            (Matrix transformationMatrix, int renderLayer, Action renderAction)[] orderedRenderQueue = this.renderQueue.OrderByDescending(k => k.renderLayer).ToArray();

            for (int i = 0; i < this.renderQueueSize; i++) {
                var element = orderedRenderQueue[i];
                Matrix m = element.transformationMatrix;
                Action a = element.renderAction;

                m.Multiply(viewprojectionMatrix, MatrixOrder.Append);

                //this.graphics.DrawLine(GetPen(Color.White, 1), Viewport.Width / 2f, 0, Viewport.Width / 2f, Viewport.Height);
                //this.graphics.DrawLine(GetPen(Color.White, 1), 0, Viewport.Height / 2f, Viewport.Width, Viewport.Height / 2f);

                this.graphics.Transform = m;

                a();

                this.graphics.Transform = IDENTITY;
            }

            this.graphics = null;

            try {
                this.bufferedGraphics.Render();
            } catch (Exception) {
                // ignored
            }
        }

        internal void Clear() {
            if (!IsRendering)
                return;

            this.graphics.Clear(ClearColor);
        }

        private RectangleF GetCachedRectangle(float x, float y, float width, float height) {
            if (this.rectangleCasheNextAvailableIndex == this.rectangleCache.Count)
                this.rectangleCache.Add(new RectangleF());

            RectangleF r = this.rectangleCache[this.rectangleCasheNextAvailableIndex++];
            r.X = x;
            r.Y = y;
            r.Width = width;
            r.Height = height;

            return r;
        }

        private void ResetRectangleCache() {
            this.rectangleCasheNextAvailableIndex = 0;
        }

        #region Texture
        public void DrawCenteredTexture(int renderLayer, Image image, float srcX, float srcY, float srcWidth, float srcHeight, float x, float y, float width, float height) {
            void Draw() => this.graphics.DrawImage(image, GetCachedRectangle(x - width / 2f, -y - height / 2f, width, height), GetCachedRectangle(srcX, srcY, srcWidth, srcHeight), GraphicsUnit.Pixel);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, Draw));
            this.renderQueueSize++;
        }

        public void DrawCenteredTexture(int renderLayer, Image image, float x, float y, float width, float height) {
            void Draw() => this.graphics.DrawImage(image, x - width / 2f, -y - height / 2f, width, height);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, Draw));
            this.renderQueueSize++;
        }

        public void DrawTexture(int renderLayer, Image image, float x, float y, float width, float height) {
            void Draw() => this.graphics.DrawImage(image, x, -y, width, height);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, Draw));
            this.renderQueueSize++;
        }
        #endregion

        #region Text
        public void DrawText(int renderLayer, Color color, string fontName, float fontSize, string text, float x, float y) {
            Font f = GetFont(fontName, fontSize);

            //float fontHeight = this.graphics.MeasureString(text, f).Height;
            Action a = () => this.graphics.DrawString(text, f, GetBrush(color), x, -y/* - fontHeight*/);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, a));
            this.renderQueueSize++;
        }

        public void DrawText(int renderLayer, Color color, string fontName, string text, float x, float y, (int width, int height) container) {
            (int fontSize, float width, float height) data = GetMaximumFontSizeFitInRectangle(fontName, text, container);

            //DrawRectangle(renderLayer, color, 1f, x, y, data.width, data.height);
            DrawText(renderLayer, color, fontName, data.fontSize, text, x + (container.width - data.width) / 2f, y - (container.height - data.height) / 2f);
        }

        public void DrawCenteredText(int renderLayer, Color color, string fontName, string text, float x, float y, (int width, int height) container) {
            (int fontSize, float width, float height) data = GetMaximumFontSizeFitInRectangle(fontName, text, container);

            //DrawCenteredRectangle(renderLayer, color, 1f, x, y, data.width, data.height);
            DrawText(renderLayer, color, fontName, data.fontSize, text, x - data.width / 2f, y - data.height / 2f);
        }

        public (int fontSize, float width, float height) GetMaximumFontSizeFitInRectangle(string fontName, string text, (int width, int height) container, int minimumFontSize = 3, int maximumFontSize = 1000) {
            Font newFont;
            float prevWidth = 0;
            float prevHeight = 0;
            for (int newFontSize = minimumFontSize; ; newFontSize++) {
                newFont = GetFont(fontName, newFontSize);
                //newFont = new Font(font.FontFamily, newFontSize, font.Style);

                SizeF size = MeasureDrawTextSize(text, newFont);
                if (size.Width > container.width || size.Height > container.height || newFontSize > maximumFontSize)
                    return (newFontSize - 1, prevWidth, prevHeight);

                prevWidth = size.Width;
                prevHeight = size.Height;
            }
        }

        public SizeF MeasureDrawTextSize(string text, Font font) {
            if (!IsRendering)
                throw new RenderException("Cannot measure string when not rendering.");

            SizeF s = this.graphics.MeasureString(text, font);
            s.Width *= 0.975f;
            s.Height *= 0.95f;

            return s;
        }
        #endregion

        #region Point
        public void DrawPoint(int renderLayer, Color color, float size, Vector2 p) {
            DrawPoint(renderLayer, color, size, p.X, p.Y);
        }

        public void DrawPoint(int renderLayer, Color color, float size, float x, float y) {
            FillEllipse(renderLayer, color, x, y, size, size);
        }
        #endregion

        #region Line
        public void DrawLine(int renderLayer, Color color, float size, Vector2 from, Vector2 to) {
            DrawLine(renderLayer, color, size, from.X, from.Y, to.X, to.Y);
        }

        public void DrawLine(int renderLayer, Color color, float size, float x0, float y0, float x1, float y1) {
            Action a = () => this.graphics.DrawLine(GetPen(color, size), x0, -y0, x1, -y1);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, a));
            this.renderQueueSize++;
        }
        #endregion

        #region Polygon
        //public void FillPolygon(int renderLayer, Color color, Vector2 position, Polygon polygon) {
        //    FillPolygon(color, position.X, position.Y, polygon);
        //}

        //public void FillPolygon(int renderLayer, Color color, float x, float y, Polygon polygon) {
        //}

        public void DrawPolygon(int renderLayer, Color color, float size, Vector2 position, Polygon polygon) {
            DrawPolygon(renderLayer, color, size, position.X, position.Y, polygon);
        }

        public void DrawPolygon(int renderLayer, Color color, float size, float x, float y, Polygon polygon) {

            int points = polygon.Points.Count();
            for (int i = 0; i < points; i++) {
                Vector2 start = polygon.Points.ElementAt(i);
                Vector2 end = polygon.Points.ElementAt((i + 1) % points);

                Action a = () => this.graphics.DrawLine(GetPen(color, size), x + start.X, y + start.Y, x + end.X, y + end.Y);
                this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, a));
                this.renderQueueSize++;
            }
        }
        #endregion

        #region Rectangle
        public void FillCenteredRectangle(int renderLayer, Color color, float centerX, float centerY, float width, float height) {
            FillRectangle(renderLayer, color, centerX - width / 2f, centerY + height / 2f, width, height);
        }

        public void FillRectangle(int renderLayer, Color color, float minX, float minY, float width, float height) {
            width = Math.Max(0.0000001f, width);
            height = Math.Max(0.000001f, height);

            Action a = () => this.graphics.FillRectangle(GetBrush(color), minX, -minY/* - height*/, width, height);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, a));
            this.renderQueueSize++;
        }

        public void DrawCenteredRectangle(int renderLayer, Color color, float size, float centerX, float centerY, float width, float height) {
            DrawRectangle(renderLayer, color, size, centerX - width / 2f, centerY + height / 2f, width, height);
        }

        public void DrawRectangle(int renderLayer, Color color, float size, float minX, float minY, float width, float height) {
            width = Math.Max(0.0000001f, width);
            height = Math.Max(0.0000001f, height);

            Action a = () => this.graphics.DrawRectangle(GetPen(color, size), minX, -minY/* - height*/, width, height);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, a));
            this.renderQueueSize++;
        }
        #endregion

        #region Circle
        public void FillCircle(int renderLayer, Color color, float x, float y, float radius) {
            FillEllipse(renderLayer, color, x, y, radius, radius);
        }

        public void DrawCircle(int renderLayer, Color color, float size, float x, float y, float radius) {
            DrawEllipse(renderLayer, color, size, x, y, radius, radius);
        }
        #endregion

        #region Ellipse
        public void FillEllipse(int renderLayer, Color color, Vector2 position, float radiusX, float radiusY) {
            FillEllipse(renderLayer, color, position.X, position.Y, radiusX, radiusY);
        }

        public void FillEllipse(int renderLayer, Color color, float x, float y, float radiusX, float radiusY) {
            radiusX = Math.Max(0.0000001f, radiusX);
            radiusY = Math.Max(0.0000001f, radiusY);

            float diameterX = radiusX * 2f;
            float diameterY = radiusY * 2f;
            Action a = () => this.graphics.FillEllipse(GetBrush(color), x - radiusX, -y /* - diameterY*/ - radiusY, diameterX, diameterY);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, a));
            this.renderQueueSize++;
        }

        public void DrawEllipse(int renderLayer, Color color, float size, Vector2 position, float radiusX, float radiusY) {
            DrawEllipse(renderLayer, color, size, position.X, position.Y, radiusX, radiusY);
        }

        public void DrawEllipse(int renderLayer, Color color, float size, float x, float y, float radiusX, float radiusY) {
            radiusX = Math.Max(0.0000001f, radiusX);
            radiusY = Math.Max(0.0000001f, radiusY);

            float diameterX = radiusX * 2f;
            float diameterY = radiusY * 2f;
            Action a = () => this.graphics.DrawEllipse(GetPen(color, size), x - radiusX, -y/* - diameterY*/ - radiusY, diameterX, diameterY);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, a));
            this.renderQueueSize++;
        }
        #endregion

        #region Pie
        public void FillPie(int renderLayer, Color color, float x, float y, float radius, float startAngle, float sweepAngle) {
            radius = Math.Max(0.0000001f, radius);
            sweepAngle = Math.Max(0.0000001f, sweepAngle);

            float diameter = radius * 2f;
            Action a = () => this.graphics.FillPie(GetBrush(color), x - radius, -y/* - diameter*/ - radius, diameter, diameter, -startAngle, -sweepAngle);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, a));
            this.renderQueueSize++;
        }

        public void DrawPie(int renderLayer, Color color, float size, float x, float y, float radius, float startAngle, float sweepAngle) {
            radius = Math.Max(0.0000001f, radius);
            sweepAngle = Math.Max(0.0000001f, sweepAngle);

            float diameter = radius * 2f;
            Action a = () => this.graphics.DrawPie(GetPen(color, size), x - radius, -y/* - diameter*/ - radius, diameter, diameter, -startAngle, -sweepAngle);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, a));
            this.renderQueueSize++;
        }
        #endregion


        #region Ring
        public void DrawRing(int renderLayer, Color color, float size, float x, float y, float radiusX, float radiusY, float startAngle, float sweepAngle) {
            sweepAngle = Math.Max(0.0000001f, sweepAngle);
            radiusX = Math.Max(0.0000001f, radiusX);
            radiusY = Math.Max(0.0000001f, radiusY);

            float diameterX = radiusX * 2f;
            float diameterY = radiusY * 2f;
            Action a = () => this.graphics.DrawArc(GetPen(color, size), x - radiusX, -y /* - diameterY*/ - radiusY, diameterX, diameterY,startAngle, sweepAngle);

            this.renderQueue.Add((CurrentTransformationMatrix, renderLayer, a));
            this.renderQueueSize++;
        }
        #endregion

        public void ApplyTransformation(Transform transform) {
            if (!IsRendering)
                return;

            Matrix m = transform.GlobalTransformationMatrix;
            if (this.transformStack.Any())
                m.Multiply(this.transformStack.Peek(), MatrixOrder.Append);

            this.transformStack.Push(m);
        }

        public void ApplyTranslation(float dx, float dy) {
            if (!IsRendering)
                return;

            Matrix m = this.matrixPool.Get();
            m.Translate(dx, -dy);
            if (this.transformStack.Any())
                m.Multiply(this.transformStack.Peek(), MatrixOrder.Append);

            this.transformStack.Push(m);
        }

        public void ApplyRotation(float angle) {
            if (!IsRendering)
                return;

            Matrix m = this.matrixPool.Get();
            m.Rotate(angle * MathUtility.RadToDegf);
            if (this.transformStack.Any())
                m.Multiply(this.transformStack.Peek(), MatrixOrder.Append);

            this.transformStack.Push(m);
        }

        public void ApplyRotation(float angle, float px, float py) {
            if (!IsRendering)
                return;

            Matrix m = this.matrixPool.Get();
            m.RotateAt(angle * MathUtility.RadToDegf, new PointF(px, py));
            if (this.transformStack.Any())
                m.Multiply(this.transformStack.Peek(), MatrixOrder.Append);

            this.transformStack.Push(m);
        }

        public void ApplyScaling(float sx, float sy) {
            if (!IsRendering)
                return;

            Matrix m = this.matrixPool.Get();
            m.Scale(sx, sy);
            if (this.transformStack.Any())
                m.Multiply(this.transformStack.Peek(), MatrixOrder.Append);

            this.transformStack.Push(m);
        }

        public void RevertTransform() {
            if (!IsRendering)
                return;

            if (this.transformStack.Count == 0)
                return;

            this.matrixPool.Put(this.transformStack.Pop());
        }

        public void Dispose() {
            this.bufferedGraphics?.Dispose();
            this.bufferedGraphicsContext?.Dispose();

            this.bufferedGraphics = null;
            this.bufferedGraphicsContext = null;
        }

        public bool IsRendering => this.graphics != null;

        private Matrix CurrentTransformationMatrix => this.transformStack.Any() ? this.transformStack.Peek().Clone() : this.matrixPool.Get();



        private static readonly Dictionary<Color, Brush> cachedBrushes;
        private static readonly Dictionary<Color, Dictionary<float, Pen>> cachedPens;
        private static readonly Dictionary<string, Dictionary<float, Font>> cachedFonts;

        static Renderer() {
            Renderer.cachedBrushes = new Dictionary<Color, Brush>();
            Renderer.cachedPens = new Dictionary<Color, Dictionary<float, Pen>>();
            Renderer.cachedFonts = new Dictionary<string, Dictionary<float, Font>>();

            foreach (FontFamily font in FontFamily.Families) {
                Renderer.cachedFonts[font.Name] = new Dictionary<float, Font>();
            }
        }

        public static IEnumerable<string> AvailableFonts {
            get {
                foreach (FontFamily font in FontFamily.Families)
                    yield return font.Name;
            }
        }

        private static Brush GetBrush(Color color) {
            Brush b;
            if (!Renderer.cachedBrushes.TryGetValue(color, out b)) {
                b = new SolidBrush(color);
                Renderer.cachedBrushes[color] = b;
            }
            return b;
        }

        private static Font GetFont(string fontName, float size) {
            if (!Renderer.cachedFonts.ContainsKey(fontName))
                fontName = DEFAULT_FONT_NAME;

            size = Math.Max(1, size);

            Dictionary<float, Font> fonts = Renderer.cachedFonts[fontName];
            Font f;
            if (!fonts.TryGetValue(size, out f)) {
                f = new Font(fontName, size);
                fonts[size] = f;
            }

            return f;
        }

        private static Pen GetPen(Color color, float size) {
            Pen p;

            if (!Renderer.cachedPens.TryGetValue(color, out Dictionary<float, Pen> sizedPens)) {
                sizedPens = new Dictionary<float, Pen>();
                Renderer.cachedPens[color] = sizedPens;

                p = new Pen(color, size);
                sizedPens[size] = p;

            } else if (!sizedPens.TryGetValue(size, out p)) {
                p = new Pen(color, size);
                sizedPens[size] = p;
            }

            return p;
        }
    }
}
