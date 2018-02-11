namespace GameEngineX.Utility {
    public class PingPong {
        private float sign;

        public PingPong() {
            this.Value = 0;
            this.sign = 1;
        }

        public void Update(float deltaTime) {
            Value += this.sign * deltaTime;

            if (this.sign < 0 && Value < 0) {
                Value = System.Math.Abs(Value);
                this.sign = 1;
            } else if (this.sign > 0 && Value > 1) {
                Value = 2f - Value;
                this.sign = -1;
            }
        }

        public void Reverse() {
            this.sign *= -1;
        }

        public float Value { get; private set; }
    }
}
