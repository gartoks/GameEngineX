using System;

namespace GameEngineX.Utility.Exceptions {
    public class GameStateException : Exception {
        public GameStateException(string msg)
            : base(msg) { }
    }
}
