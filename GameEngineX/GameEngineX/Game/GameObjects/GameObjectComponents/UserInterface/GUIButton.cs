using System;
using System.Drawing;
using GameEngineX.Graphics;
using Rectangle = GameEngineX.Utility.Math.Rectangle;

namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    public class GUIButton : GUIInteractableComponent {
        private string text;
        public Color TextColor;
        private string fontName;
        public bool TriggerOnRelease;

        public event Action<GUIButton> OnButtonClick;

        protected override void AdditionalInitialize() {
            Text = null;
            TextColor = Color.Black;
            FontName = null;
            TriggerOnRelease = true;

            OnMouseClicked += (b, x, y) => {
                if (TriggerOnRelease)
                    return;

                OnButtonClick?.Invoke(this);
            };

            OnMouseReleased += (b, x, y) => {
                if (!TriggerOnRelease)
                    return;

                OnButtonClick?.Invoke(this);
            };
        }

        protected override void AdditionalRender(int renderLayer, Renderer renderer) {
            Viewport viewport = Handler.Viewport;

            Rectangle worldBounds = viewport.WorldBounds;
            viewport.WorldToScreenCoordinates(worldBounds.X + WorldWidth, worldBounds.Y + WorldHeight, out float w, out float h);

            ResolveDockingCoordinates(Dock, WorldWidth, WorldHeight, out float x, out float y);

            renderer.DrawText(renderLayer, TextColor, FontName, Text, x, y, ((int)WorldWidth, (int)WorldHeight));
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
                    }
                    catch (Exception) {
                        value = "Arial";
                    }
                    finally {
                        this.fontName = value;
                    }
                } else
                    this.fontName = "Arial";


            }
        }

    }
}
