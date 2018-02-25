using System;

namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    public abstract class GUIInteractableComponent : GUIComponent {

        public bool Enabled = true;

        public event Action<float, float> OnMouseClicked;
        public event Action<float, float> OnMouseDown;
        public event Action<float, float> OnMouseReleased;

        internal void InvokeMouseClicked(float mouseX, float mouseY) {
            if (Enabled)
                OnMouseClicked?.Invoke(mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

        internal void InvokeMouseDown(float mouseX, float mouseY) {
            if (Enabled)
                OnMouseDown?.Invoke(mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

        internal void InvokeMouseReleased(float mouseX, float mouseY) {
            if (Enabled)
                OnMouseReleased?.Invoke(mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

    }
}
