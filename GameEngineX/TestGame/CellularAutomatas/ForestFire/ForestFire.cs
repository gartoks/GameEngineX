using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using GameEngineX.Utility;

namespace TestGame.CellularAutomatas.ForestFire {
    public class ForestFire : CellularAutomata {
        private static string FILE_PATH = "forestFire.ca";

        private const int CELLS = 100;

        private const long STATE_TREE = 1;
        private const long STATE_FIRE = 2;
        private const long STATE_ASH = 4;
        private const long STATE_WATER = 8;

        private const float INIT_TREE_CHANCE = 0.5f;
        private const float INIT_WATER_CHANCE = 0.0f;
        private const float TREE_SEEDLING_CHANCE = 0.05f;
        private const float TREE_REGROW_CHANCE = 0.0075f;
        private const float TREE_REGROW_FACTOR = 0.015f;
        private const float FIRE_CHANCE = 0.00025f;
        private const float ASH_DECOMPOSITION_BASE_FACTOR = 0.2f;
        private const float ASH_DECOMPOSITION_TREE_FACTOR = 0.2f;
        private const float GROWTH_FACTOR_BASE = 0.05f;
        private const float ASH_GROWTH_FACTOR = 0.05f;
        private const float FIRE_DIMINISHING_BASE_FACTOR = 0.2f;
        private const float FIRE_DIMINISHING_WATER_FACTOR = 0.1f;
        private const float SAPLING_SPREAD_CHANCE = 0.005f;
        private const float SAPLING_MUTATION_CHANCE = 0.1f;
        private const float SAPLING_MUTATION_STD_DEV = 1;

        public override void Initialize() {
            base.Initialize();

            //ResourceManager.RegisterResourceLoader(new CellularAutomataStateLoader());

            //ResourceManager.LoadResource<CellularAutomataInitializationData, CellularAutomataStateLoadingParameters>(
            //    "CAStateData",
            //    new CellularAutomataStateLoadingParameters(new [] {FILE_PATH}), 0, false);

            //ResourceManager.GetResource("CAStateData", out CellularAutomataInitializationData initData, true);

            //CellularAutomataInitializationData initData = CellularAutomataStateLoader.Load(SavePath, new CellularAutomataStateLoadingParamyeters(new[] { FILE_PATH }));
            CellularAutomataInitializationData initData = new CellularAutomataInitializationData(0, CELLS, true, NeighbourhoodMode.Moore);

            Init(initData);
        }

        private static float totalUtility;
        protected override void StepComplete() {
            Debug.WriteLine($"Total Utility: {totalUtility / StateCount(STATE_TREE)}");

            totalUtility = 0;
        }

        public override IEnumerable<long> States => new[] { STATE_EMPTY, STATE_FIRE, STATE_TREE, STATE_WATER, STATE_ASH };

        protected override void GenerateCellState(Random random, int x, int y, out long state, out float stateValue, out object cellData) {

            ForestFireCellData ffCellData = new ForestFireCellData(null);
            cellData = ffCellData;
            state = STATE_EMPTY;
            stateValue = 0;

            if (random.NextDouble() < INIT_WATER_CHANCE) {
                stateValue = 0;
                state = STATE_WATER;
            } else if (random.NextDouble() < INIT_TREE_CHANCE) {
                state = STATE_TREE;
                ffCellData.Grow(random);
                stateValue = 0;
            }
        }

        protected override void CalculateNewState(int x, int y, long currentState, float currentStateValue, object currentCellData, float deltaTime, Random random, out long state, out float stateValue, out object cellData) {
            ForestFireCellData ffCellData = (ForestFireCellData)currentCellData;
            state = currentState;
            stateValue = currentStateValue;
            cellData = ffCellData;

            if (currentState == STATE_TREE)
                totalUtility += ffCellData.TreeData.Utility;

            switch (currentState) {
                case STATE_EMPTY: {
                        int nb_tree = Neighbours(x, y, STATE_TREE);
                        if (random.NextDouble() < TREE_SEEDLING_CHANCE * nb_tree || random.NextDouble() < TREE_REGROW_CHANCE) {

                    //if (random.NextDouble() < ffCellData.SproutChance(random) * TREE_REGROW_FACTOR) {
                        ffCellData.Grow(random);
                        state = STATE_TREE;
                        stateValue = 0;
                    }
                    break;
                }
                case STATE_ASH: {
                    int nb_tree = Neighbours(x, y, STATE_TREE);
                    float decomposition = ASH_DECOMPOSITION_BASE_FACTOR + nb_tree * ASH_DECOMPOSITION_TREE_FACTOR;
                    if (currentStateValue <= decomposition) {
                        state = STATE_EMPTY;
                        stateValue = 0;
                    } else {
                        stateValue = Math.Max(0, currentStateValue - decomposition);
                    }
                    break;
                }
                case STATE_TREE: {
                    int nb_fire = Neighbours(x, y, STATE_FIRE);
                    float nbv_fire = NeighbourStateValue(x, y, STATE_FIRE, (v, a) => v + a);
                    int nb_water = Neighbours(x, y, STATE_WATER);
                    if ((nb_fire > nb_water && random.NextDouble() < 1 * nbv_fire) || random.NextDouble() < FIRE_CHANCE) {
                        stateValue = currentStateValue;
                        state = STATE_FIRE;
                        ffCellData.Burn();
                    } else {
                        //foreach (object cD in NeighbourCellData(x, y)) {
                        //    ForestFireCellData ffCD = (ForestFireCellData)cD;

                        //    if (ffCD.TreeData == null)
                        //        continue;

                        //    if (random.NextDouble() >= SAPLING_SPREAD_CHANCE)
                        //        continue;

                        //    SaplingData newSapling = ffCD.TreeData.Mutate(random, SAPLING_MUTATION_CHANCE, SAPLING_MUTATION_STD_DEV);
                        //    ffCellData.Add(newSapling);
                        //}

                        if (currentStateValue < 1) {
                            int nb_ash = Neighbours(x, y, STATE_ASH);
                            float growth = GROWTH_FACTOR_BASE + nb_ash * ASH_GROWTH_FACTOR;
                            //float growth = ffCellData.TreeData.GrowModifier * GROWTH_FACTOR_BASE + nb_ash * ASH_GROWTH_FACTOR;

                            stateValue = Math.Min(1, currentStateValue + growth);
                        }
                    }}
                    break;
                case STATE_FIRE: {
                    int nb_water = Neighbours(x, y, STATE_WATER);
                    float diminishing = FIRE_DIMINISHING_BASE_FACTOR + nb_water * FIRE_DIMINISHING_WATER_FACTOR;
                    if (currentStateValue <= diminishing) {
                        state = STATE_ASH;
                        stateValue = 1;
                    } else {
                        stateValue = Math.Max(0, currentStateValue - diminishing);
                    }
                    break;
                }
            }
        }

        private static Color COLOR_EMPTY = Color.FromArgb(0, 15, 0);
        private static Color COLOR_ASH = Color.FromArgb(63, 63, 63);
        protected  override Color GetStateColor(long state, float stateValue) {
            switch (state) {
                case STATE_TREE: return CalculateTreeColor(state, stateValue);
                case STATE_FIRE: return ColorUtility.LerpColors(Color.DarkRed, Color.Yellow, stateValue);
                case STATE_ASH: return ColorUtility.LerpColors(COLOR_EMPTY, COLOR_ASH, stateValue);
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
                case STATE_ASH: return "Ash";
                case STATE_WATER: return "Water";
                default: return "Empty";
            }
        }

        protected override string SavePath => FILE_PATH;

    }
}
