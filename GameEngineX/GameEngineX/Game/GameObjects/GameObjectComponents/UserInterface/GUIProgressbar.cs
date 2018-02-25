using System;
using System.Drawing;
using GameEngineX.Game.UserInterface;
using GameEngineX.Graphics;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    public class GUIProgressbar : GUIInteractableComponent {
        private float value;

        private GUIInteractionGraphics barGraphics;

        public event Action<GUIProgressbar> OnValueChanged;

        protected override void AdditionalInitialize() {

            this.barGraphics = new GUIInteractionColors(Color.LawnGreen, Color.LawnGreen, Color.LawnGreen);

            OnMouseClicked += OnOnMouseDown;
            OnMouseDown += OnOnMouseDown;
            OnMouseReleased += OnOnMouseDown;

            InteractionGraphics = new GUIInteractionColors(Color.DarkGray, Color.DarkGray, Color.DarkGray);
        }

        private void OnOnMouseDown(float x, float y) {
            float px = x / WorldWidth;

            Value = px;
        }

        protected override void AdditionalRender(int renderLayer, Renderer renderer) {
            BarGraphics.Render(renderLayer, renderer, 0, 0, WorldWidth * Value, WorldHeight, InteractionState);
        }

        public float Value {
            get => this.value;
            set {
                this.value = MathUtility.Clamp01(value);
                OnValueChanged?.Invoke(this);
            }
        }

        public GUIInteractionGraphics BarGraphics {
            get => this.barGraphics;
            set => this.barGraphics = value ?? throw new ArgumentNullException(nameof(value));
        }

    }
}