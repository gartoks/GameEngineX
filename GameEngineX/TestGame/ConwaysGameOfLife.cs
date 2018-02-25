using System;
using System.Drawing;

namespace TestGame {
    public class ConwaysGameOfLife : CellularAutomata {

        private const int CELLS = 100;

        private const float ALIVE_CHANCE = 0.3f;

        private const long STATE_ALIVE = 1;
        private const long STATE_DEAD = 2;

        private static readonly Color COLOR_ALIVE = Color.White;
        private static readonly Color COLOR_DEAD = Color.FromArgb(15, 15, 15);

        public override void Initialize() {
            base.Initialize();

            Init(CELLS, true, NeighbourhoodMode.Moore);

            Setup();
        }

        protected override void StepComplete() {
        }

        protected override void GenerateCellState(Random random, int x, int y, out long state, out float stateValue) {
            stateValue = 0;
            state = random.NextDouble() < ALIVE_CHANCE ? STATE_ALIVE : STATE_DEAD;
        }

        protected override void CalculateNewState(int x, int y, long currentState, float currentStateValue, Random random, out long state, out float stateValue) {
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

        protected override Color GetStateColor(long state, float stateValue) {
            switch (state) {
                case STATE_ALIVE: return COLOR_ALIVE;
                case STATE_DEAD: return COLOR_DEAD;
                default: throw new NotImplementedException();
            }
        }

        protected override string CellStateToString(long state) {
            switch (state) {
                case STATE_ALIVE: return "Alive";
                case STATE_DEAD: return "Dead";
                default: throw new NotImplementedException();
            }
        }
    }
}