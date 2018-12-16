using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GameEngineX.Application;
using GameEngineX.Game;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Graphics;
using GameEngineX.Graphics.Renderables;
using GameEngineX.Input;
using GameEngineX.Utility;
using GameEngineX.Utility.Math;
using Rectangle = GameEngineX.Utility.Math.Rectangle;

namespace TestGame.CellularAutomatas {
    public abstract class CellularAutomata : GameObjectComponent, IRendering {

        protected const int STATE_EMPTY = 0;

        public int Seed { get; private set; }

        public int CellCount { get; private set; }

        public bool IsTorus { get; private set; }
        public NeighbourhoodMode NeighbourhoodMode { get; private set; }

        private bool NeedGeneration;

        public bool ParallelExecution;

        public Key PauseKey = Key.Space;
        public Key TrackStateKey = Key.T;
        public Key SaveKey = Key.F5;
        public Key LoadKey = Key.F9;
        public Key EditKey = Key.Enter;
        public Key ExitKey = Key.Escape;

        private CustomShape shape;

        private CellularAutomataCell[,] grid;
        private bool gridSelector;

        private int stateHistorySize;
        private readonly Dictionary<long, int> stateCounter = new Dictionary<long, int>();
        private readonly List<Dictionary<long, float>> stateHistory = new List<Dictionary<long, float>>();
        private int stateHistoryIndicator;

        private Random random;

        private bool trackState;
        private bool paused;

        private bool editMode;
        private int selectedStateEditIndex;
        private float editStateValue;

        private (int x, int y) hoveredCell;

        public void Init(CellularAutomataInitializationData initializationData) {
            CellCount = initializationData.CellCount;
            IsTorus = initializationData.IsTorus;
            NeighbourhoodMode = initializationData.NeighbourhoodMode;
            Seed = initializationData.Seed == 0 ? new Random().Next() : initializationData.Seed;
            this.random = new Random(Seed);
            ParallelExecution = false;
            NeedGeneration = !initializationData.IsReadOnly();

            this.paused = true;

            this.trackState = true;

            this.shape = new CustomShape(Render);

            this.stateCounter.Clear();
            this.stateHistory.Clear();
            StateHistorySize = 100;
            foreach (long state in States) {
                this.stateCounter[state] = 0;
            }

            this.grid = new CellularAutomataCell[CellCount, CellCount];
            for (int y = 0; y < CellCount; y++) {
                for (int x = 0; x < CellCount; x++) {
                    long state;
                    if (!NeedGeneration) {
                        this.grid[x, y] = new CellularAutomataCell(initializationData.CellData[x, y], () => gridSelector);
                        state = this.grid[x, y].State;
                    } else {
                        this.grid[x, y] = new CellularAutomataCell(() => gridSelector);
                        GenerateCellState(this.random, x, y, out state, out float stateValue, out object cellData);
                        this.grid[x, y].Set(state, stateValue, cellData);
                    }

                    TrackState(state, null);
                }
            }

            this.hoveredCell = (-1, -1);

            InputHandler.OnKeyUp += (key, modifiers) => {
                if (key == ExitKey)
                    ApplicationBase.Instance.Exit();
                else if (key == PauseKey)
                    this.paused = !this.paused;
                else if (key == TrackStateKey)
                    this.trackState = !this.trackState;
                else if (key == EditKey) {
                    this.editMode = !this.editMode;

                    if (this.editMode) {
                        this.selectedStateEditIndex = 0;
                        this.editStateValue = 0;
                    }
                } else if (key == Key.Up && this.editMode)
                    this.selectedStateEditIndex = (this.selectedStateEditIndex + States.Count() - 1) % States.Count();
                else if (key == Key.Down && this.editMode)
                    this.selectedStateEditIndex = (this.selectedStateEditIndex + 1) % States.Count();
                else if (key == Key.Right && this.editMode)
                    this.editStateValue = MathUtility.RoundToInt((this.editStateValue + 0.1f) * 10) / 10f;
                else if (key == Key.Left && this.editMode)
                    this.editStateValue = MathUtility.RoundToInt((this.editStateValue - 0.1f) * 10) / 10f;

            };

            InputHandler.OnMouseMove += (x, y) => {
                if (!this.editMode)
                    return;

                Vector2 mousePos = Scene.Active.MainViewport.ScreenToWorldCoordinates(new Vector2(x, y));

                Rectangle worldBounds = Scene.Active.MainViewport.WorldBounds;

                mousePos.X -= worldBounds.X;
                mousePos.Y -= worldBounds.Y;

                mousePos.X /= worldBounds.Height;
                mousePos.Y /= worldBounds.Height;

                mousePos.Y = 1f - mousePos.Y;

                if (mousePos.X >= 0 && mousePos.X < 1 && mousePos.Y >= 0 && mousePos.Y < 1) {
                    this.hoveredCell = ((int)(mousePos.X * CellCount), (int)(mousePos.Y * CellCount));

                    if (InputHandler.IsMouseButtonDown(MouseButton.Left)) {
                        long toBeSetState = States.ElementAt(this.selectedStateEditIndex);
                        CellularAutomataCell cell = this.grid[hoveredCell.x, hoveredCell.y];
                        long oldState = cell.State;
                        cell.State = toBeSetState;
                        cell.StateValue = this.editStateValue;

                        TrackState(toBeSetState, oldState);

                    }
                } else {
                    this.hoveredCell = (-1, -1);
                }
            };

        }

