using System.Drawing;

namespace GameEngineX.Graphics.Renderables {
    public class CircleShape : EllipseShape {

        public CircleShape(Color color, float radius)
            : this(color, -1, radius) { }

        public CircleShape(Color color, float size, float radius)
            : base(color, size, radius, radius) {
        }

        public float Radius {
            get => HorizontalRadius;
            set {
                HorizontalRadius = value;
                VerticalRadius = value;
            }
        }

    }
}
