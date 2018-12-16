using System;
using System.Collections.Generic;
using System.Drawing;

namespace TestGame.CellularAutomatas {
    public class Wireworld : CellularAutomata {

        private static string FILE_PATH = "wireWorld.ca";

        private const int CELLS = 50;

        private const long STATE_CONDUCTOR = 1;
        private const long STATE_ELECTRON_HEAD = 2;
        private const long STATE_ELECTRON_TAIL = 4;

        public override void Initialize() {
            base.Initialize();

            //ResourceManager.RegisterResourceLoader(new CellularAutomataStateLoader());

            //ResourceManager.LoadResource<CellularAutomataInitializationData, CellularAutomataStateLoadingParameters>(
            //    "CAStateData",
            //    new CellularAutomataStateLoadingParameters(new [] {FILE_PATH}), 0, false);

            //ResourceManager.GetResource("CAStateData", out CellularAutomataInitializationData initData, true);

            CellularAutomataInitializationData initData = new CellularAutomataInitializationData(0, CELLS, true, NeighbourhoodMode.Moore);

            Init(initData);
        }

        protected override void GenerateCellState(Random random, int x, int y, out long state, out float stateValue, out object cellData) {
            cellData = null;
            state = STATE_EMPTY;
            stateValue = 0;
        }

        protected override void CalculateNewState(int x, int y, long currentState, float currentStateValue, object currentCellData, float deltaTime, Random random, out long state, out float stateValue, out object cellData) {
            cellData = null;
            state = currentState;
            stateValue = currentStateValue;

            if (currentState == STATE_ELECTRON_HEAD)
                state = STATE_ELECTRON_TAIL;
            else if (currentState == STATE_ELECTRON_TAIL)
                state = STATE_CONDUCTOR;
            else if (currentState == STATE_CONDUCTOR) {
                int nb_heads = Neighbours(x, y, STATE_ELECTRON_HEAD);
                if (nb_heads == 1 || nb_heads == 2)
                    state = STATE_ELECTRON_HEAD;
                else
                    state = STATE_CONDUCTOR;
            }
        }

        protected override void StepComplete() {
        }

        public override IEnumerable<long> States => new[] { STATE_EMPTY, STATE_CONDUCTOR, STATE_ELECTRON_HEAD, STATE_ELECTRON_TAIL };

        private static Color COLOR_EMPTY = Color.FromArgb(0, 15, 0);
        protected override Color GetStateColor(long state, float stateValue) {
            switch (state) {
                case STATE_EMPTY: return COLOR_EMPTY;
                case STATE_CONDUCTOR: return Color.Gold;
                case STATE_ELECTRON_HEAD: return Color.Red;
                case STATE_ELECTRON_TAIL: return Color.DodgerBlue;
                default: throw new ArgumentException("Invalid state.", nameof(state));
            }
        }

        protected override string CellStateToString(long state) {
            switch (state) {
                case STATE_EMPTY: return "Empty";
                case STATE_CONDUCTOR: return "Conductor";
                case STATE_ELECTRON_HEAD: return "Electron Head";
                case STATE_ELECTRON_TAIL: return "Electron Tail";
                default: throw new ArgumentException("Invalid state.", nameof(state));
            }
        }

        protected override string SavePath => FILE_PATH;
    }
}