using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.Serialization;
using GameEngineX.Graphics;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.GameObjects.GameObjectComponents {
    public sealed class Viewport : GameObjectComponent, ISerializable {
        private Renderer renderer;
        private IRenderTarget renderTarget;
        private (float width, float height) targetSize = (1, 1);

        private (float width, float height) size;

        private float zoom;

        private Matrix viewMatrix = null;

        //internal Viewport(SerializationInfo info, StreamingContext ctxt) 
        //    : base(info, ctxt) {

            //float tSW = info.GetSingle(nameof(targetSize) + "Width");
            //float tSH = info.GetSingle(nameof(targetSize) + "Height");
            //targetSize = (tSW, tSH);

            //float sW = info.GetSingle(nameof(size) + "Width");
            //float sH = info.GetSingle(nameof(size) + "Height");
            //size = (sW, sH);

            //zoom = info.GetSingle(nameof(zoom));
            //viewMatrix = MatrixExtensions.CreateMatrix((float[])info.GetValue(nameof(viewMatrix), typeof(float[])));

            //object rTObj = info.GetValue(nameof(renderTarget), typeof(IRenderTarget));
            //RenderTarget =    TODO
        //}

        public override void Initialize() {
            this.renderTarget = null;
            this.targetSize = (1, 1);

            Width = 2f;
            Height = 2f;

            // TODO register at game

            //PixelPerMeter = pixelPerMeter;
            //MeterPerPixel = 1f / PixelPerMeter;

            //this.size = (viewportWidth, viewportHeight);

            Zoom = 1;

            GameBase.Instance.ActiveScene.AddViewport(this);
        }

        public override void Death() {
            GameBase.Instance.ActiveScene.RemoveViewport(this);

            this.renderer?.Dispose();
        }

        private readonly List<GameObject> renderingGameObjects = new List<GameObject>();
        internal void Render() {
            if (!IsActive)
                return;

            Rectangle wB = ExpandedWorldBounds(2);

            this.renderingGameObjects.Clear();  // TODO
            this.renderingGameObjects.AddRange(GameBase.Instance.ActiveScene.GameObjects.Where(gO => gO.Parent == null && wB.Contains(gO.Transform.Position.X, gO.Transform.Position.Y)));  // TODO may need to lock gamebase GOs

            this.renderer.BeginRendering();

            this.renderer.Clear();

            this.renderer.ApplyTranslation(-Transform.Position.X, -Transform.Position.Y);

            foreach (GameObject gO in this.renderingGameObjects) {
                gO.Render(this.renderer);
            }

            this.renderer.RevertTransform();

            this.renderer.FinishRendering(this);
        }

        public void ScreenToWorldCoordinates(float sx, float sy, out float wx, out float wy) {
            wx = sx / targetSize.width;
            wy = sy / targetSize.height;
            Rectangle worldBounds = WorldBounds;

            wy = 1f - wy;

            wx *= worldBounds.Width;
            wy *= worldBounds.Height;

            wx += worldBounds.X - worldBounds.Center.x;
            wy += worldBounds.Y - worldBounds.Center.y;
        }

        public (float x, float y) ScreenToWorldCoordinates((float x, float y) pos) {
            ScreenToWorldCoordinates(pos.x, pos.y, out float x, out float y);

            pos.x = x;
            pos.y = y;

            return pos;
        }

        public void WorldToScreenCoordinates(float wx, float wy, out float sx, out float sy) {
            Rectangle worldBounds = WorldBounds;

            sx = wx - worldBounds.X;
            sy = wy - worldBounds.Y;

            sx /= worldBounds.Width;
            sy /= worldBounds.Height;

            sx *= targetSize.width;
            sy *= targetSize.height;
        }

        public (float x, float y) WorldToScreenCoordinates((float x, float y) pos) {
            WorldToScreenCoordinates(pos.x, pos.y, out float x, out float y);

            pos.x = x;
            pos.y = y;

            return pos;
        }

        public bool IsVisible(float x, float y, float radius = 0) {
            return WorldBounds.Contains(x, y, radius);
        }

        public IRenderTarget RenderTarget {
            get => this.renderTarget;
            set {
                this.renderTarget = value;

                value = value ?? new ScreenRenderTarget();

                //if (value != null) {
                float min = Math.Min(value.TargetRectangle.Width, value.TargetRectangle.Height);
                    this.targetSize.width = min;
                    this.targetSize.height = min;
                //} else {
                    this.targetSize.width = value.TargetRectangle.Width;
                    this.targetSize.height = value.TargetRectangle.Height;
                //}

                this.renderer = new Renderer(Renderer.BACKGROUND_COLOR, value);
            }
        }

        public float Width {
            get => this.size.width;
            set => this.size.width = value;
        }

        public float Height {
            get => this.size.height;
            set => this.size.height = value;
        }

        public float Zoom {
            get => this.zoom;
            set {
                if (value == 0)
                    throw new ArgumentException();

                this.zoom = value;
            }
        }

        public void ResetZoom() {
            Zoom = 1;
        }

        public Rectangle WorldBounds {
            get {
                float ratio = targetSize.height / targetSize.width;
                float x = Zoom * Width / (2f * ratio);
                float y = Zoom * Height / 2f;

                return new Rectangle(Transform.Position.X - x, Transform.Position.Y - y, Width / ratio, Height);
            }
        }

        private Rectangle ExpandedWorldBounds(float scale) {
            float ratio = targetSize.height / targetSize.width;
            float x = scale * Zoom * Width / (2f * ratio);
            float y = scale * Zoom * Height / 2f;

            return new Rectangle(Transform.Position.X - x, Transform.Position.Y - y, scale * Width / ratio, scale * Height);
        }

        internal Matrix ViewProjectionMatrix {
            get {
                Matrix m = new Matrix();

                if (this.viewMatrix == null) {
                    this.viewMatrix = new Matrix();
                    this.viewMatrix.Scale(Zoom * (this.targetSize.width / Width) * targetSize.height / targetSize.width, Zoom * (this.targetSize.height / Height));
                    this.viewMatrix.Rotate(0, MatrixOrder.Append);// TODO
                    //this.viewMatrix.Translate(this.targetSize.width / 2f - Transform.Position.X, this.targetSize.height / 2f + Transform.Position.Y, MatrixOrder.Append);
                    this.viewMatrix.Translate(this.targetSize.width / 2f, this.targetSize.height / 2f, MatrixOrder.Append);
                }

                return this.viewMatrix;
            }
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext context) {
            base.GetObjectData(info, context);

            info.AddValue(nameof(targetSize) + "Width", targetSize.width);
            info.AddValue(nameof(targetSize) + "Height", targetSize.height);

            info.AddValue(nameof(size) + "Width", size.width);
            info.AddValue(nameof(size) + "Height", size.height);

            info.AddValue(nameof(zoom), zoom);

            info.AddValue(nameof(viewMatrix.Elements), typeof(float[]));

            //object rTObj = info.GetValue(nameof(renderTarget), typeof(IRenderTarget));
            //RenderTarget =
        }

    }
}