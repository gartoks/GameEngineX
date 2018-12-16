using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Graphics.Renderables;
using GameEngineX.Utility.Math;
using GameEngineX.Utility.Shapes;

namespace TestGame.Pathfinding {
    public class Node : GameObjectComponent {

        private Color color = Color.DarkTurquoise;
        private CircleShape shape;
        private Sprite sprite;
        public Collider Collider { get; private set; }

        private HashSet<Edge> edges;

        public override void Initialize() {
            this.edges = new HashSet<Edge>();

            float radius = 5;

            shape = new CircleShape(color, radius);
            sprite = GameObject.AddComponent<Sprite>();
            sprite.Renderable = shape;

            Circle colliderShape = new Circle(Vector2.ZERO, radius);
            Collider = GameObject.AddComponent<Collider>();
            Collider.Shape = colliderShape;
            Collider.OnCollisionEnter += (me, other, data) => shape.Color = Color.DodgerBlue;
            Collider.OnCollisionExit += (me, other, data) => shape.Color = this.color;
        }

        internal void AddEdge(Edge edge) {
            this.edges.Add(edge);
        }

        public bool IsConnectedTo(Node node) => ConnectedNodes.Any(n => n == node);

        public IEnumerable<Node> ConnectedNodes => Edges.Select(e => e.Other(this));

        public IEnumerable<Edge> Edges => this.edges;

        public Color Color {
            set {
                this.color = value;

                shape.Color = color;
                
            }
        }

    }
}