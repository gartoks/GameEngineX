using System;
using System.Collections.Generic;
using System.Linq;
using GameEngineX.Resources.ResourceLoaders;
using GameEngineX.Utility.Exceptions;

namespace GameEngineX.Resources {
    public static class ResourceManager {
        private static readonly Dictionary<Type, Dictionary<Type, IResourceLoader>> loaders;

        private static readonly Dictionary<string, (Type type, IResource res)> globalResources;
        private static readonly Dictionary<string, (Type type, IResource res)> sceneResources;

        private static readonly SortedDictionary<int, Queue<ResourceLoadingTask>> loadingQueue;
        private static int loadingQueueSize;
        private static int highestPriority;

        static ResourceManager() {
            ResourceManager.loaders = new Dictionary<Type, Dictionary<Type, IResourceLoader>>();

            ResourceManager.globalResources = new Dictionary<string, (Type, IResource)>();
            ResourceManager.sceneResources = new Dictionary<string, (Type, IResource)>();

            ResourceManager.loadingQueue = new SortedDictionary<int, Queue<ResourceLoadingTask>>();
            ResourceManager.loadingQueueSize = 0;
            ResourceManager.highestPriority = -1;
        }

        internal static void Initialize() {
            RegisterResourceLoader(new TextLoader());
            //RegisterResourceLoader(new TextureLoader());
            //RegisterResourceLoader(new TextureAtlasLoader());

        }

        internal static void ContinueLoading() {
            if (!IsLoading)
                return;

            while (ResourceManager.highestPriority >= 0 && (!ResourceManager.loadingQueue.ContainsKey(ResourceManager.highestPriority) || ResourceManager.loadingQueue[ResourceManager.highestPriority].Count == 0))
                ResourceManager.highestPriority--;

            if (ResourceManager.highestPriority < 0)
                return;

            ResourceLoadingTask resourceLoadingTask;
            lock (ResourceManager.loadingQueue) {
                resourceLoadingTask = ResourceManager.loadingQueue[ResourceManager.highestPriority].Dequeue();
                loadingQueueSize--;
                //ResourceManager.highestPriority = ResourceManager.loadingQueue.Max(x => x.Value.Count == 0 ? int.MinValue : x.Key);
            }

            Console.WriteLine("Starting Loading " + resourceLoadingTask.ResourceIdentifier);

            IResourceLoader loader = GetLoader(resourceLoadingTask.ResourceType, resourceLoadingTask.ResourceLoadingParametersType);

            if (loader == null)
                throw new ResourceLoadingException($"Resource loader for resource of type '{resourceLoadingTask.ResourceType}' and loading parameters of type '{resourceLoadingTask.ResourceLoadingParametersType}' does not exist.");

            IResourceLoadingParameters resourceLoadingParameters = resourceLoadingTask.ResourceLoadingParameters;
            IResource resource = loader.LoadResource(resourceLoadingTask.ResourceIdentifier, resourceLoadingParameters);

            if (resourceLoadingTask.GlobalResource)
                ResourceManager.globalResources[resourceLoadingTask.ResourceIdentifier] = (resourceLoadingTask.ResourceType, resource);
            else
                ResourceManager.sceneResources[resourceLoadingTask.ResourceIdentifier] = (resourceLoadingTask.ResourceType, resource);
        }

        public static bool GetResource<R>(string resourceIdentifer, out R resource, bool waitForLoading = true) {
            resource = default(R);

            (Type type, IResource res) resourceData;
            if (ResourceManager.sceneResources.TryGetValue(resourceIdentifer, out resourceData)) {
            } else if (ResourceManager.globalResources.TryGetValue(resourceIdentifer, out resourceData)) {
            } else return false;

            if (resourceData.type != typeof(R))
                return false;

            Resource<R> res = resourceData.res as Resource<R>;

            if (res.IsLoaded) {
                resource = res.Data;
                return true;
            }

            if (!waitForLoading)
                return false;

            res.WaitForLoading();   // TODO make it work

            resource = res.Data;

            return true;
        }

        public static void LoadResource<R, RLP>(string resourceIdentifier, RLP resourceLoadingParameters, int loadingPriority, bool globalResource = false) where RLP : ResourceLoadingParameters<R> {
            ResourceLoadingTask loadingTask = new ResourceLoadingTask(resourceIdentifier, typeof(RLP), resourceLoadingParameters, typeof(R), globalResource);
            if (!ResourceManager.loadingQueue.ContainsKey(loadingPriority))
                ResourceManager.loadingQueue.Add(loadingPriority, new Queue<ResourceLoadingTask>());

            Queue<ResourceLoadingTask> queue = ResourceManager.loadingQueue[loadingPriority];

            int hPrio = Math.Max(loadingPriority, ResourceManager.loadingQueue.Max(x => x.Value.Count == 0 ? int.MinValue : x.Key));
            lock (ResourceManager.loadingQueue) {
                queue.Enqueue(loadingTask);
                ResourceManager.loadingQueueSize++;

                ResourceManager.highestPriority = hPrio;
            }
        }

        public static void UnloadResource(string resourceIdentifier) {
            if (ResourceManager.globalResources.ContainsKey(resourceIdentifier))
                ResourceManager.globalResources.Remove(resourceIdentifier);
        }

        internal static void ClearSceneResources() {
            ResourceManager.sceneResources.Clear();
        }

        public static void RegisterResourceLoader<R, RLP>(ResourceLoader<R, RLP> loader) where RLP : ResourceLoadingParameters<R> {
            Type resourceType = typeof(R);
            Type resourceLoadingParameterType = typeof(RLP);

            if (!ResourceManager.loaders.ContainsKey(resourceType))
                ResourceManager.loaders[resourceType] = new Dictionary<Type, IResourceLoader>();

            ResourceManager.loaders[resourceType][resourceLoadingParameterType] = loader;
        }

        private static ResourceLoader<R, RLP> GetLoader<R, RLP>() where RLP : ResourceLoadingParameters<R> {
            Type resourceType = typeof(R);
            Type resourceLoadingParameterType = typeof(RLP);

            return (ResourceLoader<R, RLP>)GetLoader(resourceType, resourceLoadingParameterType);
        }

        private static IResourceLoader GetLoader(Type resourceType, Type resourceLoadingParameterType) {
            if (!ResourceManager.loaders.TryGetValue(resourceType, out Dictionary<Type, IResourceLoader> tmp))
                return null;

            if (!tmp.TryGetValue(resourceLoadingParameterType, out IResourceLoader loader))
                return null;

            return loader;
        }

        public static bool IsLoading => ResourceManager.loadingQueueSize > 0;

    }
}