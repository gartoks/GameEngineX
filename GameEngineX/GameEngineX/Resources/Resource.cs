using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameEngineX.Utility.Exceptions;

namespace GameEngineX.Resources {
    public interface IResource { }

    public sealed class Resource<T> : IResource {

        public readonly string ID;

        public readonly IEnumerable<string> FilePaths;

        private readonly Task<T> loadTask;

        private T data;

        internal Resource(string id, IEnumerable<string> filePaths, T data) {
            ID = id ?? throw new ArgumentNullException(nameof(id), "A resource identifier cannot be null");

            FilePaths = filePaths;

            loadTask = null;
            this.data = data;
        }

        internal Resource(string id, IEnumerable<string> filePaths, Task<T> loadTask) {
            ID = id ?? throw new ArgumentNullException(nameof(id), "A resource identifier cannot be null");

            FilePaths = filePaths;

            this.loadTask = loadTask;
        }

        public T Data {
            get {
                if (!IsLoaded)
                    throw new ResourceLoadingException($"Resource '{ID}' is not loaded yet.");

                return data;
            }
        }

        public void WaitForLoading() {
            if (IsLoaded)
                return;
            Task.WaitAll(loadTask);
        }

        public bool IsLoaded => loadTask == null || loadTask.IsCompleted;
    }
}