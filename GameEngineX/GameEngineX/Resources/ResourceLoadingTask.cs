using System;

namespace GameEngineX.Resources {
    internal class ResourceLoadingTask {

        public readonly string ResourceIdentifier;
        public readonly Type ResourceLoadingParametersType;
        public readonly IResourceLoadingParameters ResourceLoadingParameters;
        public readonly Type ResourceType;
        public readonly bool GlobalResource;

        internal ResourceLoadingTask(string resourceIdentifier, Type resourceLoadingParametersType, IResourceLoadingParameters resourceLoadingParameters, Type resourceType, bool globalResource) {
            if (string.IsNullOrEmpty(resourceIdentifier))
                throw new ArgumentException("Invalid resource identifier.", nameof(resourceIdentifier));

            ResourceIdentifier = resourceIdentifier;
            ResourceLoadingParametersType = resourceLoadingParametersType;
            ResourceLoadingParameters = resourceLoadingParameters;
            ResourceType = resourceType;
            GlobalResource = globalResource;
        }

        //void a<T>() {
        //    Type t = typeof(T);
        //    Type t2 = typeof(Resource<>).MakeGenericType(t);
        //    IResource s = null;
        //    Resource<T> r = (Resource<T>)Convert.ChangeType(s, t2);
        //}

    }
}
