using System;
using System.Drawing;
using GameEngineX.Graphics;
using GameEngineX.Graphics.Renderables.Textures;

namespace GameEngineX.Game.UserInterface {
    public abstract class GUIInteractionGraphics {
        internal abstract void Render(int renderLayer, Renderer renderer, float x, float y, float width, float height, GUIComponentInteractionState state);
    }

    public sealed class GUIInteractionColors : GUIInteractionGraphics {
        public Color DefaultState;
        public Color HoveredState;
        public Color ClickedState;

        public GUIInteractionColors()
            : this(Color.DarkGray, Color.DarkGray, Color.DarkGray) { }

        public GUIInteractionColors(Color defaultState, Color hoveredState, Color clickedState) {
            DefaultState = defaultState;
            HoveredState = hoveredState;
            ClickedState = clickedState;
        }

        internal override void Render(int renderLayer, Renderer renderer, float x, float y, float width, float height, GUIComponentInteractionState state) {
            renderer.FillRectangle(renderLayer, FindStateColor(state), x, y, width, height);
        }

        private Color FindStateColor(GUIComponentInteractionState state) {
            switch (state) {
                case GUIComponentInteractionState.None:
                    return DefaultState;
                case GUIComponentInteractionState.Hovered:
                    return HoveredState;
                case GUIComponentInteractionState.Clicked:
                    return ClickedState;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }

    public sealed class GUIInteractionTextures : GUIInteractionGraphics {
        public Texture DefaultState;
        public Texture HoveredState;
        public Texture ClickedState;

        public GUIInteractionTextures() {
            Bitmap bmp = new Bitmap(1, 1);
            bmp.SetPixel(0, 0, Color.DarkGray);

            DefaultState = new Texture2D(bmp);
            HoveredState = new Texture2D(bmp);
            ClickedState = new Texture2D(bmp);
        }

        public GUIInteractionTextures(Texture defaultState, Texture hoveredState, Texture clickedState) {
            DefaultState = defaultState;
            HoveredState = hoveredState;
            ClickedState = clickedState;
        }

        internal override void Render(int renderLayer, Renderer renderer, float x, float y, float width, float height, GUIComponentInteractionState state) {
            renderer.DrawTexture(renderLayer, FindStateTexture(state).Image, x, y, width, height);
        }

        private Texture FindStateTexture(GUIComponentInteractionState state) {
            switch (state) {
                case GUIComponentInteractionState.None:
                    return DefaultState;
                case GUIComponentInteractionState.Hovered:
                    return HoveredState;
                case GUIComponentInteractionState.Clicked:
                    return ClickedState;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
