using System;

namespace GameEngineX.Utility.Math {
    public class Rectangle {
        public float X;
        public float Y;
        private float width;
        private float height;

        public Rectangle(float x, float y, float width, float height) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Vector2 position, Vector2 size)
            : this(position.X, position.Y, size.X, size.Y) { }

        public float Width {
            get => this.width;
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "A rectangle width must be bigger than zero.");

                this.width = value;
            }
        }

        public float Height {
            get => this.height;
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "A rectangle height must be bigger than zero.");

                this.height = value;
            }
        }

        public float Top => Y + Height;

        public float Bottom => Y;

        public float Left => X;

        public float Right => X + Width;

        public float CenterX => X + Width / 2f;

        public float CenterY => Y + Height / 2f;

        public Vector2 Center => new Vector2(X + Width / 2f, Y + Height / 2f);

        public Vector2 TopLeft => new Vector2(Left, Top);

        public Vector2 TopRight => new Vector2(Right, Top);

        public Vector2 BottomLeft => new Vector2(Left, Bottom);

        public Vector2 BottomRight => new Vector2(Right, Bottom);

        public void Scale(float sx, float sy) {
            float w = Width * sx;
            float h = Height * sy;

            X += (Width - w) / 2f;
            Y += (Height - h) / 2f;

            Width = w;
            Height = h;
        }

        public void Translate(float dx, float dy) {
            X += dx;
            Y += dy;
        }

        public bool Contains(float x, float y, float radius = 0) {
            return x + radius >= Left && x - radius <= Right && y + radius >= Bottom && y - radius <= Top;
        }

        public bool Contains(Vector2 p, float radius = 0) => Contains(p.X, p.Y, radius);

        public bool Contains(Rectangle r) {
            return Contains(TopLeft) && Contains(TopRight) && Contains(BottomLeft) && Contains(BottomRight);
        }

        public bool Intersects(Rectangle r, bool includeContains) {
            bool ctl = Contains(TopLeft);
            bool ctr = Contains(TopRight);
            bool cbl = Contains(BottomLeft);
            bool cbr = Contains(BottomRight);

            int containedPoints = 0;
            if (ctl) containedPoints++;
            if (ctr) containedPoints++;
            if (cbl) containedPoints++;
            if (cbr) containedPoints++;

            return containedPoints == 1 || containedPoints == 2 || (includeContains && containedPoints == 4);
        }

        public override string ToString() {
            return $"({Left}, {Top}, {Right}, {Bottom})";
        }
    }
}