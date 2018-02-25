using System;
using GameEngineX.Game.GameObjects.GameObjectComponents;

namespace GameEngineX.Game.UserInterface {
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RequiredComponents : Attribute {
        public Type[] Required { get; set; }
        public bool InHierarchy { get; set; }

        public RequiredComponents(bool inHierarchy, Type req, params Type[] requiredTypes) {
            InHierarchy = inHierarchy;
            Required = new Type[requiredTypes.Length + 1];

            if (!req.IsSubclassOf(typeof(GameObjectComponent)))
                throw new ArgumentException(nameof(req));
            Required[0] = req;

            for (int i = 0; i < requiredTypes.Length; i++) {
                if (!requiredTypes[i].IsSubclassOf(typeof(GameObjectComponent)))
                    throw new ArgumentException(nameof(requiredTypes));

                Required[i + 1] = requiredTypes[i];
            }
        }
    }
}