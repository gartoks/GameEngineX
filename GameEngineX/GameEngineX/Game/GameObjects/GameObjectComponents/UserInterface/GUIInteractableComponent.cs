using System;

namespace GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface {
    public abstract class GUIInteractableComponent : GUIComponent {

        public bool Enabled = true;

        public bool HasFocus { get; internal set; }

        public event Action<GUIComponent, float, float> OnMouseClicked;
        public event Action<GUIComponent, float, float> OnMouseDown;
        public event Action<GUIComponent, float, float> OnMouseReleased;

        internal void InvokeMouseClicked(float mouseX, float mouseY) {
            if (Enabled)
                OnMouseClicked?.Invoke(this, mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

        internal void InvokeMouseDown(float mouseX, float mouseY) {
            if (Enabled)
                OnMouseDown?.Invoke(this, mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

        internal void InvokeMouseReleased(float mouseX, float mouseY) {
            if (Enabled)
                OnMouseReleased?.Invoke(this, mouseX - Transform.Position.X, mouseY - Transform.Position.Y);
        }

    }
}
