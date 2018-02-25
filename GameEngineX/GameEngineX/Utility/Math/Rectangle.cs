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

        public float Top => Y;

        public float Bottom => Y + Height;

        public float Left => X;

        public float Right => X + Width;

        public (float x, float y) Center => (X + Width / 2f, Y + Height / 2f);

        public (float x, float y) TopLeft => (Left, Top);

        public (float x, float y) TopRight => (Right, Top);

        public (float x, float y) BottomLeft => (Left, Bottom);

        public (float x, float y) BottomRight => (Right, Bottom);

        public bool Contains(float x, float y, float radius = 0) {
            return x + radius >= X && x - radius <= Right && y + radius >= Y && y - radius <= Bottom;
        }

        public bool Contains((float x, float y) p, float radius = 0) => Contains(p.x, p.y, radius);

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