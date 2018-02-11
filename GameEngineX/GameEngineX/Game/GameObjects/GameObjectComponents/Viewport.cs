using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using GameEngineX.Graphics;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.GameObjects.GameObjectComponents {
    [Serializable]
    public sealed class Viewport : GameObjectComponent {
        private Renderer renderer;
        private IRenderTarget renderTarget;
        private (float width, float height) targetSize = (1, 1);

        private (float width, float height) size;

        private float zoom;

        private Matrix viewMatrix = null;

        //internal Viewport() {

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

            GameBase.Instance.ActiveSceneObj.AddViewport(this);
        }

        public override void Death() {
            GameBase.Instance.ActiveSceneObj.RemoveViewport(this);

            this.renderer?.Dispose();
        }

        private readonly List<GameObject> renderingGameObjects = new List<GameObject>();
        internal void Render() {
            if (RenderTarget == null || !IsEnabled || !GameObject.IsEnabled || !GameObject.IsAlive)
                return;

            (float x, float y, float width, float height) wB = ExpandedWorldBounds(2);

            //IEnumerable<GameObject> Gos = GameBase.Instance.ActiveSceneObj.GameObjects;

            this.renderingGameObjects.Clear();
            this.renderingGameObjects.AddRange(GameBase.Instance.ActiveSceneObj.GameObjects.Where(gO => gO.Parent == null && wB.RectangleContains(gO.Transform.Position.X, gO.Transform.Position.Y)));  // TODO may need to lock gamebase GOs

            this.renderer.BeginRendering();

            this.renderer.Clear();

            this.renderer.ApplyTranslation(-Transform.Position.X, -Transform.Position.Y);

            foreach (GameObject gO in this.renderingGameObjects) {
                gO.Render(this.renderer);
            }

            this.renderer.RevertTransform();

            this.renderer.FinishRendering(this);
        }

        public bool IsVisisble(float x, float y, float radius = 0) {
            return WorldBounds.RectangleContains(x, y, radius);
        }

        public bool IsVisible(float x, float y, float radius = 0) {
            return WorldBounds.Intersects(x, y, radius);
        }

        public IRenderTarget RenderTarget {
            get => this.renderTarget;
            set {
                this.renderTarget = value;

                if (value != null) {
                    this.targetSize.width = renderTarget.TargetRectangle.Width;
                    this.targetSize.height = renderTarget.TargetRectangle.Height;
                } else {
                    this.targetSize.width = 1;
                    this.targetSize.height = 1;
                }


                this.renderer = value == null ? null : new Renderer(Renderer.BACKGROUND_COLOR, value);
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

        public (float x, float y, float width, float height) WorldBounds => (Transform.Position.X - Width / 2f, Transform.Position.Y - Height / 2f, Width, Height);

        private (float x, float y, float width, float height) ExpandedWorldBounds(float scale) {
            return (Transform.Position.X - scale * Width / 2f, Transform.Position.Y - scale * Height / 2f, Width, scale * Height);
        }

        //public void ZoomToWorldWidth(float worldWidth) {
        //    Zoom = PixelPerMeter * worldWidth / size.width;
        //}

        //public void ZoomToWorldHeight(float worldHeight) {
        //    Zoom = PixelPerMeter * worldHeight / size.height;
        //}

        internal Matrix ViewProjectionMatrix {
            get {
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
    }
}