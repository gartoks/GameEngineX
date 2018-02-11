using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameEngineX.Resources.ResourceLoaders {
    public class TextLoadingParameters : ResourceLoadingParameters<string> {

        public readonly Encoding Encoding;

        public TextLoadingParameters(IEnumerable<string> filePaths, Encoding encoding)
            : base(filePaths) {

            if (filePaths.Count() != 1)
                throw new ArgumentException("A text resource must have exactly one file.");

            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding), "String encoding for a text resource cannot be null");
        }
    }

    public class TextLoader : ResourceLoader<string, TextLoadingParameters> {

        public override string Load(IEnumerable<string> filePaths, TextLoadingParameters loadingParameters) {
            string text;
            try {
                text = File.ReadAllText(filePaths.Single(), loadingParameters.Encoding);
            } catch (Exception) {
                return null;
            }

            return text;
        }
    }
}