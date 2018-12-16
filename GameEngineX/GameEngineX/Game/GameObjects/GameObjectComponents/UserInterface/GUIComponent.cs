using System;
using GameEngineX.Game.GameObjects.Utility;
using GameEngineX.Game.UserInterface;
using GameEngineX.Graphics;
using GameEngineX.Graphics.Renderables;
using GameEngineX.Utility.Math;
using Rectangle = GameEngineX.Utility.Math.Rectangle;

namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    [RequiredComponents(true, typeof(GUIHandler))]
    public abstract class GUIComponent : GameObjectComponent, IRendering {

        public GUIHandler Handler { get; private set; }

        private GUIInteractionGraphics interactionGraphics;

        private CustomShape shape;
        public int RenderLayer { get; set; }

        public GUIComponentInteractionState InteractionState { get; internal set; }

        private float width;
        private float height;

        public GUIDock Dock = GUIDock.TopLeft;

        public event Action<GUIComponent, float, float> OnMouseEntered;
        public event Action<GUIComponent, float, float> OnMouseHovering;
        public event Action<GUIComponent, float, float> OnMouseExited;

        public sealed override void Initialize() {
            Handler = GameObject.GetComponent<GUIHandler>(GameObjectComponentSearchMode.ParentalHierarchy);
            RenderLayer = -1;

            this.interactionGraphics = new GUIInteractionColors();

            this.shape = new CustomShape(Render);

            InteractionState = GUIComponentInteractionState.None;

            Width = 1;
            Height = 1;

            AdditionalInitialize();
        }

        protected virtual void AdditionalInitialize() { } 

        public sealed override void Death() {
        }

        internal void InvokeMouseEntered(float mouseX, float mouseY) {
            OnMouseEntered?.Invoke(this, mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

        internal void InvokeMouseHovering(float mouseX, float mouseY) {
            OnMouseHovering?.Invoke(this, mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

        internal void InvokeMouseExited(float mouseX, float mouseY) {
            OnMouseExited?.Invoke(this, mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

        private void Render(int renderLayer, Renderer renderer) {
            ResolveDockingCoordinates(Dock, WorldWidth, WorldHeight, out float x, out float y);

            InteractionGraphics.Render(renderLayer, renderer, x, y, WorldWidth, WorldHeight, InteractionState);

            AdditionalRender(renderLayer, renderer);
            //renderer.FillCircle(renderLayer - 1, Color.Red, 0, 0, 1);
            //renderer.DrawRectangle(renderLayer - 1, Color.Green, 0.5f, /*Bounds.X - Transform.Position.X, Bounds.Y - Transform.Position.Y*/0, 0, bounds.Width, bounds.Height);
        }

        protected virtual void AdditionalRender(int renderLayer, Renderer renderer) { }

        public float X {
            get {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;
                Vector2 viewportCenter = viewportWorldBounds.Center;

                float x = (Transform.Position.X - viewportCenter.X) / viewportWorldBounds.Width;
                return x;
            }
            set {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;
                Vector2 viewportCenter = viewportWorldBounds.Center;

                Transform.Position.X = value * viewportWorldBounds.Width + viewportCenter.X;
            }
        }

        public float Y {
            get {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;
                Vector2 viewportCenter = viewportWorldBounds.Center;

                return (Transform.Position.Y - viewportCenter.Y) / viewportWorldBounds.Height;
            }
            set {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;
                Vector2 viewportCenter = viewportWorldBounds.Center;

                Transform.Position.Y = value * viewportWorldBounds.Height + viewportCenter.Y;
            }
        }

        public float Width {
            get => this.width;
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                this.width = value;
            }
        }

        public float Height {
            get => this.height;
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                this.height = value;
            }
        }

        public float WorldWidth {
            get {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;

                return this.width * viewportWorldBounds.Width;
            }
        }

        public float WorldHeight {
            get {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;

                return this.height * viewportWorldBounds.Height;
            }
        }

        public Rectangle Bounds {
            get {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;
                float w = Width * viewportWorldBounds.Width;
                float h = Height * viewportWorldBounds.Height;

                ResolveDockingCoordinates(Dock, w, h, out float x, out float y);


                return new Rectangle(Transform.Position.X + x, Transform.Position.Y + y - h, w, h);
            }
        }

        public GUIInteractionGraphics InteractionGraphics {
            get => this.interactionGraphics;
            set => this.interactionGraphics = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Renderable Renderable => this.shape;

        protected static void ResolveDockingCoordinates(GUIDock dock, float w, float h, out float x, out float y) {
            switch (dock) {
                case GUIDock.Centered:
                    x = -w / 2f;
                    y = h / 2f;
                    break;
                case GUIDock.TopLeft:
                    x = 0;
                    y = 0;
                    break;
                case GUIDock.TopRight:
                    x = -w;
                    y = 0;
                    break;
                case GUIDock.BottomLeft:
                    x = 0;
                    y = h;
                    break;
                case GUIDock.BottomRight:
                    x = -w;
                    y = h;
                    break;
                case GUIDock.TopCenter:
                    x = -w / 2f;
                    y = 0;
                    break;
                case GUIDock.BottomCenter:
                    x = -w / 2f;
                    y = h;
                    break;
                case GUIDock.LeftCenter:
                    x = 0;
                    y = h / 2f;
                    break;
                case GUIDock.RightCenter:
                    x = -w;
                    y = h / 2f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dock), dock, null);
            }
        }

        //public (float x, float y, float width, float height) Bounds => (GameObject.Transform.Position.X + Transform.Position.X, GameObject.Transform.Position.Y + Transform.Position.Y, Width, Height );
    }
}