using System;
using System.Drawing;
using GameEngineX.Game.UserInterface;
using GameEngineX.Graphics;
using Rectangle = GameEngineX.Utility.Math.Rectangle;

namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    public class GUILabel : GUIComponent {

        private string text;
        public Color TextColor;
        private string fontName;

        protected override void AdditionalInitialize() {
            InteractionGraphics = new GUIInteractionColors(Color.FromArgb(0, Color.Black), Color.FromArgb(0, Color.Black), Color.FromArgb(0, Color.Black));

            Text = null;
            TextColor = Color.Black;
            FontName = null;
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
                    } catch (Exception) {
                        value = "Arial";
                    } finally {
                        this.fontName = value;
                    }
                } else
                    this.fontName = "Arial";
            }
        }

    }
}