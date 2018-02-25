using System.Runtime.Serialization;

namespace GameEngineX.Game.GameObjects.GameObjectComponents {
    public abstract class GameObjectComponent : ISerializable {
        private GameObject gameObject;

        public bool IsEnabled;

        protected GameObjectComponent() {
            IsEnabled = true;
        }

        protected GameObjectComponent(SerializationInfo info, StreamingContext ctxt) {
            gameObject = (GameObject)info.GetValue(nameof(gameObject), typeof(GameObject));
            IsEnabled = info.GetBoolean(nameof(IsEnabled));
        }

        public void Destroy() {
            GameObject.RemoveComponent(this);
        }

        public virtual void Initialize() { }

        public virtual void Death() { }

        public virtual void Update(float deltaTime) { }

        internal bool IsActive => IsEnabled && GameObject.IsEnabled && GameObject.IsAlive;

        public GameObject GameObject {
            get => this.gameObject;
            internal set => this.gameObject = value;
        }

        public Transform Transform => GameObject.Transform;

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(gameObject), gameObject);
            info.AddValue(nameof(IsEnabled), IsEnabled);
        }
    }
}
