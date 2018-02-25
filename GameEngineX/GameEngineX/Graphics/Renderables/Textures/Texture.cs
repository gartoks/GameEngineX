using System.Drawing;

namespace GameEngineX.Graphics.Renderables.Textures {
    public abstract class Texture : Renderable {

        public static Texture2D CreateTextureDisplay(int width, int height) {
            return new Texture2D(width, height);
        }

        protected Texture()
            : base(Color.White) { }

        public abstract int Width { get; }

        public abstract int Height { get; }

        internal abstract Image Image { get; }
    }
}
