using System;
using System.Collections.Generic;
using System.Linq;
using GameEngineX.Resources;
using SimpleINI;

namespace TestGame.CellularAutomatas {
    public class CellularAutomataStateLoadingParameters : ResourceLoadingParameters<CellularAutomataInitializationData> {

        public CellularAutomataStateLoadingParameters(IEnumerable<string> filePaths)
            : base(filePaths) {

            if (filePaths.Count() != 1)
                throw new ArgumentException("A cellular automata state resource must have exactly one file.");
        }
    }

    public class CellularAutomataStateLoader : ResourceLoader<CellularAutomataInitializationData, CellularAutomataStateLoadingParameters> {
        public override CellularAutomataInitializationData Load(IEnumerable<string> filePaths, CellularAutomataStateLoadingParameters loadingParameters) {
            return Load(filePaths.Single(), loadingParameters);
        }

        public static CellularAutomataInitializationData Load(string file, CellularAutomataStateLoadingParameters loadingParameters) {
            INIData iniData = INIReader.Read(file);
            int seed = iniData.GetInt("Seed", "Initialization");
            bool isTorus = iniData.GetBool("IsTorus", "Initialization");
            string nbhMode_raw = iniData.GetString("NeighbourhoodMode", "Initialization");
            NeighbourhoodMode nbhMode = (NeighbourhoodMode)Enum.Parse(typeof(NeighbourhoodMode), nbhMode_raw);
            bool hasData = iniData.GetBool("HasData", "Initialization");
            int cellCount = iniData.GetInt("CellCount", "Initialization");

            if (!hasData)
                return new CellularAutomataInitializationData(seed, cellCount, isTorus, nbhMode);

            (long state, float stateValue)[,] cellData = new(long state, float stateValue)[cellCount, cellCount];
            for (int y = 0; y < cellCount; y++) {
                for (int x = 0; x < cellCount; x++) {
                    long state = iniData.GetLong($"{x}_{y}_state", "CellData");
                    float stateValue = iniData.GetFloat($"{x}_{y}_stateValue", "CellData");
                    cellData[x, y] = (state, stateValue);
                }
            }

            return new CellularAutomataInitializationData(seed, cellData, isTorus, nbhMode);
        }

        public static void Write(string filePath, CellularAutomataInitializationData data) {
            INIData iniData = new INIData();
            iniData.SetInt(data.Seed, "Seed", "Initialization");
            iniData.SetBool(data.IsTorus, "IsTorus", "Initialization");
            iniData.SetString(data.NeighbourhoodMode.ToString(), "NeighbourhoodMode", "Initialization");
            iniData.SetInt(data.CellCount, "CellCount", "Initialization");
            iniData.SetBool(data.IsReadOnly(), "HasData", "Initialization");

            if (data.IsReadOnly()) {
                for (int y = 0; y < data.CellCount; y++) {
                    for (int x = 0; x < data.CellCount; x++) {
                        iniData.SetLong(data.CellData[x, y].state, $"{x}_{y}_state", "CellData");
                        iniData.SetFloat(data.CellData[x, y].stateValue, $"{x}_{y}_stateValue", "CellData");
                    }
                }
            }

            INIWriter.Write(iniData, filePath);
        }
    }
}