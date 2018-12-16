using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using GameEngineX.Game.GameObjects;

namespace TestGame.FunctionPlotting {
    public static class FunctionPlotBuilder {

        public static FunctionPlotter BuildFromData(string filePath, GameObject gO) {
            string[] lines = File.ReadAllLines(filePath);
            string[] columnNames = lines[0].Split(',');
            string[] metaData = lines[1].Split(',');

            int columns = columnNames.Length;
            int rows = lines.Length - 2;

            float stepSize = float.Parse(metaData[0]) / 10.0f;
            float start = float.Parse(metaData[1]) / 10f;
            float end = start + rows * stepSize;

            float[,] functionData = new float[columns, rows];
            float minY = float.MaxValue;
            float maxY = float.MaxValue;
            float[,] functionDataBounds = new float[columns, 2];
            for (int i = 0; i < columns; i++) {
                functionDataBounds[i, 0] = float.MaxValue;
                functionDataBounds[i, 1] = float.MinValue;
            }

            for (int row = 2; row < lines.Length; row++) {
                string[] rowData = lines[row].Split(',');

                for (int column = 0; column < columns; column++) {
                    float functionValue = float.Parse(rowData[column]);

                    functionData[column, row - 2] = functionValue;

                    if (functionValue < functionDataBounds[column, 0])
                        functionDataBounds[column, 0] = functionValue;
                    if (functionValue > functionDataBounds[column, 1])
                        functionDataBounds[column, 1] = functionValue;
                    if (functionValue < minY)
                        minY = functionValue;
                    if (functionValue > maxY)
                        maxY = functionValue;

                    //Debug.WriteLine(column + " " + row + " " + functionValue);
                }
            }

            Random random = new Random();
            Color RandomColor() => Color.FromArgb(random.Next(255), random.Next(255), random.Next(255), 255);

            FunctionPlotter plotter = gO.AddComponent<FunctionPlotter>();
            plotter.StepSize = stepSize;
            //plotter.SetBounds(start, minY, end, maxY);

            for (int column = 0; column < columns; column++) {
                int c = column;
                Func<float, float> function = x => {
                    x = Math.Max(start, Math.Min(end, x));

                    int idx = Math.Min((int)((x - start) / stepSize), rows - 1);
                    float offset = x - idx * stepSize;
                    int nextIdx = Math.Min(idx + 1, rows - 1);
                    float functionValue0 = functionData[c, idx];
                    float functionValue1 = functionData[c, nextIdx];

                    return Interpolate(offset, functionValue0, functionValue1) / 10f;
                };

                FunctionPlotter.Function f = new FunctionPlotter.Function(function);
                plotter.AddFunction(columnNames[column], f, RandomColor());
            }

            return plotter;
        }

        private static float Interpolate(float t, float v0, float v1) => v0 + t * (v1 - v0);

    }
}