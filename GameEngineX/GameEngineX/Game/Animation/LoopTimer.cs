using System;

namespace GameEngineX.Game.Animation {
    public sealed class LoopTimer : IAnimationTimer {

        public LoopTimer() { }

        public float Value(float time, float max) {
            if (max < 0)
                throw new ArgumentOutOfRangeException(nameof(max), "The LoopTimer maximum must be bigger than zero.");

            float div = time / max;
            int loops = (int)div;
            float frac = div - loops;

            return frac * max;
        }

    }
}