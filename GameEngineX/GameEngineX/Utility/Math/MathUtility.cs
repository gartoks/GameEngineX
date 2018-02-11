using System;
using System.Collections.Generic;

namespace GameEngineX.Utility.Math {
    public static class MathUtility {

        public const float E = 2.7182818284590451f;
        public const float PI = 3.1415926535897931f;
        public const float TwoPIf = 2.0f * PI;
        public const float HalfPIf = 0.5f * PI;
        public const float QuarterPIf = 0.25f * PI;
        public const float ThreeHalfPIf = 1.5f * PI;

        public const double TwoPI = 2.0 * System.Math.PI;
        public const double HalfPI = 0.5 * System.Math.PI;
        public const double QuarterPI = 0.25 * System.Math.PI;
        public const double ThreeHalfPI = 1.5 * System.Math.PI;

        public const float RadToDegf = 180f / PI;
        public const float DegToRadf = PI / 180f;

        public const double RadToDeg = 180.0 / System.Math.PI;
        public const double DegToRad = System.Math.PI / 180.0;

        public static int Clamp(int v, int min, int max) {
            return v > max ? max : (v < min ? min : v);
        }

        public static void Clamp(ref int v, int min, int max) {
            v = Clamp(v, min, max);
        }

        public static double Clamp(double v, double min, double max) {
            return v > max ? max : (v < min ? min : v);
        }

        public static void Clamp(ref double v, double min, double max) {
            v = Clamp(v, min, max);
        }

        public static double Clamp01(double v) {
            return Clamp(v, 0, 1);
        }

        public static void Clamp01(ref double v) {
            v = Clamp01(v);
        }

        public static float Clamp(float v, float min, float max) {
            return v > max ? max : (v < min ? min : v);
        }

        public static void Clamp(ref float v, float min, float max) {
            v = Clamp(v, min, max);
        }
        public static float Clamp01(float v) {
            return Clamp(v, 0, 1);
        }

        public static void Clamp01(ref float v) {
            v = Clamp01(v);
        }

        public static int FloorToInt(this double v) {
            return (v > 0) ? ((int)v) : (((int)v) - 1);
        }

        public static int CeilToInt(this double v) {
            return v > 0 ? ((int)v) + 1 : (int)v;
        }

        public static int RoundToInt(this double v) {
            return FloorToInt(v + 0.5);
        }

        public static int Mod(this int v, int m) {
            int a = v % m;
            return a < 0 ? a + m : a;
        }

        public static bool IsInCircle(double x, double y, double xOrigin, double yOrigin, double radius, bool allowOnEdge = true) {
            double c_x = x - xOrigin;
            double c_y = y - yOrigin;

            double radius2 = radius * radius;
            double len2 = c_x * c_x + c_y * c_y;

            return allowOnEdge && len2 <= radius2 || !allowOnEdge && len2 < radius2;
        }

        public static double DistanceToCircle(double x, double y, double xOrigin, double yOrigin, double radius) {
            (double x, double y) dir = (x - xOrigin, y - yOrigin);
            return dir.Length() - radius;
        }

        public static IEnumerable<(double x, double y)> CircleRectangleIntersectionPoints((double x, double y, double width, double height) r, (double x, double y) center, double radius) {
            (double x, double y) p0 = (r.x, r.y);
            (double x, double y) p1 = (r.x + r.width, r.y);
            (double x, double y) p2 = (r.x + r.width, r.y + r.height);
            (double x, double y) p3 = (r.x, r.y + r.height);

            List<(double x, double y)> ret = new List<(double x, double y)>();
            ret.AddRange(CircleLineIntersectionPoints(p0, p1, center, radius));
            ret.AddRange(CircleLineIntersectionPoints(p1, p2, center, radius));
            ret.AddRange(CircleLineIntersectionPoints(p2, p3, center, radius));
            ret.AddRange(CircleLineIntersectionPoints(p3, p0, center, radius));

            return ret;
        }