        public override void Update(float deltaTime) {
            UpdateInput();

            //UpdateEditing();

            if (this.paused || this.editMode)
                return;

            UpdateCells(deltaTime);
        }

        private void UpdateInput() {
            //if (InputHandler.IsKeyPressed(ExitKey))
            //    ApplicationBase.Instance.Exit();

            //if (InputHandler.IsKeyPressed(PauseKey))
            //    this.paused = !this.paused;

            //if (InputHandler.IsKeyPressed(TrackStateKey))
            //    this.trackState = !this.trackState;

            //if (InputHandler.IsKeyPressed(EditKey)) {
            //    this.editMode = !this.editMode;

            //    if (this.editMode) {
            //        this.selectedStateEditIndex = 0;
            //        this.editStateValue = 0;
            //    }
            //}

            if (InputHandler.IsKeyPressed(SaveKey))
                Save();

            if (InputHandler.IsKeyPressed(LoadKey))
                Load();
        }

        /*private void UpdateEditing() {
            if (!this.editMode)
                return;

            if (InputHandler.IsKeyPressed(Key.Up)) {
                this.selectedStateEditIndex = (this.selectedStateEditIndex + States.Count() - 1) % States.Count();
            } else if (InputHandler.IsKeyPressed(Key.Down)) {
                this.selectedStateEditIndex = (this.selectedStateEditIndex + 1) % States.Count();
            }

            if (InputHandler.IsKeyPressed(Key.Right)) {
                this.editStateValue = MathUtility.RoundToInt((this.editStateValue + 0.1f) * 10) / 10f;
            } else if (InputHandler.IsKeyPressed(Key.Left)) {
                this.editStateValue = MathUtility.RoundToInt((this.editStateValue - 0.1f) * 10) / 10f;
            }

            (int x, int y) mousePosition_raw = InputHandler.MousePosition;
            Vector2 mousePos = Scene.Active.MainViewport.ScreenToWorldCoordinates(new Vector2(mousePosition_raw.x, mousePosition_raw.y));

            Rectangle worldBounds = Scene.Active.MainViewport.WorldBounds;

            mousePos.X -= worldBounds.X;
            mousePos.Y -= worldBounds.Y;

            mousePos.X /= worldBounds.Height;
            mousePos.Y /= worldBounds.Height;

            mousePos.Y = 1f - mousePos.Y;

            if (mousePos.X >= 0 && mousePos.X < 1 && mousePos.Y >= 0 && mousePos.Y < 1) {
                this.hoveredCell = ((int)(mousePos.X * CellCount), (int)(mousePos.Y * CellCount));

                if (InputHandler.IsMouseButtonDown(MouseButton.Left)) {
                    long toBeSetState = States.ElementAt(this.selectedStateEditIndex);
                    CellularAutomataCell cell = this.grid[hoveredCell.x, hoveredCell.y];
                    long oldState = cell.State;
                    cell.State = toBeSetState;
                    cell.StateValue = this.editStateValue;

                    TrackState(toBeSetState, oldState);

                }
            } else {
                this.hoveredCell = (-1, -1);
            }
        }*/

