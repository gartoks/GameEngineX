using System;

namespace TestGame.CellularAutomatas {
    public class CellularAutomataInitializationData {
        private int seed;
        private int cellCount;
        private bool isTorus;
        private NeighbourhoodMode neighbourhoodMode;

        public (long state, float stateValue)[,] CellData { get; }

        public CellularAutomataInitializationData(int seed, int cellCount, bool isTorus, NeighbourhoodMode neighbourhoodMode) {
            this.seed = seed;
            this.cellCount = cellCount;
            this.isTorus = isTorus;
            this.neighbourhoodMode = neighbourhoodMode;
        }

        internal CellularAutomataInitializationData(int seed, (long, float)[,] cellData, bool isTorus, NeighbourhoodMode neighbourhoodMode) {
            this.seed = seed;

            if (cellData.GetLength(0) != cellData.GetLength(1))
                throw new ArgumentException("The CA grid must be square.", nameof(cellData));
            CellData = cellData;
            this.cellCount = cellData.GetLength(0);
            this.isTorus = isTorus;
            this.neighbourhoodMode = neighbourhoodMode;
        }

        public int Seed {
            get => this.seed;
            set {
                if (IsReadOnly())
                    return;

                this.seed = value;
            }
        }

        public int CellCount {
            get => this.cellCount;
            set {
                if (IsReadOnly())
                    return;

                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "The cell count must be bigger than zero.");

                this.cellCount = value;
            }
        }

        public bool IsTorus {
            get => this.isTorus;
            set {
                if (IsReadOnly())
                    return;

                this.isTorus = value;
            }
        }


        public NeighbourhoodMode NeighbourhoodMode {
            get => this.neighbourhoodMode;
            set {
                if (IsReadOnly())
                    return;

                this.neighbourhoodMode = value;
            }
        }

        public bool IsReadOnly() => CellData != null;
    }

}
