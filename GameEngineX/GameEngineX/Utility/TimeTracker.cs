using System;
using System.Threading;

namespace GameEngineX.Utility {
    public class TimeTracker {
        public readonly int TargetsTicksPerSecond;

        private readonly DateTime startTime;
        private int lastTime;
        public float DeltaTime { get; private set; }

        private int tickCounter;
        private int tickTimeTracker;

        public int SleepTime { get; private set; }
        public int TicksPerSecond { get; private set; }

        public TimeTracker(int targetsTickPerSecond) {

            TargetsTicksPerSecond = targetsTickPerSecond;

            this.startTime = DateTime.UtcNow;

            this.lastTime = RunTimeMilliseconds() - TargetSleepTime;
            DeltaTime = TargetsTicksPerSecond / 1000f;

            this.tickCounter = 0;
            this.tickTimeTracker = 0;
            TicksPerSecond = TargetsTicksPerSecond;
        }

        public void EarlyTick() {
            int currentTime = RunTimeMilliseconds();
            int deltaTime = currentTime - this.lastTime;
            this.lastTime = currentTime;

            DeltaTime = deltaTime / 1000f;

            this.tickTimeTracker += deltaTime;
            this.tickCounter++;
            if (this.tickTimeTracker < 1000)
                return;

            TicksPerSecond = this.tickCounter;

            this.tickTimeTracker -= 1000;
            this.tickCounter = 0;
        }

        public void LateTick() {
            int currentTime = RunTimeMilliseconds();
            SleepTime = System.Math.Max(0, TargetSleepTime - (currentTime - this.lastTime));
        }

        public void FullTick(Action<float> updateCallback, bool sleep = true) {
            EarlyTick();

            updateCallback?.Invoke(DeltaTime);

            LateTick();

            if (sleep && SleepTime > 0)
                Thread.Sleep(SleepTime);
        }

        public int RunTimeMilliseconds() {
            return (int)(DateTime.UtcNow - this.startTime).TotalMilliseconds;
        }

        private int TargetSleepTime => TargetsTicksPerSecond <= 0 ? 0 : (int)(1000f / TargetsTicksPerSecond);
    }
}