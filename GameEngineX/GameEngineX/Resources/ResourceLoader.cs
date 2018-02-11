using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

        public Resource<R> LoadResourceAsync(string resourceIdentifier, RLP loadingParameters, Action<Resource<R>> onLoadingCompleteCallback = null) {
            Task<R> task = new Task<R>(() => Load(loadingParameters.FilePaths, loadingParameters));

            Resource<R> resource = new Resource<R>(resourceIdentifier, loadingParameters.FilePaths, task);

            FieldInfo resourceDataField = resource.GetType().GetField("data", BindingFlags.NonPublic | BindingFlags.Instance);

            task.GetAwaiter().OnCompleted(() => {
                resourceDataField.SetValue(resource, task.Result);
                onLoadingCompleteCallback?.Invoke(resource);
            });

            task.Start();

            return resource;
        }

        public IEnumerable<Resource<R>> LoadResourceBatch(IEnumerable<(string resourceIdentifier, RLP lP)> loadingData) {
            return loadingData.Select(l => LoadResource(l.resourceIdentifier, l.lP)).ToList();
        }

        public IEnumerable<Resource<R>> LoadResourceBatchAsync(IEnumerable<(string resourceIdentifier, RLP lP)> loadingData, Action<Resource<R>> onLoadingCompleteCallback = null) {
            return loadingData.Select(loadingParameter => LoadResourceAsync(loadingParameter.resourceIdentifier, loadingParameter.lP, onLoadingCompleteCallback)).ToList();
        }
    }
}