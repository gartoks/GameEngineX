using System.Drawing;
using System.Windows.Input;
using GameEngineX.Application.Logging;
using GameEngineX.Game.UserInterface;
using GameEngineX.Graphics;
using GameEngineX.Input;
using Rectangle = GameEngineX.Utility.Math.Rectangle;

namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    public class GUIDebugDisplay : GUIComponent {

        public Key DisplayKey;
        private bool showDisplay;

        private int LineCount;
        private (string, Color)[] Lines;
        private int lineIndex;

        protected override void AdditionalInitialize() {
            DisplayKey = Key.F11;
            InteractionGraphics = new GUIInteractionColors(Color.FromArgb(0, Color.White), Color.FromArgb(0, Color.White), Color.FromArgb(0, Color.White));
            RenderLayer = int.MinValue;

            this.showDisplay = true;
            
            Rectangle worldBounds = Scene.Active.MainViewport.WorldBounds;
            LineCount = (int)(worldBounds.Height / 5.5f) - 1;
            Lines = new (string, Color)[LineCount];
            for (int i = 0; i < Lines.Length; i++) {
                Lines[i] = ("", Color.White);
            }
            lineIndex = 0;

            Log.OnLog += (s, color) => {
                Color c = color == Color.Black ? Color.White : color;

                Lines[lineIndex] = (s, c);
                lineIndex = (lineIndex + 1) % LineCount;
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

            renderer.DrawText(renderLayer,
                Color.Lime, "Consolas", 4,
                Application.ApplicationBase.Instance.FramesPerSecond + "fps " + Application.ApplicationBase.Instance.UpdatesPerSecond + "ups",
                worldBounds.X, worldBounds.Y);


            for (int i = 0; i < LineCount; i++) {
                int idx = (lineIndex + i) % LineCount;
                (string text, Color color) line = Lines[idx];

                renderer.DrawText(renderLayer,
                    Color.FromArgb(63, line.color), "Consolas", 4,
                    line.text,
                    worldBounds.X, worldBounds.Y + worldBounds.Height - i * 5.65f);
            }
        }

    }
}