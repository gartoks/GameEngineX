using System;

namespace GameEngineX.Utility {
    public class PingPong {
        private float min = float.MinValue;
        private float max = float.MaxValue;

        private float value;
        private float sign;

        public PingPong()
            : this(0, 1) { }

        public PingPong(float min, float max) {
            Min = min;
            Max = max;

            this.value = 0;
            this.sign = 1;
        }

        public void Update(float deltaTime) {
            this.value += this.sign * deltaTime;

            if (this.sign < 0 && this.value < 0) {
                this.value = System.Math.Abs(this.value);
                this.sign = 1;
            } else if (this.sign > 0 && this.value > 1) {
                this.value = 2f - this.value;
                this.sign = -1;
            }
        }

        public void Reverse() {
            this.sign *= -1;
        }

        public float Value => Min + this.value * (Max - Min);

        public float Min {
            get => this.min;
            set {
                if (value > Max)
                    throw new ArgumentException("The PingPong minimum must be smaller than the maximum.", nameof(value));
                this.min = value;
            }
        }

        public float Max {
            get => this.max;
            set {
                if (value < Min)
                    throw new ArgumentException("The PingPong maximum must be bigger than the minimum.", nameof(value));
                this.max = value;
            }
        }
    }
}
