using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GameEngineX.Graphics.Renderables.Textures;

namespace GameEngineX.Resources.ResourceLoaders {
    public class TextureLoadingParameters : ResourceLoadingParameters<Texture> {

        public TextureLoadingParameters(IEnumerable<string> filePaths)
            : base(filePaths) {

            if (filePaths.Count() != 1)
                throw new ArgumentException("A texture resource must have exactly one file.");
        }
    }

    public class TextureLoader : ResourceLoader<Texture, TextureLoadingParameters> {
        public override Texture Load(IEnumerable<string> filePaths, TextureLoadingParameters loadingParameters) {
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
