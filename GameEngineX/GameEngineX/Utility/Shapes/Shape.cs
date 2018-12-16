using GameEngineX.Game.GameObjects;
using GameEngineX.Utility.Math;

namespace GameEngineX.Utility.Shapes {
    public abstract class Shape {

        public abstract bool Contains(Transform t, Vector2 p);


    }
}