using System;
using System.Collections.Generic;
using System.Drawing;

namespace TestGame.CellularAutomatas {
    public class ConwaysGameOfLife : CellularAutomata {
        private static string FILE_PATH = "cwgol.ca";

        private const int CELLS = 100;

        private const float ALIVE_CHANCE = 0.3f;

        private const long STATE_ALIVE = 1;
        private const long STATE_DEAD = 2;

        private static readonly Color COLOR_ALIVE = Color.White;
        private static readonly Color COLOR_DEAD = Color.FromArgb(15, 15, 15);

        public override void Initialize() {
            base.Initialize();

            //ResourceManager.RegisterResourceLoader(new CellularAutomataStateLoader());

            //ResourceManager.LoadResource<CellularAutomataInitializationData, CellularAutomataStateLoadingParameters>(
            //    "CAStateData",
            //    new CellularAutomataStateLoadingParameters(new [] {FILE_PATH}), 0, false);

            //CellularAutomataInitializationData initData = CellularAutomataStateLoader.Load(SavePath, new CellularAutomataStateLoadingParameters(new[] { FILE_PATH }));

            CellularAutomataInitializationData initData = new CellularAutomataInitializationData(0, CELLS, true, NeighbourhoodMode.Moore);

            Init(initData);
        }

        protected override void StepComplete() {
        }

        protected override void GenerateCellState(Random random, int x, int y, out long state, out float stateValue, out object cellData) {
            cellData = null;
            stateValue = 0;
            state = random.NextDouble() < ALIVE_CHANCE ? STATE_ALIVE : STATE_DEAD;
        }

        protected override void CalculateNewState(int x, int y, long currentState, float currentStateValue, object currentCellData, float deltaTime, Random random, out long state, out float stateValue, out object cellData) {
            cellData = null;
            stateValue = 0;
            int neighbours = Neighbours(x, y, STATE_ALIVE);

            if (neighbours < 2)
                state = STATE_DEAD;
            else if (neighbours == 3)
                state = STATE_ALIVE;
            else if (neighbours > 3)
                state = STATE_DEAD;
            else
                state = currentState;
        }

        public override IEnumerable<long> States => new[] {STATE_ALIVE, STATE_DEAD};

        protected override Color GetStateColor(long state, float stateValue) {
            switch (state) {
                case STATE_ALIVE: return COLOR_ALIVE;
                case STATE_DEAD: return COLOR_DEAD;
                default: throw new ArgumentException();
            }
        }

        protected override string CellStateToString(long state) {
            switch (state) {
                case STATE_ALIVE: return "Alive";
                case STATE_DEAD: return "Dead";
                default: throw new ArgumentException();
            }
        }

        protected override string SavePath => FILE_PATH;
    }
}