using System;
using GameEngineX.Game.GameObjects.Utility;
using GameEngineX.Input;
using GameEngineX.Utility.Math;
using GameEngineX.Utility.Shapes;

namespace GameEngineX.Game.GameObjects.GameObjectComponents {
    public class Collider : GameObjectComponent {

        public delegate void CollisionEventHandler(Collider me, Collider other, CollisionData collisionData);

        public event CollisionEventHandler OnCollisionEnter;
        public event CollisionEventHandler OnCollisionExit;

        private bool contained;
        private Shape shape;

        public override void Initialize() {
            this.shape = new Circle(Vector2.ZERO, 10);

            //OnCollisionEnter += (me, other, data) => Log.WriteLine($"{GameObject.Name} - Enter - {GetType().Name}");
            //OnCollisionExit += (me, other, data) => Log.WriteLine($"{GameObject.Name} - Exit - {GetType().Name}");
        }

        public override void Death() {
        }

        public override void Update(float deltaTime) {
            Vector2 mousePos = Scene.Active.MainViewport.ScreenToWorldCoordinates(InputHandler.MousePosition);

            // TODO

            bool contains = this.shape.Contains(GameObject.Transform, mousePos);
            if (contains && !contained)
                OnCollisionEnter?.Invoke(this, null, null); // TODO
            else if (!contains && contained)
                OnCollisionExit?.Invoke(this, null, null); // TODO

            this.contained = contains;
        }

        public Shape Shape {
            get => this.shape;
            set => this.shape = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool Contains => this.contained;
    }
}