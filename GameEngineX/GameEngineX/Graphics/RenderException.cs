using System;

namespace GameEngineX.Graphics {
    public class RenderException : Exception {
        public RenderException(string message)
            : base(message) {
        }
    }
}