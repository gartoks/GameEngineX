using System.Collections.Generic;
using System.Drawing;

namespace GameEngineX.Graphics.Renderables.Textures {
    public class TextureAtlas : Texture2D {
        private readonly Dictionary<string, SubTexture> subTextures;

        internal TextureAtlas(Image image)
            : base(image) {

            this.subTextures = new Dictionary<string, SubTexture>();
        }

        public void AddRegion(string identifier, int x, int y, int width, int height) {
            SubTexture tex = new SubTexture(this, (x, y, width, height));

            this.subTextures[identifier] = tex;
        }

        public Texture GetSubTexture(string identifier) {
            if (this.subTextures.TryGetValue(identifier, out SubTexture tex))
                return tex;

            return null;
        }
    }
}
