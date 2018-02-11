using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngineX.Resources {
    public interface IResourceLoadingParameters {
    }

    public abstract class ResourceLoadingParameters<T> : IResourceLoadingParameters {
        public readonly IEnumerable<string> FilePaths;

        protected ResourceLoadingParameters(IEnumerable<string> filePaths) {
            if (filePaths == null || !filePaths.Any() || filePaths.Any(string.IsNullOrEmpty))
                throw new ArgumentException("Invalid resource file paths.");

            FilePaths = filePaths;
        }
    }
}
