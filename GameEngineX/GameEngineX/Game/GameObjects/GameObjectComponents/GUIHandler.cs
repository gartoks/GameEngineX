using System.Collections.Generic;
using GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface;
using GameEngineX.Game.GameObjects.Utility;
using GameEngineX.Game.UserInterface;
using GameEngineX.Input;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.GameObjects.GameObjectComponents {
    [RequiredComponents(true, typeof(Viewport))]
    public sealed class GUIHandler : GameObjectComponent {
        public Viewport Viewport { get; private set; }
        private List<GUIComponent> hoveredComponents;

        private GUIInteractableComponent focus;

        public override void Initialize() {
            this.Viewport = GameObject.GetRootGameObject().GetComponent<Viewport>(GameObjectComponentSearchMode.ParentalHierarchy);
            this.hoveredComponents = new List<GUIComponent>();
        }

        public override void Death() {
            
        }

        public override void Update(float deltaTime) {
            Vector2 mousePos = Viewport.ScreenToWorldCoordinates(InputHandler.MousePosition);

            IEnumerable<GUIComponent> guiComponents = GameObject.GetComponents<GUIComponent>(GameObjectComponentSearchMode.ChildHierarchy, true);

            bool mouseButtonPressed = InputHandler.IsMouseButtonPressed(MouseButton.Left) || InputHandler.IsMouseButtonPressed(MouseButton.Right);
            bool mouseButtonDown = InputHandler.IsMouseButtonDown(MouseButton.Left) || InputHandler.IsMouseButtonDown(MouseButton.Right);
            bool mouseButtonReleased = InputHandler.IsMouseButtonReleased(MouseButton.Left) || InputHandler.IsMouseButtonReleased(MouseButton.Right);

            foreach (GUIComponent guiComponent in guiComponents) {
                bool isHovered = guiComponent.Bounds.Contains(mousePos.X, mousePos.Y);
                if (isHovered && guiComponent is GUIButton)
                    guiComponent.Bounds.Contains(mousePos.X, mousePos.Y);
                bool wasHovered = hoveredComponents.Contains(guiComponent);

                if (isHovered && !wasHovered) {
                    hoveredComponents.Add(guiComponent);
                    guiComponent.InteractionState = GUIComponentInteractionState.Hovered;
                    guiComponent.InvokeMouseEntered(mousePos.X, mousePos.Y);

                    if ((mouseButtonPressed || mouseButtonDown) && guiComponent is GUIInteractableComponent gIC) {
                        gIC.InteractionState = GUIComponentInteractionState.Clicked;
                        gIC.InvokeMouseClicked(mousePos.X, mousePos.Y);
                        Focus = gIC;
                    }
                } else if (!isHovered && wasHovered) {
                    hoveredComponents.Remove(guiComponent);
                    guiComponent.InteractionState = GUIComponentInteractionState.None;
                    guiComponent.InvokeMouseExited(mousePos.X, mousePos.Y);

                    if ((mouseButtonDown || mouseButtonReleased) && guiComponent is GUIInteractableComponent gIC)
                        gIC.InvokeMouseReleased(mousePos.X, mousePos.Y);
                } else if (isHovered) {
                    guiComponent.InvokeMouseHovering(mousePos.X, mousePos.Y);

                    if (guiComponent is GUIInteractableComponent gIC) {
                        if (mouseButtonPressed) {
                            gIC.InteractionState = GUIComponentInteractionState.Clicked;
                            gIC.InvokeMouseClicked(mousePos.X, mousePos.Y);
                            Focus = gIC;
                        } else if (mouseButtonReleased) {
                            gIC.InteractionState = GUIComponentInteractionState.Hovered;
                            gIC.InvokeMouseReleased(mousePos.X, mousePos.Y);
                        } else if (mouseButtonDown) {
                            gIC.InvokeMouseDown(mousePos.X, mousePos.Y);
                        }
                    }
                }


            }
        }

        public GUIInteractableComponent Focus {
            get => this.focus;
            set {
                if (this.focus == value)
                    return;

                if (this.focus != null)
                    this.focus.HasFocus = false;

                this.focus = value;

                if (this.focus != null)
                    this.focus.HasFocus = true;
            }
        }
    }
}