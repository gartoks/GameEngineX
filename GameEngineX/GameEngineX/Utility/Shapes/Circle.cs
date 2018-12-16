using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using GameEngineX.Game;
using GameEngineX.Game.GameObjects;
using GameEngineX.Utility.Math;

namespace GameEngineX.Utility.Shapes {
    public class Circle : Shape {
        private Vector2 center;
        private float radius;

        public Circle(Vector2 center, float radius) {
            this.center = new Vector2();

            Center = center;
            Radius = radius;
        }

        public override bool Contains(Transform t, Vector2 p) {
            Matrix tGlobalTransformationMatrix = t.GlobalTransformationMatrix;
            PointF[] points = { new PointF(p.X, -p.Y) };
            Scene.Active.MainViewport.Transform.GlobalTransformationMatrix.TransformPoints(points);
            points[0].X *= -1f;
            points[0].Y *= -1f;
            tGlobalTransformationMatrix.TransformPoints(points);

            return Vector2.DistanceSqr(Center, points[0].X, points[0].Y) <= Radius * Radius * t.Scale.X * t.Scale.X * t.Scale.Y * t.Scale.Y;
        }

        public Vector2 Center {
            get => this.center;
            set => this.center.Set(value);
        }

        public float Radius {
            get => this.radius;
            set {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this.radius = value;
            }
        }
    }
}