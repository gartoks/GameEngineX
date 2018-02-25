using System;
using System.Drawing;

namespace GameEngineX.Graphics.Renderables {
    public class CustomShape : Renderable {

        private readonly Action<int, Renderer> render;

        public CustomShape(Action<int, Renderer> render)
            : base(Color.White, 1) {

            this.render = render;
        }

        internal override void Render(int renderLayer, Renderer renderer) {
            render(renderLayer, renderer);
        }
    }
}
