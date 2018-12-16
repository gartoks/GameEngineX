using System.Collections.Generic;
using System.Drawing;
using System.Windows.Input;
using GameEngineX.Application.Logging;
using GameEngineX.Game.UserInterface;
using GameEngineX.Graphics;
using GameEngineX.Input;
using GameEngineX.Utility.Math;
using Rectangle = GameEngineX.Utility.Math.Rectangle;

namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    public class GUIDebugDisplay : GUIComponent {

        public Key DisplayKey;

        private bool showDisplay;

        public int LineCount;
        private (string, Color)[] Lines;
        private int lineIndex;

        protected override void AdditionalInitialize() {
            DisplayKey = Key.F11;
            InteractionGraphics = new GUIInteractionColors(Color.FromArgb(0, Color.White), Color.FromArgb(0, Color.White), Color.FromArgb(0, Color.White));
            RenderLayer = int.MinValue;

            this.showDisplay = false;
            
            //Rectangle worldBounds = Scene.Active.MainViewport.WorldBounds;
            LineCount = 50;
            //LineCount = (int)(worldBounds.Height / 5.5f) - 1;
            Lines = new (string, Color)[LineCount];
            for (int i = 0; i < Lines.Length; i++) {
                Lines[i] = ("", Color.White);
            }
            lineIndex = 0;

            Log.OnLog += (s, color) => {
                Color c = color == Color.Black ? Color.White : color;

                Lines[lineIndex] = (s, c);
                lineIndex = (lineIndex - 1) % LineCount;
                lineIndex += lineIndex < 0 ? LineCount : 0;
            };
        }

        public override void Update(float deltaTime) {
            if (!InputHandler.IsKeyPressed(DisplayKey))
                return;

            this.showDisplay = !this.showDisplay;
        }

        protected override void AdditionalRender(int renderLayer, Renderer renderer) {
            if (!this.showDisplay)
                return;

            Rectangle worldBounds = Scene.Active.MainViewport.WorldBounds;
            float fontSize = worldBounds.Height / ((LineCount + 4) * 1.5f);

            renderer.DrawText(renderLayer,
                Color.Lime, "Consolas", fontSize,
                Application.ApplicationBase.Instance.FramesPerSecond + "fps " + Application.ApplicationBase.Instance.UpdatesPerSecond + "ups",
                worldBounds.X, worldBounds.Top);

            Vector2 mousePos = Scene.Active.MainViewport.ScreenToWorldCoordinates(InputHandler.MousePosition);

            renderer.DrawText(renderLayer, Color.Lime, "Consolas", fontSize, mousePos.ToString(), 0, worldBounds.Top);
            renderer.DrawText(renderLayer, Color.Lime, "Consolas", fontSize, InputHandler.MousePosition.ToString(), worldBounds.Width / 4f, worldBounds.Top);

            for (int i = 0; i < LineCount; i++) {
                int idx = (lineIndex + i + 1) % LineCount;
                (string text, Color color) line = Lines[idx];

                renderer.DrawText(renderLayer,
                    Color.FromArgb(63, line.color), "Consolas", fontSize,
                    line.text,
                    worldBounds.X, worldBounds.Top - (i + 2) * fontSize * 1.5f);
            }
        }

        public bool Show {
            get => this.showDisplay;
            set => this.showDisplay = value;
        }

    }
}