        public static IEnumerable<(double x, double y)> CircleLineIntersectionPoints((double x, double y) p0, (double x, double y) p1, (double x, double y) center, double radius) {
            (double x, double y) p1p0 = p1.Subtract(p0);
            (double x, double y) cp0 = center.Subtract(p0);

            double a = p1p0.LengthSq();
            double bBy2 = p1p0.x * cp0.x + p1p0.y * cp0.y;
            double c = cp0.LengthSq() - radius * radius;

            double pBy2 = bBy2 / a;
            double q = c / a;

            double disc = pBy2 * pBy2 - q;
            if (disc < 0)
                return new(double, double)[0];

            double tmpSqrt = System.Math.Sqrt(disc);
            double p0p1ScalingFactor1 = -pBy2 + tmpSqrt;
            double p0p1ScalingFactor2 = -pBy2 - tmpSqrt;

            List<(double x, double y)> ret = new List<(double x, double y)>();

            (double x, double y) ret0 = (p0.x - p1p0.x * p0p1ScalingFactor1, p0.y - p1p0.y * p0p1ScalingFactor1);
            if (IsInCircle(ret0.x, ret0.y, center.x, center.y, radius))
                ret.Add(ret0);

            if (disc == 0)
                return ret;

            (double x, double y) ret1 = (p0.x - p1p0.x * p0p1ScalingFactor2, p0.y - p1p0.y * p0p1ScalingFactor2);
            if (IsInCircle(ret1.x, ret1.y, center.x, center.y, radius))
                ret.Add(ret1);

            return ret;
        }

        public static bool RectangleContains(this (float x, float y, float width, float height) rect, float x, float y, float radius = 0) {
            return x + radius >= rect.x && x - radius <= rect.x + rect.width && y + radius >= rect.y && y - radius <= rect.y + rect.height;
        }

        public static (float x, float y, float width, float height) Expand(this (float x, float y, float width, float height) bounds, float left, float right, float top, float bottom) {
            return (bounds.x - left, bounds.y - bottom, bounds.width + left + right, bounds.height + top + bottom);
        }

        public static bool Intersects(this (float x, float y, float width, float height) bounds, float x, float y, float radius) {
            if (x >= bounds.x && x <= bounds.x + bounds.width && y >= bounds.y && y <= bounds.y + bounds.height)
                return true;

            bool intersectsX = !(x + radius < bounds.x) && !(x - radius > bounds.x + bounds.width);
            bool intersectsY = !(y + radius < bounds.y) && !(y - radius > bounds.y + bounds.height);
            float r2 = radius * radius;
            if (intersectsX && intersectsY) {
                float tmpX = x - bounds.x;
                float tmpY = y - bounds.y;
                if (tmpX * tmpX + tmpY * tmpY < r2)
                    return true;

                tmpX = x - (bounds.x + bounds.width);
                tmpY = y - bounds.y;
                if (tmpX * tmpX + tmpY * tmpY < r2)
                    return true;

                tmpX = x - bounds.x;
                tmpY = y - (bounds.y + bounds.height);
                if (tmpX * tmpX + tmpY * tmpY < r2)
                    return true;

                tmpX = x - (bounds.x + bounds.width);
                tmpY = y - (bounds.y + bounds.height);
                if (tmpX * tmpX + tmpY * tmpY < r2)
                    return true;
            }

            return false;
        }

        public static (double t1, double t2) LineIntersectionPoints(double p1x, double p1y, double d1x, double d1y, double p2x, double p2y, double d2x, double d2y) {
            if ((d1x == 0 && d1y == 0) || (d2x == 0 && d2y == 0)) {
                throw new ArgumentException("a direction vector cannot be null.");
            }

            double t1 = ((p1x - p2x) * d2y - (p1y - p2y) * d2x) / (d1y * d2x - d1x * d2y);

            double t2;
            if (d2x != 0)
                t2 = (p1x - p2x + t1 * d1x) / d2x;
            else
                t2 = (p1y - p2y + t1 * d1y) / d2y;

            return (t1, t2);
        }

