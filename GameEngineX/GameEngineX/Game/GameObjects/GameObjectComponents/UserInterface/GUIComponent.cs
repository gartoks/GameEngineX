using System;
using GameEngineX.Game.GameObjects.Utility;
using GameEngineX.Game.UserInterface;
using GameEngineX.Graphics;
using GameEngineX.Graphics.Renderables;
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

        public event Action<float, float> OnMouseEntered;
        public event Action<float, float> OnMouseHovering;
        public event Action<float, float> OnMouseExited;

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
            OnMouseEntered?.Invoke(mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

        internal void InvokeMouseHovering(float mouseX, float mouseY) {
            OnMouseHovering?.Invoke(mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

        internal void InvokeMouseExited(float mouseX, float mouseY) {
            OnMouseExited?.Invoke(mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

        private void Render(int renderLayer, Renderer renderer) {
            InteractionGraphics.Render(renderLayer, renderer, 0, 0, WorldWidth, WorldHeight, InteractionState);

            AdditionalRender(renderLayer, renderer);
        }

        protected virtual void AdditionalRender(int renderLayer, Renderer renderer) { }

        public float X {
            get {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;
                (float x, float y) viewportCenter = viewportWorldBounds.Center;

                float x = (Transform.Position.X - viewportCenter.x) / viewportWorldBounds.Width;
                return x;
            }
            set {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;
                (float x, float y) viewportCenter = viewportWorldBounds.Center;

                Transform.Position.X = value * viewportWorldBounds.Width + viewportCenter.x;
            }
        }

        public float Y {
            get {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;
                (float x, float y) viewportCenter = viewportWorldBounds.Center;

                return (Transform.Position.Y - viewportCenter.y) / viewportWorldBounds.Height;
            }
            set {
                Rectangle viewportWorldBounds = Scene.Active.MainViewport.WorldBounds;
                (float x, float y) viewportCenter = viewportWorldBounds.Center;

                Transform.Position.Y = value * viewportWorldBounds.Height + viewportCenter.y;
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

                return new Rectangle(Transform.Position.X, Transform.Position.Y, Width * viewportWorldBounds.Width, Height * viewportWorldBounds.Height);
            }
        }

        public GUIInteractionGraphics InteractionGraphics {
            get => this.interactionGraphics;
            set => this.interactionGraphics = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Renderable Renderable => this.shape;

        //public (float x, float y, float width, float height) Bounds => (GameObject.Transform.Position.X + Transform.Position.X, GameObject.Transform.Position.Y + Transform.Position.Y, Width, Height );
    }
}