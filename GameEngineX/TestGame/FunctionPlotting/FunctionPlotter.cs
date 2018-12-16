using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Input;
using GameEngineX.Game;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Graphics;
using GameEngineX.Graphics.Renderables;
using GameEngineX.Input;
using GameEngineX.Utility.Math;
using Rectangle = GameEngineX.Utility.Math.Rectangle;

namespace TestGame.FunctionPlotting {
    public class FunctionPlotter : GameObjectComponent, IRendering {

        public delegate float Function(float x);

        public Color PlotAreaBackgroundColor;
        public Color AxisColor;
        public Color GridColor;
        private CustomShape shape;
        private float horizontalGridDistance;
        private float verticalGridDistance;
        private float minGridX;
        private int gridCountX;
        private float minGridY;
        private int gridCountY;

        private Rectangle bounds;
        private float stepSize;
        private float[] xValues;

        private Dictionary<string, (Function function, Color color, float[] values, bool active)> functions;

        public override void Initialize() {
            this.functions = new Dictionary<string, (Function function, Color color, float[] values, bool active)>();

            this.shape = new CustomShape(Render);

            PlotAreaBackgroundColor = Color.FromArgb(15, 15, 15);
            AxisColor = Color.FromArgb(192, 192, 192);
            GridColor = Color.FromArgb(31, 31, 31);

            this.bounds = new Rectangle(0, 0, 1, 1);
            this.stepSize = 0.01f;
            this.horizontalGridDistance = 0.1f;
            this.verticalGridDistance = 0.1f;

            SetBounds(-2, -1, 2, 1);

            StepSize = 0.01f;
            HorizontalGridDistance = 0.1f;
            VerticalGridDistance = 0.1f;
        }

        public override void Death() {
            base.Death();
        }

        public override void Update(float deltaTime) {
        }

