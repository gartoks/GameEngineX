using System.Drawing;

namespace GameEngineX.Graphics.Textures {
    public class Texture2D : Texture, IRenderTarget {

        protected readonly Image image;
        protected readonly Rectangle bounds;

        internal Texture2D(int width, int height)
            : this(new Bitmap(width, height)) { }

        internal Texture2D(Image image) {
            this.image = image;
            this.bounds = new Rectangle(0, 0, image.Width, image.Height);
        }

        internal override void Render(int renderLayer, Renderer renderer) {
            renderer.DrawTexture(renderLayer, Image, 0, 0, Width, Height);
        }

        public override int Width => Image.Width;

        public override int Height => Image.Height;

        internal override Image Image => this.image;

        public System.Drawing.Graphics Graphics => System.Drawing.Graphics.FromImage(Image);

        public Rectangle TargetRectangle => this.bounds;
    }
}
