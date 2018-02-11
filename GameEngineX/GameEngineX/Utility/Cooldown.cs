namespace GameEngineX.Utility {
    public class Cooldown {
        private float time;
        private float cooldown;

        private bool justReadied;

        public Cooldown(float cooldown) {
            this.time = cooldown;
            this.cooldown = 0;

            this.justReadied = false;
        }

        public bool Consume() {
            if (!IsReady)
                return false;

            Reset();

            return true;
        }

        public bool UpdateConsume(float deltaTime) {
            Update(deltaTime);

            return Consume();
        }

        public void Update(float deltaTime) {
            float prevCooldown = RemainingCooldown;

            this.cooldown = System.Math.Max(0, this.cooldown - deltaTime);

            this.justReadied = prevCooldown != 0 && RemainingCooldown == 0;
        }

        public void Reset() {
            this.cooldown = this.time;
        }

        public bool IsReady => this.cooldown == 0;

        public bool JustReadied => this.justReadied;

        public float RemainingCooldown => this.cooldown;

        public float CooldownTime {
            get => this.time;
            set {
                this.time = value;

                this.cooldown = System.Math.Min(RemainingCooldown, CooldownTime);
            }
        }

        public float RemainingCooldownPercentage => RemainingCooldown / CooldownTime;
    }
}
