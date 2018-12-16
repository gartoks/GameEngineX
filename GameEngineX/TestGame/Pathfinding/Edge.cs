using System.Collections.Generic;
using System.Drawing;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Graphics.Renderables;
using GameEngineX.Utility.Math;

namespace TestGame.Pathfinding {
    public class Edge : GameObjectComponent {

        private Color color = Color.Aquamarine;

        private Sprite sprite;
        private Node from;  // set via initialize
        private Node to;    // set via initialize

        private Node[] nodes;
        public float Length { get; private set; }

        public override void Initialize() {
            this.nodes = new[] {from, to};

            from.AddEdge(this);
            to.AddEdge(this);

            Length = Vector2.Distance(from.Transform.Position, to.Transform.Position);

            Vector2 fromPos = from.Transform.Position;
            Vector2 toPos = to.Transform.Position;
            Vector2 pos = fromPos + 0.5f * (toPos - fromPos);
            Transform.Position.Set(pos);

            sprite = GameObject.AddComponent<Sprite>();
            sprite.RenderLayer = 1;
            sprite.Renderable = new CustomShape((renderLayer, renderer) => {
                Vector2 start = fromPos - Transform.Position;
                Vector2 end = toPos - Transform.Position;
                renderer.DrawLine(renderLayer, color, 1f, start, end);
            });
        }

        public Node Other(Node node) {
            if (!ConnectsNode(node))
                return null;

            if (node == from)
                return to;
            else
                return from;
        }

        public bool ConnectsNode(Node node) => node == to || node == from;

        public IEnumerable<Node> Nodes => this.nodes;

        public Color Color {
            set => this.color = value;
        }
    }
}