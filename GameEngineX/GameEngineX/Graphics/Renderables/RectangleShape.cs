using System.Drawing;

namespace GameEngineX.Graphics.Renderables {
    public class RectangleShape : Renderable {

        public bool Filled;
        public float Width;
        public float Height;

        public RectangleShape(Color color, float width, float height)
            : this(color, -1, width, height) { }

        public RectangleShape(Color color, float size, float width, float height)
            : base(color, size) {

            Width = width;
            Height = height;
        }

        internal override void Render(int renderLayer, Renderer renderer) {
            if (IsFilled)
                renderer.FillCenteredRectangle(renderLayer, Color, 0, 0, Width, Height);
            else
                renderer.DrawCenteredRectangle(renderLayer, Color, Size, 0, 0, Width, Height);
        }

    }
}
