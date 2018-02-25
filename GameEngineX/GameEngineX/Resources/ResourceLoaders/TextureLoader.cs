using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GameEngineX.Graphics.Renderables.Textures;

namespace GameEngineX.Resources.ResourceLoaders {
    public class TextureLoaderParameters : ResourceLoadingParameters<Texture> {
        public TextureLoaderParameters(IEnumerable<string> filePaths)
            : base(filePaths) {

            if (filePaths.Count() != 1)
                throw new ArgumentException("A texture resource must have exactly one file.");
        }
    }

    public class TextureLoader : ResourceLoader<Texture, TextureLoaderParameters> {
        public override Texture Load(IEnumerable<string> filePaths, TextureLoaderParameters loadingParameters) {
            Image image;
            try {
                image = Image.FromFile(filePaths.Single());
            } catch (Exception) {
                return null;
            }

            return new Texture2D(image);
        }
    }
}
