using System;

namespace GameEngineX.Game.GameObjects.GameObjectComponents {
    public class Timer : GameObjectComponent {
        private float time;
        public float RemainingTime { get; private set; }

        public bool IsRunning { get; private set; }

        public event Action<Timer> OnTimerComplete;
        public event Action<Timer> OnTimerTick;

        public override void Initialize() {
            IsRunning = false;
        }

        public override void Death() {
        }

        public override void Update(float deltaTime) {
            if (IsRunning) {
                RemainingTime = Math.Max(0, RemainingTime - deltaTime);

                OnTimerTick?.Invoke(this);

                if (RemainingTime == 0) {
                    IsRunning = false;
                    OnTimerComplete?.Invoke(this);
                }
            }
        }

        public void Start() {
            if (IsRunning)
                return;

            IsRunning = true;

            RemainingTime = Time;
        }

        public void Stop() {
            if (!IsRunning)
                return;

            IsRunning = false;
            Reset();
        }

        public void Reset() {
            if (IsRunning)
                return;

            RemainingTime = 0;
        }

        public void Resume() {
            if (RemainingTime == 0)
                return;

            IsRunning = true;
        }

        public void Pause() {
            IsRunning = false;
        }

        public float Time {
            get => this.time;
            set {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (IsRunning)
                    return;

                this.time = value;
            }
        }

        public float Percentage => RemainingTime / Time;
    }
}