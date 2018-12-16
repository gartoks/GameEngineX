using System;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.Animation {
    public sealed class OneTimeTimer : IAnimationTimer {

        public OneTimeTimer() { }

        public float Value(float time, float max) {
            if (max < 0)
                throw new ArgumentOutOfRangeException(nameof(max), "The OneTimeTimer maximum must be bigger than zero.");

            time = MathUtility.Clamp(time, 0, max);

            return time;
        }
    }
}