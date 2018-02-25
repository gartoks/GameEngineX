using System.Collections.Generic;
using GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface;
using GameEngineX.Game.GameObjects.Utility;
using GameEngineX.Game.UserInterface;
using GameEngineX.Input;

namespace GameEngineX.Game.GameObjects.GameObjectComponents {
    [RequiredComponents(true, typeof(Viewport))]
    public sealed class GUIHandler : GameObjectComponent {

        public Viewport Viewport { get; private set; }
        private List<GUIComponent> hoveredComponents;

        public override void Initialize() {
            this.Viewport = GameObject.GetRootGameObject().GetComponent<Viewport>(GameObjectComponentSearchMode.ParentalHierarchy);
            this.hoveredComponents = new List<GUIComponent>();
        }

        public override void Death() {
            
        }

        public override void Update(float deltaTime) {
            (int x, int y) mousePosition_raw = InputHandler.MousePosition;
            (float x, float y) mousePos = Viewport.ScreenToWorldCoordinates((mousePosition_raw.x, mousePosition_raw.y));

            IEnumerable<GUIComponent> guiComponents = GameObject.GetComponents<GUIComponent>(GameObjectComponentSearchMode.ChildHierarchy, true);

            bool mouseButtonPressed = InputHandler.IsMouseButtonPressed(MouseButton.Left) || InputHandler.IsMouseButtonPressed(MouseButton.Right);
            bool mouseButtonDown = InputHandler.IsMouseButtonDown(MouseButton.Left) || InputHandler.IsMouseButtonDown(MouseButton.Right);
            bool mouseButtonReleased = InputHandler.IsMouseButtonReleased(MouseButton.Left) || InputHandler.IsMouseButtonReleased(MouseButton.Right);

            foreach (GUIComponent guiComponent in guiComponents) {
                bool isHovered = guiComponent.Bounds.Contains(mousePos.x, mousePos.y);
                bool wasHovered = hoveredComponents.Contains(guiComponent);

                if (isHovered && !wasHovered) {
                    hoveredComponents.Add(guiComponent);
                    guiComponent.InvokeMouseEntered(mousePos.x, mousePos.y);
                    guiComponent.InteractionState = GUIComponentInteractionState.Hovered;

                    if ((mouseButtonPressed || mouseButtonDown) && guiComponent is GUIInteractableComponent gIC) {
                        gIC.InvokeMouseClicked(mousePos.x, mousePos.y);
                        gIC.InteractionState = GUIComponentInteractionState.Clicked;
                    }
                } else if (!isHovered && wasHovered) {
                    hoveredComponents.Remove(guiComponent);
                    guiComponent.InvokeMouseExited(mousePos.x, mousePos.y);
                    guiComponent.InteractionState = GUIComponentInteractionState.None;

                    if ((mouseButtonDown || mouseButtonReleased) && guiComponent is GUIInteractableComponent gIC)
                        gIC.InvokeMouseReleased(mousePos.x, mousePos.y);
                } else if (isHovered) {
                    guiComponent.InvokeMouseHovering(mousePos.x, mousePos.y);

                    if (guiComponent is GUIInteractableComponent gIC) {
                        if (mouseButtonPressed) {
                            gIC.InvokeMouseClicked(mousePos.x, mousePos.y);
                            gIC.InteractionState = GUIComponentInteractionState.Clicked;
                        } else if (mouseButtonReleased) {
                            gIC.InvokeMouseReleased(mousePos.x, mousePos.y);
                            gIC.InteractionState = GUIComponentInteractionState.Hovered;
                        } else if (mouseButtonDown) {
                            gIC.InvokeMouseDown(mousePos.x, mousePos.y);
                        }
                    }
                }


            }

            //for (int i = hoveredComponents.Count - 1; i >= 0; i--) {
            //    GUIComponent gC = hoveredComponents[i];

            //    if (!gC.Bounds.RectangleContains(mousePos.x, mousePos.y)) {
            //        hoveredComponents.Remove(gC);
            //        gC.InvokeMouseExited();
            //    }
            //}

            //bool mouseButtonDown = InputHandler.IsMouseButtonDown(MouseButton.Left) || InputHandler.IsMouseButtonDown(MouseButton.Right);
            //bool mouseButtonPressed = InputHandler.IsMouseButtonPressed(MouseButton.Left) || InputHandler.IsMouseButtonPressed(MouseButton.Right);

            //foreach (GUIComponent guiComponent in guiComponents) {
            //    bool mouseOver = guiComponent.Bounds.RectangleContains(mousePos.x, mousePos.y);

            //    if (mouseOver && !hoveredComponents.Contains(guiComponent)) {
            //        this.hoveredComponents.Add(guiComponent);
            //        guiComponent.InvokeMouseEntered();
                    
            //        if (mouseButtonDown && guiComponent is GUIInteractableComponent gIC)
            //            gIC.InvokeMouseClicked();
            //    }
            //}


        }
        

    }
}