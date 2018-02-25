using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using GameEngineX.Application;
using GameEngineX.Game;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Graphics;
using GameEngineX.Graphics.Renderables;
using Rectangle = GameEngineX.Utility.Math.Rectangle;

namespace TestGame {
    public abstract class CellularAutomata : GameObjectComponent, IRendering {
        private sealed class Cell {
            private long s0;
            private float v0;
            private long s1;
            private float v1;
            private Func<bool> selector;

            public Cell(Func<bool> selector) {
                this.selector = selector;
            }

            public void Set(long state, float value) {
                State = state;
                Value = value;
            }

            public void SetCalculation(long state, float value) {
                CalculationState = state;
                CalculationValue = value;
            }

            public long State {
                get => selector() ? s0 : s1;
                set {
                    if (selector())
                        s0 = value;
                    else
                        s1 = value;
                }
            }

            public long CalculationState {
                get => selector() ? s1 : s0;
                set {
                    if (selector())
                        s1 = value;
                    else
                        s0 = value;
                }
            }

            public float Value {
                get => selector() ? v0 : v1;
                set {
                    if (selector())
                        v0 = value;
                    else
                        v1 = value;
                }
            }

            public float CalculationValue {
                get => selector() ? v1 : v0;
                set {
                    if (selector())
                        v1 = value;
                    else
                        v0 = value;
                }
            }
        }

        private const int STATEHISTORY_SIZE = 100;

        protected const int STATE_EMPTY = 0;

        public int CellCount { get; private set; }

        public bool IsTorus { get; private set; }
        public NeighbourhoodMode NeighbourhoodMode { get; private set; }

        public bool ParallelExecution;

        private CustomShape shape;

        private Cell[,] grid;
        private bool gridSelector;

        private Dictionary<long, int> stateCounter;
        private List<Dictionary<long, float>> stateHistory;
        private int stateHistoryIndicator;

        private Random random;

        public void Init(int cellCount, bool isTorus, NeighbourhoodMode neighbourhoodMode) {
            CellCount = cellCount;
            IsTorus = isTorus;
            NeighbourhoodMode = neighbourhoodMode;
            ParallelExecution = false;

            this.shape = new CustomShape(Render);

            this.grid = new Cell[CellCount, CellCount];
            for (int y = 0; y < CellCount; y++) {
                for (int x = 0; x < CellCount; x++) {
                    this.grid[x, y] = new Cell(() => gridSelector);
                }
            }
            //this.calculationGrid = new int[CellCount, CellCount];

            this.stateCounter = new Dictionary<long, int>();
            this.stateHistory = new List<Dictionary<long, float>>();
            for (int i = 0; i < STATEHISTORY_SIZE; i++) {
                stateHistory.Add(new Dictionary<long, float>());
            }
            this.stateHistoryIndicator = 0;
        }

        public void Setup(int? seed = null) {
            this.stateCounter.Clear();

            int sd = seed ?? new Random().Next();
            this.random = new Random(sd);

            for (int y = 0; y < CellCount; y++) {
                for (int x = 0; x < CellCount; x++) {
                    GenerateCellState(this.random, x, y, out long state, out float stateValue);
                    this.grid[x, y].Set(state, stateValue);
                    TrackState(state, null);
                }
            }
        }

        public override void Update(float deltaTime) {
            if (!ParallelExecution) {
                for (int y = 0; y < CellCount; y++) {
                    for (int x = 0; x < CellCount; x++) {
                        CurrentStateAt(x, y, out long currentState, out float currentStateValue);
                        CalculateNewState(x, y, currentState, currentStateValue, random, out long state, out float stateValue);
                        this.grid[x, y].SetCalculation(state, stateValue);
                        TrackState(this.grid[x, y].CalculationState, this.grid[x, y].State);
                    }
                }
            } else {
                Parallel.For(0, CellCount, y => {
                    for (int x = 0; x < CellCount; x++) {
                        CurrentStateAt(x, y, out long currentState, out float currentStateValue);
                        CalculateNewState(x, y, currentState, currentStateValue, random, out long state, out float stateValue);
                        this.grid[x, y].SetCalculation(state, stateValue);
                        TrackState(this.grid[x, y].CalculationState, this.grid[x, y].State);
                    }
                });
            }
            this.stateHistoryIndicator++;
            this.stateHistoryIndicator %= STATEHISTORY_SIZE;

            SwapGrids();

            StepComplete();
        }


