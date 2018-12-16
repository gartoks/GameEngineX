using System;
using GameEngineX.Game.Animation;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.GameObjects.GameObjectComponents {
    public class Animation : GameObjectComponent {

        private IAnimationTimer animationTimer;
        private float runTime;
        public Action<Animation> Animated;

        public bool Paused;
        private float time;
        private float value;

        public override void Initialize() {
            this.animationTimer = new OneTimeTimer();

            RunTime = 1;
            Paused = true;
        }

        public override void Update(float deltaTime) {
            if (Paused)
                return;

            this.time += deltaTime;
            this.time = MathUtility.Clamp(time, 0, RunTime);

            this.value = animationTimer.Value(time / RunTime, 1f);

            Animated?.Invoke(this);
        }

        public void Reset() {
            this.time = 0;
        }

        public float Value => this.value;

        public IAnimationTimer AnimationTimer {
            get => this.animationTimer;
            set {
                this.animationTimer = value ?? throw new ArgumentNullException(nameof(value));

                this.value = float.NaN;
            }
        }

        public float RunTime {
            get => this.runTime;
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this.runTime = value;
                this.time = 0;
            }
        }

        public float Time {
            get => this.time;
            set {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this.time = MathUtility.Clamp(value, 0, RunTime);
            }
        }
    }
}