        private void Render(int renderLayer, Renderer renderer) {
            bool changedArea = false;
            if (InputHandler.IsKeyDown(Key.E)) {
                this.bounds.Scale(1.01f, 1.01f);
                this.stepSize *= 1.01f;
                changedArea = true;
            } else if (InputHandler.IsKeyDown(Key.Q)) {
                this.bounds.Scale(0.99f, 0.99f);
                this.stepSize *= 0.99f;
                changedArea = true;
            }
            if (InputHandler.IsKeyDown(Key.A)) {
                this.bounds.Translate(-bounds.Width * 0.01f, 0);
                changedArea = true;
            } else if (InputHandler.IsKeyDown(Key.D)) {
                this.bounds.Translate(bounds.Width * 0.01f, 0);
                changedArea = true;
            }
            if (InputHandler.IsKeyDown(Key.W)) {
                this.bounds.Translate(0, bounds.Height * 0.01f);
                changedArea = true;
            } else if (InputHandler.IsKeyDown(Key.S)) {
                this.bounds.Translate(0, -bounds.Height * 0.01f);
                changedArea = true;
            }

            if (changedArea) {
                RecalculateXValues();
                RecalculateGridValues();
            }

            Rectangle worldBounds = Scene.Active.MainViewport.WorldBounds;

            float worldBoundsRatio = worldBounds.Width / worldBounds.Height;
            float boundsRatio = bounds.Width / bounds.Height;

            float plotAreaWidth;
            float plotAreaHeight;
            float minX, minY, maxX, maxY;

            if (boundsRatio > 1) {
                // height < width
                plotAreaWidth = worldBounds.Width;
                plotAreaHeight = worldBounds.Height / boundsRatio;

                minX = worldBounds.Left;
                maxX = worldBounds.Right;
                minY = worldBounds.Bottom + (worldBounds.Height - plotAreaHeight) / 2f;
                maxY = minY + plotAreaHeight;
            } else {
                // height > width
                plotAreaWidth = worldBounds.Width * boundsRatio;
                plotAreaHeight = worldBounds.Height;

                minX = worldBounds.Left + (worldBounds.Width - plotAreaWidth) / 2f;
                maxX = minX + plotAreaWidth;
                minY = worldBounds.Bottom;
                maxY = worldBounds.Top;
            }

            float centerX = minX + plotAreaWidth / 2f;
            float centerY = minY + plotAreaHeight / 2f;
            float gridSizeX = plotAreaWidth / bounds.Width;
            float gridSizeY = plotAreaHeight / bounds.Height;
            void PointToDraw(float x, float y, out float drawX, out float drawY) {
                drawX = minX + (x - bounds.X) * gridSizeX;
                drawY = minY + (y - bounds.Y) * gridSizeY;
                //drawY = maxY - (y - bounds.Y) * gridSizeY;
            }
            void DrawToPoint(float x, float y, out float px, out float py) {
                px = (x - minX) / gridSizeX + bounds.X;
                py = (y - minY) / gridSizeY + bounds.Y;
                //py = (maxY - y) / gridSizeY + bounds.Y;
            }

            renderer.FillRectangle(RenderLayer, PlotAreaBackgroundColor, minX, maxY, plotAreaWidth, plotAreaHeight);

            if (bounds.Top >= 0 && bounds.Bottom <= 0) {
                PointToDraw(0, 0, out float dx, out float dy);
                renderer.DrawLine(RenderLayer - 2, AxisColor, 0.1f, minX, dy, maxX, dy);
            }
            if (bounds.Left <= 0 && bounds.Right >= 0) {
                PointToDraw(0, 0, out float dx, out float dy);
                renderer.DrawLine(RenderLayer - 2, AxisColor, 0.1f, dx, minY, dx, maxY);
            }

            for (int gX = 0; gX < this.gridCountX; gX++) {
                float gridX = this.minGridX + gX * HorizontalGridDistance;

                PointToDraw(gridX, 0, out float dx, out float dy);
                renderer.DrawLine(RenderLayer - 1, GridColor, 0.1f, dx, minY, dx, maxY);
            }

            for (int gY = 0; gY < this.gridCountY; gY++) {
                float gridY = this.minGridY + gY * VerticalGridDistance;

                PointToDraw(0, gridY, out float dx, out float dy);
                renderer.DrawLine(RenderLayer - 1, GridColor, 0.1f, minX, dy, maxX, dy);
            }

            Vector2 mousePos = Scene.Active.MainViewport.ScreenToWorldCoordinates(InputHandler.MousePosition);
            float textX = mousePos.X > worldBounds.CenterX ? -40 : 0;
            float textY = mousePos.Y > worldBounds.CenterY ? -8 : 4;
            DrawToPoint(mousePos.X, mousePos.Y, out float vx, out float vy);
            
            if (bounds.Contains(vx, vy)) {
                IEnumerable<(string name, Function function, Color color)> functions = this.functions.Where(v => v.Value.active).Select(v => (v.Key, v.Value.function, v.Value.color));
                bool nearFunction = false;
                float functionValue = vy;
                float functionValueDst = float.MaxValue;
                Color functionColor = AxisColor;
                string functionName = "";
                foreach ((string name, Function function, Color color) functionData in functions) {
                    float fV = functionData.function(vx);
                    float fVD = Math.Abs(fV - vy);
                    if (fVD < functionValueDst && fVD < bounds.Height * 0.02f) {
                        nearFunction = true;
                        functionValue = fV;
                        functionValueDst = fVD;
                        functionColor = functionData.color;
                        functionName = functionData.name;
                    }
                }

                renderer.DrawText(RenderLayer - 4, functionColor, "Consolas", nearFunction ? 3.5f : 3.75f, $"{functionName}[{(int)(vx * 10000f) / 10000f}, {(int)(functionValue * 1000f) / 1000f}]", mousePos.X + textX, mousePos.Y + textY);

                if (nearFunction) {
                    PointToDraw(vx, functionValue, out float tmpDX, out float tmpDY);
                    renderer.FillCircle(RenderLayer - 4, functionColor, tmpDX, tmpDY, 0.75f);
                }
            }

            foreach ((Function function, Color color, float[] values, bool active) functionData in this.functions.Values.Where(fD => fD.active)) {
                if (this.xValues == null || functionData.values == null)
                    continue;

                for (int i = 0; i < this.xValues.Length - 1; i++) {
                    float px0 = xValues[i];
                    float py0 = functionData.values[i];
                    float px1 = xValues[i + 1];
                    float py1 = functionData.values[i + 1];

                    bool inside0 = bounds.Contains(px0, py0);
                    bool inside1 = bounds.Contains(px1, py1);
                    if (!inside0 && !inside1)
                        continue;
                    else if (inside0 && !inside1) {
                        // TODO
                    }

                    PointToDraw(px0, py0, out float dx0, out float dy0);
                    PointToDraw(px1, py1, out float dx1, out float dy1);
                    
                    renderer.DrawLine(RenderLayer - 3, functionData.color, 0.15f, dx0, dy0, dx1, dy1);
                }
            }

            
        }

