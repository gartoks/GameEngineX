using System;
using System.Collections.Generic;
using System.Linq;

namespace TestGame.CellularAutomatas.CellularEmpire {
    public class CellularEmpireCellData {

        private readonly CellularEmpire Automata;

        public long State { get; private set; }

        private float experience;

        public CellularEmpireCellData(CellularEmpire automata, long state) {
            Automata = automata;
            State = state;
            experience = 0.05f;
        }

        public void Battle(Random random, IEnumerable<CellularEmpireCellData> neighbours, out long newState, out float newStateValue) {
            float[] stateBattleStrength = new float[Automata.States.Count()];
            foreach (CellularEmpireCellData cellularEmpireCellData in neighbours)
                stateBattleStrength[CellularEmpire.StateToIndex(cellularEmpireCellData.State)] += 0.5f * CalculateBattleStrength(cellularEmpireCellData.experience);
            stateBattleStrength[CellularEmpire.StateToIndex(State)] += CalculateBattleStrength(experience);

            float maxStrength = 0;
            long maxStrengthState = -1;
            for (int i = 0; i < stateBattleStrength.Length; i++) {
                float stateStrength = stateBattleStrength[i];
                if (maxStrengthState != -1 && !(maxStrength < stateStrength))
                    continue;

                maxStrength = stateStrength;
                maxStrengthState = CellularEmpire.IndexToState(i);
            }

            if (maxStrengthState == State) {
                experience += (float)Math.Max(0.05, experience * experience);
            } else {
                State = maxStrengthState;
                experience = (float)(random.NextDouble() * maxStrength);
            }

            newState = State;
            newStateValue = StateValue;
        }

        public float StateValue => CalculateBattleStrength(experience);

        private static float CalculateBattleStrength(float x) => 1f - (float)(-2.0f / (1.0f + Math.Exp(-5.5f * x * x)) + 2f);
        
    }
}