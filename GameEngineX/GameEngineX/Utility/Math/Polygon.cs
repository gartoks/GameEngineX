using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GameEngineX.Application.Logging;

namespace GameEngineX.Utility.Math {
    public class Polygon {

        public static Polygon EmptyPolygon() {
        return new Polygon(false);
        }

        public static Polygon PolygonFromPie(int resolution, float radius, float startAngle, float sweepAngle) {
            float sA_rad = MathUtility.DegToRadf * startAngle;
            float swA_rad = MathUtility.DegToRadf * sweepAngle;
            float u = swA_rad * radius;
            int sections = (int)(u / resolution + 1);
            float angleSection = swA_rad / sections;

            Vector2[] points = new Vector2[sections + 2];

            points[0] = new Vector2(0, 0);
            for (int i = 0; i <= sections; i++) {
                float x = (float)(radius * System.Math.Cos(sA_rad + i * angleSection));
                float y = (float)(radius * System.Math.Sin(sA_rad + i * angleSection));

                points[i + 1] = new Vector2(x, y);
            }

            float offsetX = radius * (float)System.Math.Cos(sA_rad + swA_rad / 2f) / 2f;
            float offsetY = radius * (float)System.Math.Sin(sA_rad + swA_rad / 2f) / 2f;

            for (int i = 0; i < points.Length; i++) {
                points[i].Subtract(offsetX, offsetY);
            }

            return new Polygon(false, points);
        }

        public static Polygon PolygonFromCircle(int resolution, float radius) {
            return PolygonFromEllipse(resolution, radius, radius);
        }

        public static Polygon PolygonFromEllipse(int resolution, float horizontalRadius, float verticalRadius) {
            float u = (float)(2f * System.Math.PI * System.Math.Sqrt(0.5 * (horizontalRadius * horizontalRadius + verticalRadius * verticalRadius)));
            int sections = (int)(u / resolution + 1);
            float angleSection = (float)(2f * System.Math.PI / sections);

            Vector2[] points = new Vector2[sections];
            for (int i = 0; i < sections; i++) {
                float x = (float)(horizontalRadius * System.Math.Cos(i * angleSection));
                float y = (float)(verticalRadius * System.Math.Sin(i * angleSection));

                points[i] = new Vector2(x, y);
            }

            return new Polygon(false, points);
        }

        public static Polygon PolygonFromRect(float width, float height) {
            Vector2 p0 = new Vector2(-width / 2f, -height / 2f);
            Vector2 p1 = new Vector2(+width / 2f, -height / 2f);
            Vector2 p2 = new Vector2(+width / 2f, +height / 2f);
            Vector2 p3 = new Vector2(-width / 2f, +height / 2f);

            return new Polygon(false, p0, p1, p2, p3);
        }

        private Vector2 center;
        private readonly Vector2[] points;
        private readonly Vector2[] edges;

        private Rect boundingRect;
        private float area;

        public Polygon(bool clockwise, params Vector2[] points) {
            if (clockwise) {
                IEnumerable<Vector2> rev = points.Reverse();
                this.points = new Vector2[points.Length];
                for (int i = 0; i < points.Length; i++)
                    this.points[i] = rev.ElementAt(i);
            } else
                this.points = points;

            edges = new Vector2[points.Length];

            // TODO check for self intersection

            RecalculateData();
        }

        private void RecalculateData() {
            RecalculateEdges();
            RecalculateBoundingRect();
            RecalculateCenter();

            area = System.Math.Abs(MathUtility.CalculateArea(points));

            if (!IsConvex()) {
                Log.WriteLine("Polygon is not convex.");
                return;
            }
        }

        private void RecalculateEdges() {
            for (int i = 0; i < points.Length; i++) {
                Vector2 p0 = points[i];
                Vector2 p1 = points[(i + 1) % points.Length];

                edges[i] = new Vector2(p1.X - p0.X, p1.Y - p0.Y);
            }
        }

        private void RecalculateBoundingRect() {
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for (int i = 0; i < points.Length; i++) {
                if (points[i].X < minX)
                    minX = points[i].X;
                if (points[i].X > maxX)
                    maxX = points[i].X;
                if (points[i].Y < minY)
                    minY = points[i].Y;
                if (points[i].Y > maxY)
                    maxY = points[i].Y;
            }

            if (points.Length != 0)
                boundingRect = new Rect(minX, maxY, maxX - minX, maxY - minY);
            else
                boundingRect = new Rect(0, 0, 0, 0);
        }

        private void RecalculateCenter() {
            float totalX = 0;
            float totalY = 0;
            foreach (Vector2 p in points) {
                totalX += p.X;
                totalY += p.Y;
            }

            center = new Vector2(totalX / points.Length, totalY / points.Length);
        }