        public void AddFunction(string name, Function function, Color color) {
            this.functions[name] = (function, color, null, true);

            RecalculateFunctionValues(name);
        }

        public void ActivateFunction(string name) {
            if (this.functions.TryGetValue(name, out (Function function, Color color, float[] values, bool active) functionData))
                functionData.active = true;
        }

        public void DeactivateFunction(string name) {
            if (this.functions.TryGetValue(name, out (Function function, Color color, float[] values, bool active) functionData))
                functionData.active = false;
        }

        private void RecalculateFunctionValues(params string[] functionNames) => RecalculateFunctionValues((IEnumerable<string>)functionNames);

        private void RecalculateFunctionValues(IEnumerable<string> functionNames) {
            functionNames = new List<string>(functionNames);
            foreach (string functionName in functionNames) {
                (Function function, Color color, float[] values, bool active) functionData = this.functions[functionName];
                float[] values = new float[xValues.Length];
                for (int i = 0; i < xValues.Length; i++) {
                    values[i] = functionData.function(xValues[i]);
                }
                functionData.values = values;
                this.functions[functionName] = functionData;
            }
        }

        private void RecalculateXValues() {
            float tmp = bounds.X / StepSize;
            float minXValue = StepSize * (int)(bounds.X < 0 ? tmp : tmp + 1f);
            int xValueCount = (int)(bounds.Width / StepSize) + 1;

            this.xValues = new float[xValueCount];
            for (int i = 0; i < xValueCount; i++) {
                this.xValues[i] = minXValue + i * StepSize;
            }

            RecalculateFunctionValues(functions.Keys);
        }

        private void RecalculateGridValues() {
            float tmp = bounds.X / HorizontalGridDistance;
            this.minGridX = HorizontalGridDistance * (int)(bounds.X < 0 ? tmp : tmp + 1f);
            tmp = bounds.Y / VerticalGridDistance;
            this.minGridY = VerticalGridDistance * (int)(bounds.Y < 0 ? tmp : tmp + 1f);

            this.gridCountX = (int)(bounds.Width / HorizontalGridDistance) + 1;
            this.gridCountY = (int)(bounds.Height / VerticalGridDistance) + 1;
        }

        public void SetBounds(float minX, float minY, float maxX, float maxY) {
            if (maxX < minX)
                throw new ArgumentOutOfRangeException(nameof(maxX));
            if (maxY < minY)
                throw new ArgumentOutOfRangeException(nameof(maxY));

            this.bounds.X = minX;
            this.bounds.Y = minY;
            this.bounds.Width = (maxX - minX);
            this.bounds.Height = (maxY - minY);
            
            RecalculateXValues();
            RecalculateGridValues();
        }

        public float StepSize {
            get => this.stepSize;
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this.stepSize = value;

                RecalculateXValues();
            }
        }

        public float HorizontalGridDistance {
            get => this.horizontalGridDistance;
            set {
                if (value < 0)
                    value = 0;

                this.horizontalGridDistance = value;
                RecalculateGridValues();
            }
        }

        public float VerticalGridDistance {
            get => this.verticalGridDistance;
            set {
                if (value < 0)
                    value = 0;

                this.verticalGridDistance = value;
                RecalculateGridValues();
            }
        }

        public Renderable Renderable => this.shape;

        public int RenderLayer => 0;

    }
}