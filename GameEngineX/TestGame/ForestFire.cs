using System;
using System.Drawing;
using GameEngineX.Utility;

namespace TestGame {
    public class ForestFire : CellularAutomata {

        private const int CELLS = 100;

        private const long STATE_TREE = 1;
        private const long STATE_FIRE = 2;
        private const long ASH = 4;
        private const long STATE_WATER = 8;

        private const float INIT_TREE_CHANCE = 0.5f;
        private const float INIT_WATER_CHANCE = 0.0f;
        private const float TREE_SEEDLING_CHANCE = 0.05f;
        private const float TREE_REGROW_CHANCE = 0.0075f;
        private const float FIRE_CHANCE = 0.00025f;
        private const float ASH_DECOMPOSITION_BASE_FACTOR = 0.2f;
        private const float ASH_DECOMPOSITION_TREE_FACTOR = 0.2f;
        private const float GROWTH_FACTOR_BASE = 0.05f;
        private const float ASH_GROWTH_FACTOR = 0.05f;
        private const float FIRE_DIMINISHING_BASE_FACTOR = 0.2f;
        private const float FIRE_DIMINISHING_WATER_FACTOR = 0.1f;

        public override void Initialize() {
            base.Initialize();

            Init(CELLS, true, NeighbourhoodMode.Moore);

            Setup();
        }

        protected override void StepComplete() {
        }

        protected override void GenerateCellState(Random random, int x, int y, out long state, out float stateValue) {
            state = STATE_TREE;
            stateValue = 0;

            if (random.NextDouble() < INIT_WATER_CHANCE) {
                stateValue = 0;
                state = STATE_WATER;
            } else if (random.NextDouble() < INIT_TREE_CHANCE) {
                state = STATE_TREE;
                stateValue = 0;
            }
        }

        protected override void CalculateNewState(int x, int y, long currentState, float currentStateValue, Random random, out long state, out float stateValue) {
            state = currentState;
            stateValue = currentStateValue;

            if (currentState == STATE_EMPTY) {
                //int nb_water = Neighbours(x, y, STATE_WATER);
                //if (random.NextDouble() < 0.1f * INIT_WATER_CHANCE * (nb_water + 1))
                //    return STATE_WATER;
                int nb_tree = Neighbours(x, y, STATE_TREE);
                if (random.NextDouble() < TREE_SEEDLING_CHANCE * nb_tree || random.NextDouble() < TREE_REGROW_CHANCE) {
                    state = STATE_TREE;
                    stateValue = 0;
                }
            } else if (currentState == ASH) {
                int nb_tree = Neighbours(x, y, STATE_TREE);
                float decomposition = ASH_DECOMPOSITION_BASE_FACTOR + nb_tree * ASH_DECOMPOSITION_TREE_FACTOR;
                if (currentStateValue <= decomposition) {
                    state = STATE_EMPTY;
                    stateValue = 0;
                } else {
                    stateValue = currentStateValue - decomposition;
                }
            } else if (currentState == STATE_TREE) {
                int nb_fire = Neighbours(x, y, STATE_FIRE);
                float nbv_fire = NeighbourStateValue(x, y, STATE_FIRE, (v, a) => v + a);
                int nb_water = Neighbours(x, y, STATE_WATER);
                if ((nb_fire > nb_water && random.NextDouble() < 1 * nbv_fire) || random.NextDouble() < FIRE_CHANCE) {
                    stateValue = currentStateValue;
                    state = STATE_FIRE;
                } else if (currentStateValue < 1) {
                    int nb_ash = Neighbours(x, y, ASH);
                    float growth = GROWTH_FACTOR_BASE + nb_ash * ASH_GROWTH_FACTOR;

                    stateValue = Math.Min(1, currentStateValue + growth);
                }
            } else if (currentState == STATE_FIRE) {
                int nb_water = Neighbours(x, y, STATE_WATER);
                float diminishing = FIRE_DIMINISHING_BASE_FACTOR + nb_water * FIRE_DIMINISHING_WATER_FACTOR;
                if (currentStateValue <= diminishing) {
                    state = ASH;
                    stateValue = 1;
                } else {
                    stateValue = currentStateValue - diminishing;
                }
            }
        }

        private static Color COLOR_EMPTY = Color.FromArgb(0, 15, 0);
        private static Color COLOR_ASH = Color.FromArgb(63, 63, 63);
        protected  override Color GetStateColor(long state, float stateValue) {
            switch (state) {
                case STATE_TREE: return CalculateTreeColor(state, stateValue);
                case STATE_FIRE: return ColorUtility.LerpColors(Color.DarkRed, Color.Yellow, stateValue);
                case ASH: return ColorUtility.LerpColors(COLOR_EMPTY, COLOR_ASH, stateValue);
                case STATE_WATER: return Color.DodgerBlue;
                default: return COLOR_EMPTY;
            }
        }

        private Color CalculateTreeColor(long state, float stateValue) {
            float threshold = 0.2f;
            if (stateValue < threshold)
                return ColorUtility.LerpColors(COLOR_EMPTY, Color.ForestGreen, stateValue / threshold);
            else
                return ColorUtility.LerpColors(Color.ForestGreen, Color.DarkGreen, (stateValue - threshold) / (1f - threshold));
        }

        protected override string CellStateToString(long state) {
            switch (state) {
                case STATE_TREE: return "Tree";
                case STATE_FIRE: return "Fire";
                case ASH: return "Ash";
                case STATE_WATER: return "Water";
                default: return "Empty";
            }
        }

    }
}
