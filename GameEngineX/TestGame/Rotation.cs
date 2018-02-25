using System;
using GameEngineX.Application;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Utility.Math;

namespace TestGame {
    public class Rotation : GameObjectComponent {

        public float InitialAngle;

        public float Radius;
        public float AngularVelocity;

        public override void Initialize() {
            InitialAngle = 0;
            Radius = 15;
            AngularVelocity = 0.25f * MathUtility.PI;
        }

        public override void Update(float deltaTime) {
            float angle = InitialAngle + (ApplicationBase.Instance.Time * AngularVelocity).NormalizeAngle();
            MathUtility.NormalizeAngle(ref angle);

            //Transform.Rotation = angle;
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            Transform.Position.X = Radius * c;
            Transform.Position.Y = Radius * s;
        }
    }
}
