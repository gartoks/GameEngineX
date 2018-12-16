using System;

namespace TestGame.CellularAutomatas {
    public sealed class CellularAutomataCell {
        private long state0;
        private float stateValue0;
        private long state1;
        private float stateValue1;
        public object CellData;
        private readonly Func<bool> selector;

        public CellularAutomataCell(Func<bool> selector) {
            this.selector = selector;
        }

        internal CellularAutomataCell((long state, float stateValue) cellData, Func<bool> selector) {
            this.selector = selector;

            State = cellData.state;
            StateValue = cellData.stateValue;
        }

        public void Set(long state, float stateValue, object cellData) {
            State = state;
            StateValue = stateValue;
            CellData = cellData;
        }

        public void SetCalculation(long state, float stateValue) {
            CalculationState = state;
            CalculationStateValue = stateValue;
        }

        public long State {
            get => selector() ? state0 : state1;
            set {
                if (selector())
                    state0 = value;
                else
                    state1 = value;
            }
        }

        public long CalculationState {
            get => selector() ? state1 : state0;
            set {
                if (selector())
                    state1 = value;
                else
                    state0 = value;
            }
        }

        public float StateValue {
            get => selector() ? stateValue0 : stateValue1;
            set {
                if (selector())
                    stateValue0 = value;
                else
                    stateValue1 = value;
            }
        }

        public float CalculationStateValue {
            get => selector() ? stateValue1 : stateValue0;
            set {
                if (selector())
                    stateValue1 = value;
                else
                    stateValue0 = value;
            }
        }

        internal (long, float) ToCellData => (State, StateValue);
    }

}