        public static (float t1, float t2) LineIntersectionPoints(float p1x, float p1y, float d1x, float d1y, float p2x, float p2y, float d2x, float d2y) {
            if ((d1x == 0 && d1y == 0) || (d2x == 0 && d2y == 0)) {
                throw new ArgumentException("a direction vector cannot be null.");
            }

            float t1 = ((p1x - p2x) * d2y - (p1y - p2y) * d2x) / (d1y * d2x - d1x * d2y);

            float t2;
            if (d2x != 0)
                t2 = (p1x - p2x + t1 * d1x) / d2x;
            else
                t2 = (p1y - p2y + t1 * d1y) / d2y;

            return (t1, t2);
        }

        public static double NormalDistributionProbabilityDensity(double x, double mean, double stdDeviation) {
            double a = (x - mean) * (x - mean);
            double b = 2 * stdDeviation * stdDeviation;
            return (1.0 / System.Math.Sqrt(b * System.Math.PI)) * System.Math.Exp(-a / b);
        }

        public static (double x, double y) ToCarthesian(this (double radius, double angle) v) {
            return PolarToCarthesianCoordinates(v.radius, v.angle);
        }

        public static (double x, double y) PolarToCarthesianCoordinates(double radius, double angle) {
            return (radius * System.Math.Cos(angle), radius * System.Math.Sin(angle));
        }

        public static (double radius, double angle) ToPolar(this (double x, double y) v) {
            return CarthesianToPolarCoordinates(v.x, v.y);
        }

        public static (double radius, double angle) CarthesianToPolarCoordinates(double x, double y) {
            if (x == 0 && y == 0)
                throw new ArgumentOutOfRangeException();

            double r = System.Math.Sqrt(x * x + y * y);

            double angle;
            if (x == 0 && y < 0)
                angle = ThreeHalfPI;
            else if (x == 0 && y > 0)
                angle = HalfPI;
            else
                angle = System.Math.Atan2(y, x);

            NormalizeAngle(ref angle);

            return (r, angle);
        }

        public static (float x, float y) Add(this (float x, float y) v0, (float x, float y) v1) {
            return (v0.x + v1.x, v0.y + v1.y);
        }

        public static (double x, double y) Subtract(this (double x, double y) v0, (double x, double y) v1) {
            return (v0.x - v1.x, v0.y - v1.y);
        }

        public static double Length(this (double x, double y) v) {
            return System.Math.Sqrt(v.LengthSq());
        }

        public static double LengthSq(this (double x, double y) v) {
            return v.x * v.x + v.y * v.y;
        }

        public static double AngleBetween(this (double x, double y) v, (double x, double y) v1) {
            return v1.Angle() - v.Angle();
        }

        public static double Angle(this (double x, double y) w) {
            (double x, double y) v = (w.x, w.y);

            if (System.Math.Abs(v.x) < 0.0001)
                v.x = 0;

            if (System.Math.Abs(v.y) < 0.0001)
                v.y = 0;

            double r = v.Length();

            if (v.x == 0 && v.y < 0)
                return ThreeHalfPI;
            if (v.x == 0 && v.y > 0)
                return HalfPI;
            if (v.x > 0 && v.y == 0)
                return 0;
            else if (v.x < 0 && v.y == 0)
                return System.Math.PI;

            double a;
            a = v.x != 0 ? System.Math.Acos(v.x / r) : System.Math.Asin(v.y / r);

            a = System.Math.Abs(a);
            if (v.y < 0)
                a = TwoPI - a;

            NormalizeAngle(ref a);

            return a;
        }

        public static double NormalizeAngle(this double a) {
            while (a < 0)
                a += TwoPI;

            if (a >= TwoPI)
                a = a % TwoPI;

            return a;
        }

        public static void NormalizeAngle(ref double a) {
            while (a < 0)
                a += TwoPI;

            if (a >= TwoPI)
                a = a % TwoPI;
        }

