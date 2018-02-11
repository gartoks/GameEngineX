namespace GameEngineX.Graphics {
    public interface IRendering {

        int RenderLayer { get; }

        Renderable Renderable { get; }
    }
}
