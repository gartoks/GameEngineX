using System.Drawing;

namespace GameEngineX.Graphics {
    public abstract class Renderable {
        public Color Color;
        public float Size;

        protected Renderable(Color color)
            : this(color, -1) { }

        protected Renderable(Color color, float size) {
            Color = color;
            Size = size;
        }

        internal abstract void Render(int renderLayer, Renderer renderer);

        public bool IsFilled => Size <= 0;
    }
}
