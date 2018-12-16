using System.Collections.Generic;
using System.Linq;

namespace GameEngineX.Resources {
    internal interface IResourceLoader {
        IResource LoadResource(string resourceIdentifier, IResourceLoadingParameters loadingParameters);
    }

    public abstract class ResourceLoader<R, RLP> : IResourceLoader where RLP : ResourceLoadingParameters<R> {

        public abstract R Load(IEnumerable<string> filePaths, RLP loadingParameters);

        public Resource<R> LoadResource(string resourceIdentifier, RLP loadingParameters) {
            R value = Load(loadingParameters.FilePaths, loadingParameters);

            return new Resource<R>(resourceIdentifier, loadingParameters.FilePaths, value);
        }

        public IResource LoadResource(string resourceIdentifier, IResourceLoadingParameters loadingParameters) {
            return LoadResource(resourceIdentifier, (RLP)loadingParameters);
        }

        public IEnumerable<Resource<R>> LoadResourceBatch(IEnumerable<(string resourceIdentifier, RLP lP)> loadingData) {
            return loadingData.Select(l => LoadResource(l.resourceIdentifier, l.lP)).ToList();
        }
    }
}