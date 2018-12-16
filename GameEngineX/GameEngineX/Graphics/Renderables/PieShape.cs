using System.Drawing;

namespace GameEngineX.Graphics.Renderables {
    public class PieShape : Renderable {
        public float Radius;
        public float StartAngle;
        public float SweepAngle;

        public PieShape(Color color, float radius, float startAngle, float sweepAngle)
            : this(color, -1, radius, startAngle, sweepAngle) {
        }

        public PieShape(Color color, float size, float radius, float startAngle, float sweepAngle)
            : base(color, size) {

            Radius = radius;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
        }

        internal override void Render(int renderLayer, Renderer renderer) {
            if (IsFilled)
                renderer.FillPie(renderLayer, Color, 0, 0, Radius, StartAngle, SweepAngle);
            else
                renderer.DrawRing(renderLayer, Color, Size, 0, 0, Radius, Radius, StartAngle, SweepAngle);
        }

    }
}