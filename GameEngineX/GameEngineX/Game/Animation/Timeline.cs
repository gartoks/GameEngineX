using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngineX.Game.Animation {
    public sealed class Timeline {
        public static readonly IAnimationTimer DefaultAnimationTimer = new OneTimeTimer();
        public static readonly InterpolatorDelegate DefaultInterpolator = (t, min, max) => {
            if (t < 0 || t > 1)
                throw new ArgumentOutOfRangeException(nameof(t), "The interpolation selector must be between 0 and 1 (inclusive)");

            if (min > max)
                throw new ArgumentOutOfRangeException(nameof(min), "The minimum must be bigger than the maximum.");

            return min + t * (max - min);
        };

        public delegate float InterpolatorDelegate(float t, float min, float max);

        private readonly SortedDictionary<float, float> keyframes;

        private IAnimationTimer animationTimer;
        private InterpolatorDelegate interpolator;

        public Timeline(IAnimationTimer animationTimer, InterpolatorDelegate interpolator) {
            this.keyframes = new SortedDictionary<float, float>();

            AnimationTimer = animationTimer;
            Interpolator = interpolator;
        }

        public float GetValue(float time) {
            return GetValue(time, AnimationTimer, Interpolator);
        }

        public float GetValue(float time, IAnimationTimer animationTimer, InterpolatorDelegate interpolator) {
            if (!this.keyframes.Any())
                return 0;

            KeyValuePair<float, float> minTime = this.keyframes.First();
            KeyValuePair<float, float> maxTime = this.keyframes.Last();

            float startTime = StartingTime;
            time = startTime + AnimationTimer.Value(time, FinishingTime - startTime);

            if (time <= minTime.Key)
                return minTime.Value;

            if (time >= maxTime.Key)
                return maxTime.Value;

            KeyValuePair<float, float> prevTime = this.keyframes.First(kf => kf.Key >= time);
            KeyValuePair<float, float> succTime = this.keyframes.First(kf => kf.Key > prevTime.Value);

            float dt = succTime.Key - prevTime.Key;
            float t = (time - prevTime.Key) / dt;

            return interpolator.Invoke(t, prevTime.Value, succTime.Value);
        }

        public void AddKeyframe(float time, float value) {
            if (time < 0)
                throw new ArgumentOutOfRangeException();

            this.keyframes[time] = value;
        }

        public void RemoveKeyframe(float time) {
            if (time < 0)
                throw new ArgumentOutOfRangeException();

            this.keyframes.Remove(time);
        }

        public void RemoveKeyframe(int keyframeIndex) {
            if (keyframeIndex < 0 || keyframeIndex >= this.keyframes.Count)
                throw new IndexOutOfRangeException();

            KeyValuePair<float, float> t = this.keyframes.ElementAt(keyframeIndex);
            RemoveKeyframe(t.Key);
        }

        public void Clear() {
            this.keyframes.Clear();
        }

        public IAnimationTimer AnimationTimer {
            get => this.animationTimer;
            set {
                if (value == null)
                    value = DefaultAnimationTimer;

                this.animationTimer = value;
            }
        }

        public InterpolatorDelegate Interpolator {
            get => this.interpolator;
            set {
                if (value == null)
                    value = DefaultInterpolator;

                this.interpolator = value;
            }
        }

        public float StartingTime => this.keyframes.Any() ? this.keyframes.First().Key : 0;

        public float FinishingTime => this.keyframes.Any() ? this.keyframes.Last().Key : 0;

        public int Count => this.keyframes.Count;

        public IEnumerable<(float, float)> Keyframes => this.keyframes.Select(kf => (kf.Key, kf.Value));

    }
}