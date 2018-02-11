using System;
using System.Drawing.Drawing2D;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.GameObjects {
    public class Transform {
        public readonly GameObject GameObject;

        private readonly Vector2 position = new Vector2();
        private readonly Vector2 scale = new Vector2(1, 1);
        private float rotation;

        //public bool UITransform;

        internal Transform(GameObject gO)
            : this(gO, new Vector2(), 0, new Vector2(1, 1)) { }

        internal Transform(GameObject gO, Vector2 position, float rotation = 0, Vector2 scale = null) {
            GameObject = gO;
            Position = position == null ? new Vector2() : position;
            Rotation = rotation;
            Scale = scale == null ? new Vector2(1, 1) : scale;

            //UITransform = false;
        }

        public Vector2 LocalPosition {
            get {
                if (GameObject.Parent == null)
                    return Position;

                return Position - GameObject.Parent.Transform.Position;
            }

            set {
                if (GameObject.Parent == null)
                    Position = value;
                else
                    Position = GameObject.Parent.Transform.Position + value;
            }
        }

        public Vector2 Position {
            get => this.position;
            set {
                if (value == null)
                    throw new ArgumentNullException();

                this.position.Set(value);
                //this.transformationMatrix = null;
            }
        }

        public float Rotation {
            get => this.rotation;
            set {
                this.rotation = value;
                //this.transformationMatrix = null;

                while (this.rotation < 0)
                    this.rotation += MathUtility.TwoPIf;

                this.rotation = this.rotation % MathUtility.TwoPIf;
            }
        }

        public Vector2 Scale {
            get => this.scale;
            set {
                if (value == null)
                    throw new ArgumentNullException();

                if (value.LengthSqr == 0)
                    throw new ArgumentException();

                this.scale.Set(value);
                //this.transformationMatrix = null;
            }
        }

        internal Matrix GlobalTransformationMatrix => CalculateGlobalTransformationMatrix(new Matrix());

        private Matrix CalculateGlobalTransformationMatrix(Matrix m) {
            GameObject.Parent?.Transform.CalculateGlobalTransformationMatrix(m);

            m.Multiply(LocalTransformationMatrix);

            return m;
        }

        internal Matrix LocalTransformationMatrix {
            get {
                //if (this.transformationMatrix == null) {
                Matrix m = new Matrix();
                m.Scale(Scale.X, Scale.Y);
                m.Rotate(-Rotation * MathUtility.RadToDegf, MatrixOrder.Append);// TODO
                m.Translate(LocalPosition.X, -LocalPosition.Y, MatrixOrder.Append);
                //}

                return m/*.Clone()*/;
            }
        }

        //internal XMLElement Serialize() {
        //    XMLElement root = new XMLElement("Transform");

        //    root.AddElement(Position.ToXML("Position"));
        //    root.AddElement(Scale.ToXML("Scale"));
        //    root.AddDataElement("Rotation", Rotation.ToString());

        //    return root;
        //}

        //internal void Deserialize(XMLElement dataElement) {
        //    if (!dataElement.HasElement("Position") || !dataElement.HasElement("Scale") || !dataElement.HasElement("Rotation"))
        //        throw new SerializationException("Cannot parse transform attribute.");

        //    this.position.FromXML(dataElement.GetElement("Position"));
        //    this.scale.FromXML(dataElement.GetElement("Scale"));

        //    float rot = 0;
        //    XMLElement rotElement = dataElement.GetElement("Rotation");
        //    if (!rotElement.HasData || !float.TryParse(rotElement.Data, out rot))
        //        throw new SerializationException("Cannot parse transform attribute.");
        //    this.rotation = rot;
        //}

    }
}