        private void UpdateCells(float deltaTime) {
            if (!ParallelExecution) {
                for (int y = 0; y < CellCount; y++) {
                    for (int x = 0; x < CellCount; x++) {
                        CurrentStateAt(x, y, out long currentState, out float currentStateValue, out object currentCellData);
                        CalculateNewState(x, y, currentState, currentStateValue, currentCellData, deltaTime, random, out long state, out float stateValue, out object cellData);
                        this.grid[x, y].SetCalculation(state, stateValue);
                        TrackState(this.grid[x, y].CalculationState, this.grid[x, y].State);
                    }
                }
            } else {
                Parallel.For(0, CellCount, y => {
                    for (int x = 0; x < CellCount; x++) {
                        CurrentStateAt(x, y, out long currentState, out float currentStateValue, out object currentCellData);
                        CalculateNewState(x, y, currentState, currentStateValue, currentCellData, deltaTime, random, out long state, out float stateValue, out object cellData);
                        this.grid[x, y].SetCalculation(state, stateValue);
                        TrackState(this.grid[x, y].CalculationState, this.grid[x, y].State);
                    }
                });
            }
            this.stateHistoryIndicator++;
            this.stateHistoryIndicator %= StateHistorySize;

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
                        CellularAutomataCell c = this.grid[x, y];
                        bool isEditedCell = this.editMode && this.hoveredCell.x == x && this.hoveredCell.y == y;

                        long drawState = isEditedCell ? States.ElementAt(this.selectedStateEditIndex) : c.State;
                        float drawStateValue = isEditedCell ? this.editStateValue : c.StateValue;

                        Color stateColor = GetStateColor(drawState, drawStateValue);

                        float px = minX + x * stepX;
                        float py = minY + y * stepY;

                        renderer.FillRectangle(renderLayer, stateColor, px, py, stepX, stepY);

                        if (isEditedCell) {
                            renderer.DrawRectangle(renderLayer - 1, stateColor.Invert(), 0.1f, px, py, stepX, stepY);
                        }
                    }
                }
            }

            if (this.editMode) {
                renderer.FillRectangle(renderLayer + 10, Color.DarkRed, minX + worldBounds.Height, minY, worldBounds.Width - worldBounds.Height, worldBounds.Height);
                
                renderer.DrawCenteredText(renderLayer, Color.White, "Consolas", this.editStateValue.ToString(),
                    (worldBounds.Width - worldBounds.Height) / 2.1f, -worldBounds.Height * 0.1f, ((int)(worldBounds.Height * 0.3f), (int)(worldBounds.Height * 0.2f)));
            }

            if (!this.trackState)
                return;

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

                    bool isSelectedForEdit = this.editMode && States.ElementAt(this.selectedStateEditIndex) == item.Key;

                    float pct = item.Value / (float)(CellCount * CellCount);
                    renderer.DrawText(
                        renderLayer, c,
                        "Consolas", 5f,
                        $"{(isSelectedForEdit ? ">>" : "")}{CellStateToString(item.Key)}\t\t{item.Value}\t\t{(int)(pct * 100f)}%"
                        , minX + worldBounds.Height, minY + (i - 0) * 5.25f);


                    int k = 0;
                    for (int j = stateHistoryIndicator + 1; j < stateHistoryIndicator + 1 + StateHistorySize; j++, k++) {
                        int historyIdx = j % StateHistorySize;
                        if (!this.stateHistory[historyIdx].ContainsKey(item.Key))
                            this.stateHistory[historyIdx][item.Key] = 0;
                        renderer.DrawPoint(0, c, 0.5f, pieChartMaxX + k * historyDiagramWidth / StateHistorySize, maxY - historyDiagramHeight * stateHistory[historyIdx][item.Key]);
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

        protected abstract void GenerateCellState(Random random, int x, int y, out long state, out float stateValue, out object cellData);

        protected abstract void CalculateNewState(int x, int y, long currentState, float currentStateValue, object currentCellData, float deltaTime, Random random, out long state, out float stateValue, out object cellData);

        private void Load() {
            CellularAutomataInitializationData initData = CellularAutomataStateLoader.Load(SavePath, new CellularAutomataStateLoadingParameters(new[] { SavePath }));
            Init(initData);
        }

        private void Save() {
            (long state, float stateValue)[,] cellData = new(long state, float stateValue)[CellCount, CellCount];
            for (int y = 0; y < CellCount; y++)
            for (int x = 0; x < CellCount; x++)
                cellData[x, y] = this.grid[x, y].ToCellData;

            CellularAutomataInitializationData data = new CellularAutomataInitializationData(Seed, cellData, IsTorus, NeighbourhoodMode);
            CellularAutomataStateLoader.Write(SavePath, data);
        }

        protected void CurrentStateAt(int x, int y, out long state, out float stateValue, out object cellData) {
            if (IsTorus) {
                while (x < 0)
                    x += CellCount;
                while (y < 0)
                    y += CellCount;

                x %= CellCount;
                y %= CellCount;
            }

            if (x < 0 || x >= CellCount)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y >= CellCount)
                throw new ArgumentOutOfRangeException(nameof(y));

            CellularAutomataCell c = this.grid[x, y];
            state = c.State;
            stateValue = c.StateValue;
            cellData = c.CellData;
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

        protected HashSet<long> NeighbouringStates(int x, int y) {
            HashSet<long> neighbouringStates = new HashSet<long>();
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

                    CellularAutomataCell c = grid[xx, yy];
                    neighbouringStates.Add(c.State);
                }
            }

            return neighbouringStates;
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

                    CellularAutomataCell c = grid[xx, yy];
                    if (xx >= 0 && xx < CellCount && yy >= 0 && yy < CellCount && (c.State & state) > 0)
                        value = action(value, c.StateValue);
                }
            }

            return value;
        }

        protected IEnumerable<object> NeighbourCellData(int x, int y) {
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

                    CellularAutomataCell c = grid[xx, yy];
                    yield return c.CellData;
                }
            }
        }

        protected int StateCount(long state) {
            lock (this.stateCounter) {
                if (!this.stateCounter.ContainsKey(state))
                    throw new ArgumentException();

                return this.stateCounter[state];
            }
        }

        private void TrackState(long state, long? oldState) {
            if (!this.trackState)
                return;

            lock (this.stateCounter) {
                if (oldState != null)
                    this.stateCounter[(long)oldState]--;


                //if (this.stateCounter.TryGetValue(state, out int count))
                this.stateCounter[state] = this.stateCounter[state] + 1;
                //else
                    //this.stateCounter[state] = 1;

                this.stateHistory[stateHistoryIndicator][state] = this.stateCounter[state] / (float)(CellCount * CellCount);
            }
        }

        public abstract IEnumerable<long> States { get; }

        protected abstract Color GetStateColor(long state, float stateValue);

        protected abstract string CellStateToString(long state);

        protected abstract string SavePath { get; }

        private void SwapGrids() {
            gridSelector = !gridSelector;
        }

        public int StateHistorySize {
            get => this.stateHistorySize;
            set {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this.stateHistorySize = value;

                this.stateHistory.Clear();
                for (int i = 0; i < StateHistorySize; i++) {
                    stateHistory.Add(new Dictionary<long, float>());
                }
                this.stateHistoryIndicator = 0;
            }
        }

        public int RenderLayer => 0;

        public Renderable Renderable => this.shape;

    }
}