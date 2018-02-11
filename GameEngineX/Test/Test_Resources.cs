using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using GameEngineX.Resources;
using GameEngineX.Resources.ResourceLoaders;

namespace Test {
    public static class Test_Resources {

        static void Main(string[] args) {

            GenerateLargeTextFile("large.txt");

            Console.ReadKey();
        }

        private static void GenerateLargeTextFile(string path) {
            StringBuilder sB = new StringBuilder();
            for (int i = 0; i < 10250; i++) {
                sB.Append("A");
            }
            string longLine = sB.ToString();

            using (FileStream fS = new FileStream(path, FileMode.Create)) {
                TextWriter tW = new StreamWriter(fS);

                for (int i = 0; i < 2000; i++) {
                    tW.WriteLine(longLine);
                }

            }
        }

        private static double LoadTimeMeasurement(string path, int executions) {
            Stopwatch sW = new Stopwatch();

            long t = 0;
            for (int i = 0; i < executions; i++) {
                sW.Restart();
                File.ReadAllLines(path);
                sW.Stop();
                t += sW.ElapsedMilliseconds;
                Console.WriteLine(i);
            }

            return t / (double)executions;
        }

    }
}
