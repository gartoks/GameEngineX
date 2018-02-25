using System;
using System.Drawing;
using GameEngineX.Game.UserInterface;
using GameEngineX.Graphics;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    public class GUISlider : GUIInteractableComponent {

        private int minimum;
        private int maximum;
        private int stepSize;
        private GUIInteractionGraphics knobGraphics;

        private int value;

        public Action<GUISlider> OnValueChanged;

        protected override void AdditionalInitialize() {
            this.knobGraphics = new GUIInteractionColors(Color.LawnGreen, Color.LawnGreen, Color.LawnGreen);

            OnMouseClicked += OnOnMouseDown;
            OnMouseDown += OnOnMouseDown;
            OnMouseReleased += OnOnMouseDown;

            InteractionGraphics = new GUIInteractionColors(Color.DarkGray, Color.DarkGray, Color.DarkGray);
        }

        private void OnOnMouseDown(float x, float y) {
            float px = x / WorldWidth;

            Percentage = px;
        }

        protected override void AdditionalRender(int renderLayer, Renderer renderer) {
            int steps = (Maximum - Minimum) / StepSize;

            float x = WorldHeight / 2f + Percentage * WorldWidth * ((steps - 1) / (float)steps) - WorldHeight / 2f;
            KnobGraphics.Render(renderLayer, renderer, x, 0, WorldHeight, WorldHeight, InteractionState);
        }

        public int Value {
            get => this.value;
            set {
                this.value = MathUtility.Clamp(value, Minimum, Maximum);
                OnValueChanged?.Invoke(this);
            }
        }

        public float Percentage {
            get => Value / (float)(Maximum - Minimum);
            set => Value = (int)(value * (Maximum - Minimum));
        }

        public int Minimum {
            get => this.minimum;
            set {
                if (value > Maximum)
                    throw new ArgumentOutOfRangeException(nameof(value), "The minimum must be smaller than the maximum.");

                this.minimum = value;
            }
        }

        public int Maximum {
            get => this.maximum;
            set {
                if (value < Minimum)
                    throw new ArgumentOutOfRangeException(nameof(value), "The maximum must be bigger than the minimum.");

                this.maximum = value;
            }
        }

        public int StepSize {
            get => this.stepSize;
            set {
                Math.DivRem(Maximum - Minimum, value, out int remainder);
                if (remainder != 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "The step size must divide the slider range into equal parts.");

                this.stepSize = value;
            }
        }

        public GUIInteractionGraphics KnobGraphics {
            get => this.knobGraphics;
            set => this.knobGraphics = value ?? throw new ArgumentNullException(nameof(value));
        }

    }
}