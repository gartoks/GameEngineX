using System.Drawing;
using System.Runtime.Serialization;

namespace GameEngineX.Graphics {
    public interface IRenderTarget : ISerializable {
        System.Drawing.Graphics Graphics { get; }

        Rectangle TargetRectangle { get; }
    }
}
