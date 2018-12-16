using System;
using System.Collections.Generic;

namespace GameEngineX.Resources {
    public interface IResource { }

    public sealed class Resource<T> : IResource {

        public readonly string ID;

        public readonly IEnumerable<string> FilePaths;

        public T Data { get; }

        internal Resource(string id, IEnumerable<string> filePaths, T data) {
            ID = id ?? throw new ArgumentNullException(nameof(id), "A resource identifier cannot be null");
            FilePaths = filePaths;
            this.Data = data;
        }
    }
}