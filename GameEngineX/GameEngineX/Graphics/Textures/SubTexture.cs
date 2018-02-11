using System.Drawing;

namespace GameEngineX.Graphics.Textures {
    public class SubTexture : Texture {
        private readonly TextureAtlas textureAtlas;
        private readonly Rectangle bounds;

        internal SubTexture(TextureAtlas textureAtlas, (int x, int y, int width, int height) area) {
            this.textureAtlas = textureAtlas;

            this.bounds = new Rectangle(area.x, area.y, area.width, area.height);
        }

        internal override void Render(int renderLayer, Renderer renderer) {
            renderer.DrawTexture(renderLayer, Image, this.bounds.X, this.bounds.Y, this.bounds.Width, this.bounds.Height, 0, 0, Width, Height);
        }

        public override int Width => this.bounds.Width;

        public override int Height => this.bounds.Height;

        internal override Image Image => this.textureAtlas.Image;
    }
}
