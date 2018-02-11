using System;

namespace GameEngineX.Utility.Exceptions {
    public class ResourceLoadingException : Exception {
        public ResourceLoadingException(string msg)
            : base(msg) { }
    }
}