        public static string ToRoman(int number) {
            if ((number < 0) || (number > 3999))
                throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1)
                return string.Empty;
            if (number >= 1000)
                return "M" + ToRoman(number - 1000);
            if (number >= 900)
                return "CM" + ToRoman(number - 900); //EDIT: i've typed 400 instead 900
            if (number >= 500)
                return "D" + ToRoman(number - 500);
            if (number >= 400)
                return "CD" + ToRoman(number - 400);
            if (number >= 100)
                return "C" + ToRoman(number - 100);
            if (number >= 90)
                return "XC" + ToRoman(number - 90);
            if (number >= 50)
                return "L" + ToRoman(number - 50);
            if (number >= 40)
                return "XL" + ToRoman(number - 40);
            if (number >= 10)
                return "X" + ToRoman(number - 10);
            if (number >= 9)
                return "IX" + ToRoman(number - 9);
            if (number >= 5)
                return "V" + ToRoman(number - 5);
            if (number >= 4)
                return "IV" + ToRoman(number - 4);
            if (number >= 1)
                return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }

        #region RangomExtensions
        public static double NextDouble(this Random rand, double min, double max) {
            return min + rand.NextDouble() * (max - min);
        }

        public static double NextGaussian(this Random rand, double mean = 0, double stdDev = 1) {
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();
            double randStdNormal = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        public static double NextAngle(this Random rand) {
            return rand.NextDouble() * 2.0 * System.Math.PI;
        }

        public static void NextRandomInCircleUniformly(this Random rand, double radius, out double x, out double y) {
            double angle = rand.NextAngle();
            double r = System.Math.Sqrt(rand.NextDouble()) * radius;
            x = r * System.Math.Cos(angle);
            y = r * System.Math.Sin(angle);
        }

        public static void NextRandomInCircleCentered(this Random rand, double radius, out double x, out double y) {
            double angle = rand.NextAngle();
            double r = rand.NextDouble() * radius;
            x = r * System.Math.Cos(angle);
            y = r * System.Math.Sin(angle);
        }
        #endregion

        #region ArrayExtensions
        public static float[] Flatten(this float[,] array) {
            int l0 = array.GetLength(0);
            int l1 = array.GetLength(1);
            float[] result = new float[l0 * l1];

            for (int i = 0; i < l0; i++) {
                for (int j = 0; j < l1; j++) {
                    result[i + j * l0] = array[i, j];
                }
            }

            return result;
        }

        public static float[] Copy(this float[] array) {
            float[] copy = new float[array.Length];

            Buffer.BlockCopy(array, 0, copy, 0, array.Length);

            return copy;
        }

        public static float[] Fill(this float[] array, float v) {
            for (int i = 0; i < array.Length; i++) {
                array[i] = v;
            }

            return array;
        }

        public static float[] Fill(this float[] array, Func<int, float, float> f) {
            for (int i = 0; i < array.Length; i++) {
                array[i] = f(i, array[i]);
            }

            return array;
        }

        public static float Get2D(this float[] array, int width, int x, int y) {
            int idx = x + y * width;

            return array[idx];
        }

        public static float Get3D(this float[] array, int width, int height, int x, int y, int z) {
            int idx = x + (y + z * height) * width;

            return array[idx];
        }

        public static void Set2D(this float[] array, int width, int x, int y, float value) {
            int idx = x + y * width;

            array[idx] = value;
        }

        public static void Set3D(this float[] array, int width, int height, int x, int y, int z, float value) {
            int idx = x + (y + z * height) * width;

            array[idx] = value;
        }

        public static void Apply2D(this float[] array, int width, int x, int y, float value, Func<float, float, float> function) {
            int idx = x + y * width;

            array[idx] = function(array[idx], value);
        }

        public static void Apply3D(this float[] array, int width, int height, int x, int y, int z, float value, Func<float, float, float> function) {
            int idx = x + (y + z * height) * width;

            array[idx] = function(array[idx], value);
        }

        public static float[] Get2DRow(this float[] array, int width, int row) {
            int startIdx = width * row * sizeof(float);
            int len = sizeof(float) * width;

            float[] rowData = new float[width];
            Buffer.BlockCopy(array, startIdx, rowData, 0, len);

            return rowData;
        }

        public static float[] Get2DColumn(this float[] array, int width, int column) {

            int height = array.Length / width;

            float[] columnData = new float[height];
            for (int h = 0; h < height; h++) {
                columnData[h] = array[h * width + column];
            }

            return columnData;
        }
        #endregion
    }
}