        private void Render(int renderLayer, Renderer renderer) {
            Rectangle worldBounds = Scene.Active.MainViewport.WorldBounds;
            float stepX = worldBounds.Height / CellCount;
            float stepY = worldBounds.Height / CellCount;
            float minX = worldBounds.X;
            float minY = worldBounds.Y;
            float maxX = minX + worldBounds.Width;
            float maxY = minY + worldBounds.Height;

            if (!ApplicationBase.Instance.IsSimulation) {
                for (int y = 0; y < CellCount; y++) {
                    for (int x = 0; x < CellCount; x++) {
                        Cell c = this.grid[x, y];
                        Color stateColor = GetStateColor(c.State, c.Value);

                        float px = minX + x * stepX;
                        float py = minY + y * stepY;

                        renderer.FillRectangle(renderLayer, stateColor, px, py, stepX, stepY);
                    }
                }
            }

            //renderer.FillRectangle(renderLayer + 1, Color.DarkGray, minX + worldBounds.Height, minY, worldBounds.Width - worldBounds.Height, worldBounds.Height);

            int i = 0;
            lock (this.stateCounter) {
                float fieldSize = worldBounds.Height;
                float fieldMaxX = minX + worldBounds.Height;
                float pieChartSize = (worldBounds.Width - fieldSize) / 3.0f;
                float pieChartRadius = pieChartSize / 2f;
                float pieChartMaxX = fieldMaxX + pieChartSize;
                float historyDiagramWidth = worldBounds.Width - fieldSize - pieChartSize;
                float historyDiagramHeight = pieChartSize;

                renderer.DrawRectangle(0, Color.White, 0.1f, pieChartMaxX, maxY - pieChartSize, historyDiagramWidth, historyDiagramHeight);

                float sA = 0;
                foreach (KeyValuePair<long, int> item in this.stateCounter) {
                    Color c = GetStateColor(item.Key, 1);

                    float pct = item.Value / (float)(CellCount * CellCount);
                    renderer.DrawText(renderLayer, c, "Consolas", 5f, $"{CellStateToString(item.Key)}\t{item.Value}\t{(int)(pct * 100f)}%"
                        , minX + worldBounds.Height, minY + (i - 0) * 5.25f);


                    int k = 0;
                    for (int j = stateHistoryIndicator + 1; j < stateHistoryIndicator + 1 + STATEHISTORY_SIZE; j++, k++) {
                        int historyIdx = j % STATEHISTORY_SIZE;
                        if (!this.stateHistory[historyIdx].ContainsKey(item.Key))
                            this.stateHistory[historyIdx][item.Key] = 0;
                        renderer.DrawPoint(0, c, 0.5f, pieChartMaxX + k * historyDiagramWidth / STATEHISTORY_SIZE, maxY - historyDiagramHeight * stateHistory[historyIdx][item.Key]);
                    }


                    float a = 360f * pct;
                    renderer.FillPie(0, c, fieldMaxX + pieChartRadius, maxY - pieChartRadius, pieChartRadius, sA, a);
                    sA += a;

                    i++;
                }
                renderer.DrawCircle(0, Color.White, 0.1f, fieldMaxX + pieChartRadius, maxY - pieChartRadius, pieChartRadius);
            }
        }

        protected abstract void StepComplete();

        protected abstract void GenerateCellState(Random random, int x, int y, out long state, out float stateValue);

        protected abstract void CalculateNewState(int x, int y, long currentState, float currentStateValue, Random random, out long state, out float stateValue);

        public void CurrentStateAt(int x, int y, out long state, out float stateValue) {
            if (IsTorus) {
                x %= CellCount;
                y %= CellCount;
            }

            if (x < 0 || x >= CellCount)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y >= CellCount)
                throw new ArgumentOutOfRangeException(nameof(y));

            Cell c = this.grid[x, y];
            state = c.State;
            stateValue = c.Value;
        }

        protected int Neighbours(int x, int y, long state) {
            int count = 0;
            for (int yi = -1; yi <= 1; yi++) {
                for (int xi = -1; xi <= 1; xi++) {
                    if (xi == 0 && yi == 0)
                        continue;

                    if (NeighbourhoodMode == NeighbourhoodMode.VonNeumann && Math.Abs(xi) == 1 && Math.Abs(yi) == 1)
                        continue;

                    int xx = x + xi;
                    int yy = y + yi;

                    if (IsTorus) {
                        xx = xx < 0 ? xx + CellCount : xx;
                        yy = yy < 0 ? yy + CellCount : yy;
                        xx = xx >= CellCount ? xx - CellCount : xx;
                        yy = yy >= CellCount ? yy - CellCount : yy;
                    }

                    if (xx >= 0 && xx < CellCount && yy >= 0 && yy < CellCount && (grid[xx, yy].State & state) > 0)
                        count += 1;
                }
            }

            return count;
        }

        protected float NeighbourStateValue(int x, int y, long state, Func<float, float, float> action) {
            float value = 0;
            for (int yi = -1; yi <= 1; yi++) {
                for (int xi = -1; xi <= 1; xi++) {
                    if (xi == 0 && yi == 0)
                        continue;

                    if (NeighbourhoodMode == NeighbourhoodMode.VonNeumann && Math.Abs(xi) == 1 && Math.Abs(yi) == 1)
                        continue;

                    int xx = x + xi;
                    int yy = y + yi;

                    if (IsTorus) {
                        xx = xx < 0 ? xx + CellCount : xx;
                        yy = yy < 0 ? yy + CellCount : yy;
                        xx = xx >= CellCount ? xx - CellCount : xx;
                        yy = yy >= CellCount ? yy - CellCount : yy;
                    }

                    Cell c = grid[xx, yy];
                    if (xx >= 0 && xx < CellCount && yy >= 0 && yy < CellCount && (c.State & state) > 0)
                        value = action(value, c.Value);
                }
            }

            return value;
        }

        public int StateCount(long state) {
            lock (this.stateCounter) {
                if (!this.stateCounter.ContainsKey(state))
                    throw new ArgumentException();

                return this.stateCounter[state];
            }
        }

        private void TrackState(long state, long? oldState) {
            lock (this.stateCounter) {
                if (oldState != null)
                    this.stateCounter[(long)oldState]--;

                if (this.stateCounter.TryGetValue(state, out int count))
                    this.stateCounter[state] = count + 1;
                else
                    this.stateCounter[state] = 1;

                this.stateHistory[stateHistoryIndicator][state] = this.stateCounter[state] / (float)(CellCount * CellCount);
            }
        }

        protected abstract Color GetStateColor(long state, float stateValue);

        protected abstract string CellStateToString(long state);

        private void SwapGrids() {
            //int[,] tmp = grid;
            //grid = calculationGrid;
            //calculationGrid = tmp;
            gridSelector = !gridSelector;
        }

        public int RenderLayer => 0;

        public Renderable Renderable => this.shape;

    }
}