using System;

namespace GameEngineX.Utility.Math {
    public class Vector2 {
        public static Vector2 ZERO => new Vector2(0, 0);
        public static Vector2 IDENTITY => new Vector2(1, 1);

        public delegate void VectorChangedEventHandler(Vector2 v, float oldX, float oldY);
        public event VectorChangedEventHandler OnChanged;

        private float x;
        private float y;

        private float? length;

        public Vector2((float x, float y) p)
            : this(p.x, p.y) { }

        public Vector2(System.Windows.Point p)
            : this((float)p.X, (float)p.Y) { }

        public Vector2()
            : this(0, 0) {
        }

        public Vector2(float x, float y) {
            Data = (x, y);
        }

        public Vector2(Vector2 v)
            : this(v.X, v.Y) {
        }

        public Vector2 Set(Vector2 v) {
            Data = (v.X, v.Y);
            return this;
        }

        public Vector2 Set(float x, float y) {
            Data = (x, y);
            return this;
        }

        public Vector2 Set((float x, float y) data) {
            Data = data;
            return this;
        }

        public Vector2 Normalize() {
            float l = Length;

            if (l == 0)
                return this;

            Data = (X / l, Y / l);

            return this;
        }

        public Vector2 Add(float s) {
            return Add(s, s);
        }

        public Vector2 Add(float x, float y) {
            Data = (X + x, Y + y);

            return this;
        }

        public Vector2 Add(Vector2 v) {
            return Add(v.X, v.Y);
        }

        public Vector2 Subtract(float x, float y) {
            Data = (X - x, Y - y);

            return this;
        }

        public Vector2 Subtract(Vector2 v) {
            return Subtract(v.X, v.Y);
        }

        public Vector2 Scale(float s) {
            return Scale(s, s);
        }

        public Vector2 Scale(float x, float y) {
            Data = (X * x, Y * y);

            return this;
        }

        public Vector2 Scale(Vector2 v) {
            return Scale(v.X, v.Y);
        }

        public Vector2 Apply(Func<int, float, float> f) {
            Data = (f(0, X), f(1, Y));

            return this;
        }

        public Vector2 Normalized {
            get {
                Vector2 v = new Vector2(this);
                v.Normalize();
                return v;
            }
        }

        public Vector2 Normal(bool up) {
            float x = Y;
            float y = X;

            if (up)
                x *= -1;
            else
                y *= -1;

            Vector2 result = new Vector2(x, y);
            result.Normalize();

            return result;
        }

        public static float AngleBetween(Vector2 v1, Vector2 v2) {
            return AngleBetween(v1.X, v1.Y, v2.X, v2.y);
        }

        public static float AngleBetween(float x1, float y1, float x2, float y2) {
            return (float)(System.Math.Atan2(y2, x2) - System.Math.Atan2(y1, x1));
        }

        public static float Dot(Vector2 v1, Vector2 v2) {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        public static float Distance(Vector2 v1, Vector2 v2) {
            float dx = v2.X - v1.X;
            float dy = v2.Y - v1.Y;

            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }

        public static float Distance(Vector2 v1, float x, float y) {
            float dx = x - v1.X;
            float dy = y - v1.Y;

            return (float)System.Math.Sqrt(dx * dx + dy * dy);
        }

        public (float x, float y) Data {
            get => (X, Y);
            set {
                float oldX = X;
                float oldY = Y;

                this.x = value.x;
                this.y = value.y;

                this.length = null;

                OnChanged?.Invoke(this, oldX, oldY);
            }
        }

        public float X {
            get => this.x;
            set => Data = (value, Y);
        }

        public float Y {
            get => this.y;
            set => Data = (X, value);
        }

        public float this[int i] {
            get {
                i %= 2;
                switch (i) {
                    case 0: return X;
                    case 1: return Y;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                i %= 2;
                switch (i) {
                    case 0: {
                            X = value;
                            return;
                        }
                    case 1: {
                            Y = value;
                            return;
                        }
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public float LengthSqr => X * X + Y * Y;

        public float Length {
            get {
                if (this.length == null)
                    this.length = (float)System.Math.Sqrt(LengthSqr);

                return (float)this.length;
            }
        }

        public override string ToString() {
            return $"[{X}, {Y}]";
        }

        public override bool Equals(object obj) {
            Vector2 vector = obj as Vector2;
            return vector != null &&
                   X == vector.X &&
                   Y == vector.Y;
        }

        public override int GetHashCode() {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public static Vector2 operator +(Vector2 v0, Vector2 v1) {
            return new Vector2(v0.X + v1.X, v0.Y + v1.Y);
        }

        public static Vector2 operator -(Vector2 v0, Vector2 v1) {
            return new Vector2(v0.X - v1.X, v0.Y - v1.Y);
        }

        public static Vector2 operator *(float s, Vector2 v) {
            return v * s;
        }

        public static Vector2 operator *(Vector2 v, float s) {
            return new Vector2(v.X * s, v.Y * s);
        }

        public static float operator *(Vector2 v0, Vector2 v1) {
            return Dot(v0, v1);
        }

        public static Vector2 operator /(Vector2 v, float s) {
            return new Vector2(v.X / s, v.Y / s);
        }

        public static bool operator ==(Vector2 v1, Vector2 v2) {
            return ReferenceEquals(null, v1) ? ReferenceEquals(null, v2) : v1.Equals(v2);
        }

        public static bool operator !=(Vector2 v1, Vector2 v2) {
            return !(v1 == v2);
        }

    }
}
