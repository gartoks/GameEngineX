using System.Drawing;

namespace GameEngineX.Graphics.Renderables {
    public class EllipseShape : Renderable {

        public float HorizontalRadius;
        public float VerticalRadius;

        public EllipseShape(Color color, float horizontalRadius, float verticalRadius)
            : this(color, -1, horizontalRadius, verticalRadius) {
        }

        public EllipseShape(Color color, float size, float horizontalRadius, float verticalRadius)
            : base(color, size) {

            HorizontalRadius = horizontalRadius;
            VerticalRadius = verticalRadius;
        }

        internal override void Render(int renderLayer, Renderer renderer) {
            if (IsFilled)
                renderer.FillEllipse(renderLayer, Color, 0, 0, HorizontalRadius, VerticalRadius);
            else
                renderer.DrawEllipse(renderLayer, Color, Size, 0, 0, HorizontalRadius, VerticalRadius);
        }
    }
}