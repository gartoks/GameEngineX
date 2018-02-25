using System.Drawing;
using System.Runtime.Serialization;
using GameEngineX.Application;

namespace GameEngineX.Graphics {
    internal class ScreenRenderTarget : IRenderTarget {

        public ScreenRenderTarget() {
        }

        public ScreenRenderTarget(SerializationInfo info, StreamingContext ctxt) {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
        }

        public System.Drawing.Graphics Graphics => System.Drawing.Graphics.FromHwnd(ApplicationBase.Instance.GameArea.Handle);

        public Rectangle TargetRectangle => ApplicationBase.Instance.GameArea.ClientRectangle;
    }
}