        public Polygon Scale(float scale) {
            Vector2 center = Center;

            for (int i = 0; i < points.Length; i++) {
                Vector2 p = points[i];

                Vector2 dir = p - center;
                dir.Scale(scale);

                p = center + dir;

                points[i] = p;
            }

            RecalculateData();

            return this;
        }

        public bool Contains(Vector2 p) {
            return Contains(p.X, p.Y);
        }

        public bool Contains(float x, float y) {
            if (!BoundingRect.Contains(x, y))
                return false;

            int numPoints = points.Length;

            bool contains = false;
            for (int i = 0; i < numPoints; i++) {
                Vector2 p = points[i];
                Vector2 p1 = points[(i + 1) % numPoints];

                if ((p.Y <= y && y < p1.Y || p1.Y <= y && y < p.Y) && x < (p1.X - p.X) / (p1.Y - p.Y) * (y - p.Y) + p.X)
                    contains = !contains;
            }

            return contains;
        }

        public bool Contains(Polygon p) {
            for (int i = 0; i < p.Points.Count(); i++) {
                if (!Contains(p.Points.ElementAt(i)))
                    return false;
            }

            return true;
        }

        //public IEnumerable<Polygon> Decompose() {
        //    if (IsConvex())
        //        return new[] {this};

        // TODO
        //}

        private bool IsConvexVertex(int vertexIndex) {
            vertexIndex = vertexIndex % points.Length;

            Vector2 point = points[vertexIndex];
            Vector2 prevPoint = points[PrevIndex(vertexIndex)];
            Vector2 nextPoint = points[NextIndex(vertexIndex)];

            float area = MathUtility.CalculateArea(new[] { prevPoint, point, nextPoint });

            if (area < 0)
                return true;

            if (area > 0)
                return false;

            throw new ArgumentException();
        }

        //private bool IsEarVertex(int vertexIndex) {

        //}

        public IEnumerable<Vector2> Points => points;

        public IEnumerable<Vector2> Edges => edges;

        public Rect BoundingRect => this.boundingRect;

        public Vector2 Center => center;

        public float Area => area;

        public override bool Equals(object obj) {
            if (obj == null)
                return false;

            if (!(obj is Polygon p))
                return false;

            if (Points.Count() != p.Points.Count())
                return false;

            int startIndex = 0;
            while (startIndex < this.points.Length && !p.points[startIndex].Equals(this.points[0]))
                startIndex++;

            if (startIndex == this.points.Length)
                return false;

            for (int i = 0; i < this.points.Length; i++) {
                Vector2 tp = this.points[i];
                Vector2 op = p.points[(startIndex + i) % this.points.Length];

                if (!tp.Equals(op))
                    return false;
            }

            return true;
        }

        public override int GetHashCode() => -501195594 + EqualityComparer<Vector2[]>.Default.GetHashCode(this.points);

        private bool IsConvex() {
            int numPoints = points.Length;

            if (numPoints < 4)
                return true;

            bool hasNegTurn = false;
            bool hasPosTurn = false;
            for (int i = 0; i < numPoints; i++) {
                int i1 = (i + 1) % numPoints;
                int i2 = (i1 + 1) % numPoints;

                Vector2 p = points[i];
                Vector2 p1 = points[i1];
                Vector2 p2 = points[i2];

                float xProdLen = (p.X - p1.X) * (p.Y - p1.Y) - (p2.X - p1.X) * (p2.Y - p1.Y);

                if (xProdLen < 0)
                    hasNegTurn = true;
                else if (xProdLen > 0)
                    hasPosTurn = true;

                if (hasNegTurn && hasPosTurn)
                    return false;
            }

            return true;
        }

        private bool EdgeIntersectsLine(float x1, float y1, float x2, float y2) {
            float d1x = x2 - x1;
            float d1y = y2 - y1;

            int prevIndex = points.Length - 1;
            for (int i = 0; i < points.Length; i++) {
                Vector2 prevPoint = points[prevIndex];
                Vector2 curPoint = points[i];

                float x3 = prevPoint.X;
                float y3 = prevPoint.Y;

                float d3x = curPoint.X - x3;
                float d3y = curPoint.Y - y3;

                (float t1, float t2) = MathUtility.LineIntersectionPoints(x1, y1, d1x, d1y, x3, y3, d3x, d3y);

                if (t1 >= 0 || t1 <= 1 || t2 <= 0 || t2 >= 1)
                    return true;

                prevIndex = i;
            }

            return false;
        }

        private int NextIndex(int i) {
            return ValidateIndex(i + 1);
        }

        private int PrevIndex(int i) {
            i = ValidateIndex(i + 1);

            if (i == 0)
                return points.Length - 1;
            return i - 1;
        }

        private int ValidateIndex(int i) {
            while (i < 0)
                i += points.Length;

            return i % points.Length;
        }
    }
}
