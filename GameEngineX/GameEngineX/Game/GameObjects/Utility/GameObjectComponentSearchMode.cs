using System;

namespace GameEngineX.Game.GameObjects.Utility {
    [Flags]
    public enum GameObjectComponentSearchMode {
        This = 0, ParentalHierarchy = 1, ChildHierarchy = 2
    }
}