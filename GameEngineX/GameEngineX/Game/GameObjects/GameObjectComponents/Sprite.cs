using System.Drawing;
using GameEngineX.Graphics;
using GameEngineX.Graphics.Renderables;

namespace GameEngineX.Game.GameObjects.GameObjectComponents {
    public class Sprite : GameObjectComponent, IRendering {

        private Renderable renderable;
        public int RenderLayer { get; set; }

        public override void Initialize() {
            this.renderable = new RectangleShape(Color.FromArgb(0, Color.White), 1, 1);
            this.RenderLayer = 0;
        }

        public Renderable Renderable {
            get => this.renderable;
            set => this.renderable = value ?? new RectangleShape(Color.FromArgb(0, Color.White), 1, 1);
        }

    }
}
