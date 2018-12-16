using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestGame.CellularAutomatas.ForestFire {
    public class ForestFireCellData {
        private static float BestUtility = float.MinValue;

        private readonly List<SaplingData> saplings;
        public SaplingData TreeData;

        public ForestFireCellData(SaplingData treeData) {
            saplings = new List<SaplingData>();
            TreeData = treeData;
        }

        public float SproutChance(Random random) {
            return Best(random).SproutModifier;
        }

        public void Grow(Random random) {
            TreeData = Best(random);

            saplings.Clear();
        }

        public void Burn() {
            TreeData = null;
        }

        public void Add(SaplingData sapling) {
            if (saplings.Contains(sapling))
                return;

            int i = 0;
            if (saplings.Count > 0) {
                while (i < saplings.Count && saplings[i].Utility > sapling.Utility)
                    i++;
            }

            saplings.Insert(i, sapling);

            if (i == 0 && BestUtility < sapling.Utility) {
                BestUtility = sapling.Utility;
                Debug.WriteLine(sapling);
            }
        }

        private SaplingData Best(Random random) {
            if (saplings.Count == 0)
                Add(new SaplingData(random));

            return saplings[0];
        }
    }
}