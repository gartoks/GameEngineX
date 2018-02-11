using System.Drawing;

namespace GameEngineX.Graphics {
    public interface IRenderTarget {
        System.Drawing.Graphics Graphics { get; }

        Rectangle TargetRectangle { get; }
    }
}
