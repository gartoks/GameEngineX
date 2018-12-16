using System;
using System.Drawing;
using System.Windows.Input;
using GameEngineX.Graphics;
using GameEngineX.Input;

namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    public class GUITextbox : GUIInteractableComponent {

        private const float CURSOR_BLINK_TIME = 0.5f;

        private string text;
        public Color TextColor;
        private string fontName;
        private float fontSize;
        private int maxTextLength;

        private float cashedWidth;

        private float blinkTime;

        protected override void AdditionalInitialize() {
            Text = null;
            TextColor = Color.Black;
            FontName = null;

            this.cashedWidth = -1;
        }

        public override void Update(float deltaTime) {
            if (!HasFocus)
                return;

            if (this.blinkTime >= 2f * CURSOR_BLINK_TIME)
                this.blinkTime -= 2f * CURSOR_BLINK_TIME;
            this.blinkTime = this.blinkTime + deltaTime;

            foreach (Key key in InputHandler.PressedKeys) {
                char c = InputHandler.KeyToChar(key);

                if (key == Key.Back) {
                    if (Text.Length > 0)
                        Text = Text.Substring(0, Text.Length - 1);
                } else if (c != '\0' && (MaxTextLength < 0 || MaxTextLength > Text.Length)) {
                    Text += c;
                }
            }
        }

        protected override void AdditionalRender(int renderLayer, Renderer renderer) {
            if (Width != this.cashedWidth) {
                this.cashedWidth = Width;
                this.fontSize = renderer.GetMaximumFontSizeFitInRectangle(FontName, "A", (int.MaxValue, (int)WorldHeight)).fontSize;
            }

            //Viewport viewport = Handler.Viewport;
            //Rectangle worldBounds = viewport.WorldBounds;
            //viewport.WorldToScreenCoordinates(worldBounds.X + WorldWidth, worldBounds.Y + WorldHeight, out float w, out float h);

            renderer.DrawText(renderLayer, TextColor, FontName, this.fontSize, Text + (HasFocus && this.blinkTime > CURSOR_BLINK_TIME ? "|" : ""), 0, 0);
        }

        public string Text {
            get => this.text;
            set {
                if (value == null)
                    value = "";

                this.text = value;
            }
        }

        public string FontName {
            get => this.fontName;
            set {
                if (value != null) {
                    try {
                        new Font(value, 6);
                    } catch (Exception) {
                        value = "Arial";
                    } finally {
                        this.fontName = value;
                    }
                } else
                    this.fontName = "Arial";
            }
        }

        public int MaxTextLength {
            get => this.maxTextLength;
            set {
                this.maxTextLength = Math.Max(-1, value);

                if (Text.Length > this.maxTextLength)
                    Text = Text.Substring(0, this.maxTextLength);
            }
        }

    }
}