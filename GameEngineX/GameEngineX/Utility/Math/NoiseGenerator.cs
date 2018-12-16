using System;
using System.Linq;

namespace GameEngineX.Utility.Math {
    public class NoiseGenerator {
        static NoiseGenerator() {
            PERMUTATION = new int[256];
            for (int i = 0; i < 256; i++) {
                PERMUTATION[i] = i;
            }
        }
        private static readonly int[] PERMUTATION;

        public readonly int Seed;
        public readonly int Repeat;

        private readonly int[] Permutation;

        public NoiseGenerator(int seed, int repeat = 0) {
            Seed = seed;
            Repeat = repeat;

            Random random = new Random(seed);

            Permutation = new int[512];
            int[] perm = PERMUTATION.OrderBy(p => random.Next()).ToArray();
            for (int i = 0; i < 512; i++) {
                Permutation[i] = perm[i % 256];
            }
        }

        public double RandomNoise2D(double x, double y) {
            if (Repeat > 0) {
                x %= Repeat;
                y %= Repeat;
            }

            int xi = (int)x & 255;
            int yi = (int)y & 255;

            int g0 = Permutation[Permutation[xi] + yi];
            int g1 = Permutation[Permutation[xi + 1] + yi];
            int g2 = Permutation[Permutation[xi] + yi + 1];
            int g3 = Permutation[Permutation[xi + 1] + yi + 1];

            double v = g0 + g1 + g2 + g3;
            v /= 4;

            return v / 255.0;
        }

        public double PerlinNoise2D(double x, double y/*, double gridWidth = 1, double gridHeight = 1*/) {
            if (Repeat > 0) {
                x %= Repeat;
                y %= Repeat;
            }

            int xi = (int)x & 255;
            int yi = (int)y & 255;
            double xf = x - (int)x;
            double yf = y - (int)y;

            int g0 = Permutation[Permutation[xi] + yi];
            int g1 = Permutation[Permutation[xi + 1] + yi];
            int g2 = Permutation[Permutation[xi] + yi + 1];
            int g3 = Permutation[Permutation[xi + 1] + yi + 1];

            double xFade = Fade(xf);
            double yFade = Fade(yf);

            double d0 = Gradient(g0, xf, yf);
            double d1 = Gradient(g1, xf - 1, yf);
            double d2 = Gradient(g2, xf, yf - 1);
            double d3 = Gradient(g3, xf - 1, yf - 1);

            double xInterpolated_0 = Lerp(xFade, d0, d1);
            double xInterpolated_1 = Lerp(xFade, d2, d3);

            return 0.5 * Lerp(yFade, xInterpolated_0, xInterpolated_1) + 0.5;
        }

        public double NoiseMaxima2D(double x, double y, double stepSizeX, double stepSizeY, bool moore) {
            double value = PerlinNoise2D(x, y);

            for (int yi = -1; yi <= 1; yi++) {
                for (int xi = -1; xi <= 1; xi++) {
                    if (xi == 0 && yi == 0)
                        continue;

                    double xc = x + xi * stepSizeX;
                    double yc = y + yi * stepSizeY;

                    if (!moore && xi != 0 && yi != 0)
                        continue;

                    double v = PerlinNoise2D(xc, yc);

                    if (v > value)
                        return 0;
                }
            }

            return value;
        }

        public double PerlinNoise3D(double x, double y, double z/*, double gridWidth = 1, double gridHeight = 1*/) {
            if (Repeat > 0) {
                x %= Repeat;
                y %= Repeat;
                z %= Repeat;
            }

            int xi = (int)x & 255;
            int yi = (int)y & 255;
            int zi = (int)z & 255;
            double xf = x - (int)x;
            double yf = y - (int)y;
            double zf = z - (int)z;
            double xFade = Fade(xf);
            double yFade = Fade(yf);
            double zFade = Fade(zf);

            int a = Permutation[xi] + yi;
            int aa = Permutation[a] + zi;
            int ab = Permutation[a + 1] + zi;
            int b = Permutation[xi + 1] + yi;
            int ba = Permutation[b] + zi;
            int bb = Permutation[b + 1] + zi;

            int g0 = Permutation[aa];
            int g1 = Permutation[ba];
            int g2 = Permutation[ab];
            int g3 = Permutation[bb];
            int g4 = Permutation[aa + 1];
            int g5 = Permutation[ba + 1];
            int g6 = Permutation[ab + 1];
            int g7 = Permutation[bb + 1];

            double d0 = Gradient(g0, x, y, z);
            double d1 = Gradient(g1, x - 1, y, z);
            double d2 = Gradient(g2, x, y - 1, z);
            double d3 = Gradient(g3, x - 1, y - 1, z);
            double d4 = Gradient(g4, x, y, z - 1);
            double d5 = Gradient(g5, x - 1, y, z - 1);
            double d6 = Gradient(g6, x, y - 1, z - 1);
            double d7 = Gradient(g7, x - 1, y - 1, z - 1);

            double xInterp_0 = Lerp(xFade, d0, d1);
            double xInterp_1 = Lerp(xFade, d2, d3);
            double xInterp_2 = Lerp(xFade, d4, d5);
            double xInterp_3 = Lerp(xFade, d6, d7);
            double yInterp_0 = Lerp(yFade, xInterp_0, xInterp_1);
            double yInterp_1 = Lerp(yFade, xInterp_2, xInterp_3);
            double zInterp_0 = Lerp(zFade, yInterp_0, yInterp_1);

            return zInterp_0;
        }

        private double Lerp(double t, double v0, double v1) {
            return v0 + t * (v1 - v0);
        }

        private static double Gradient(int hash, double x, double y) {
            switch (hash & 3) {
                case 0: return x + y;
                case 1: return -x + y;
                case 2: return x - y;
                case 3: return -x - y;
                default: return 0;
            }
        }

        private static double Gradient(int hash, double x, double y, double z) {
            int h = hash & 15;
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        private static double Fade(double t) {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
    }
}
