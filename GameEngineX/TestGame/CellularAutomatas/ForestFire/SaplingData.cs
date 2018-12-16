using System;
using System.Collections.Generic;
using GameEngineX.Utility.Math;

namespace TestGame.CellularAutomatas.ForestFire {
    public class SaplingData {
        private readonly Guid id;

        public readonly float GrowModifier;
        public readonly float SproutModifier;
        public readonly float FireResistanceModifier;

        private readonly float GrowthValue;
        private readonly float SproutValue;
        private readonly float FireResistanceValue;


        public SaplingData(float growthValue, float sproutValue, float fireResistanceValue) {
            id = Guid.NewGuid();

            GrowthValue = growthValue;
            SproutValue = sproutValue;
            FireResistanceValue = fireResistanceValue;

            GrowModifier = SigmoidFunction(growthValue);
            SproutModifier = SigmoidFunction(sproutValue);
            FireResistanceModifier = SigmoidFunction(fireResistanceValue);
        }

        public SaplingData(Random random) {
            id = Guid.NewGuid();
            GrowModifier = SigmoidFunction((float)random.NextDouble());
            SproutModifier = SigmoidFunction((float)random.NextDouble());
            FireResistanceModifier = SigmoidFunction((float)random.NextDouble());
        }

        public SaplingData Mutate(Random random, float mutationChance, float mutationStdDev) {
            float gV = GrowthValue;
            float sV = SproutValue;
            float frV = FireResistanceValue;

            if (random.NextDouble() < mutationChance)
                gV += (float)random.NextGaussian(0, mutationStdDev);
            if (random.NextDouble() < mutationChance)
                sV += (float)random.NextGaussian(0, mutationStdDev);
            if (random.NextDouble() < mutationChance)
                frV += (float)random.NextGaussian(0, mutationStdDev);

            return new SaplingData(gV, sV, frV);
        }

        public float Utility => GrowModifier + SproutModifier + FireResistanceModifier;

        private static float SigmoidFunction(float growthValue) => Math.Max(0, (1f / (1f + (float)Math.Exp(-growthValue)) - 0.5f) / 0.5f);

        public override string ToString() {
            return $"[U:{Utility} GM:{GrowModifier} SM:{SproutModifier} FRM:{FireResistanceModifier}]";
        }

        public override bool Equals(object obj) => obj is SaplingData set && this.id.Equals(set.id);

        public override int GetHashCode() => 1877310944 + EqualityComparer<Guid>.Default.GetHashCode(this.id);
    }
}