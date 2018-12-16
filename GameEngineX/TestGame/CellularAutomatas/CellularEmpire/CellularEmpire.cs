using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GameEngineX.Utility.Math;

namespace TestGame.CellularAutomatas.CellularEmpire {
    public class CellularEmpire : CellularAutomata {

        private static string FILE_PATH = "cellularEmpire.ca";

        private const int CELLS = 100;

        private int stateCount;
        private long[] states;
        private Color[] stateColors;
        private string[] stateNames;

        public override void Initialize() {
            base.Initialize();

            this.stateCount = 2;

            this.states = new long[stateCount];
            for (int i = 0; i < stateCount; i++)
                this.states[i] = IndexToState(i);

            CellularAutomataInitializationData initData = new CellularAutomataInitializationData(0, CELLS, true, NeighbourhoodMode.Moore);
            
            Init(initData);

            Random random = new Random(Seed);
            this.stateColors = new Color[stateCount];
            for (int i = 0; i < stateCount; i++)
                this.stateColors[i] = Color.FromArgb(random.Next(0, 256), random.Next(0, 256), random.Next(0, 256));
            this.stateNames = new string[stateCount];
            for (int i = 0; i < stateCount; i++)
                this.stateNames[i] = "State " + this.states[i];
        }

        protected override void GenerateCellState(Random random, int x, int y, out long state, out float stateValue, out object cellData) {
            state = this.states[random.Next(this.stateCount)];
            CellularEmpireCellData cecd = new CellularEmpireCellData(this, state);
            cellData = cecd;
            stateValue = cecd.StateValue;
        }

        protected override void CalculateNewState(int x, int y, long currentState, float currentStateValue, object currentCellData, float deltaTime, Random random, out long state, out float stateValue, out object cellData) {
            cellData = currentCellData;
            CellularEmpireCellData cecd = (CellularEmpireCellData)currentCellData;
            
            cecd.Battle(random, NeighbourCellData(x, y).Cast<CellularEmpireCellData>(), out state, out stateValue);
        }

        protected override void StepComplete() {
        }

        protected override Color GetStateColor(long state, float stateValue) => stateColors[StateToIndex(state)];

        protected override string CellStateToString(long state) => stateNames[StateToIndex(state)];

        public override IEnumerable<long> States => this.states;

        protected override string SavePath => FILE_PATH;

        public static long IndexToState(int index) => Math.Pow(2, index).RoundToInt();

        public static int StateToIndex(long state) => Math.Log(state, 2).RoundToInt();
    }
}