using System;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.GameObjects {
    public class Transform : ISerializable {
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

        internal Transform(SerializationInfo info, StreamingContext ctxt) {
            GameObject = (GameObject)info.GetValue(nameof(GameObject), typeof(GameObject));
            position = (Vector2)info.GetValue(nameof(position), typeof(Vector2));
            scale = (Vector2)info.GetValue(nameof(scale), typeof(Vector2));
            rotation = (float)info.GetValue(nameof(rotation), typeof(float));
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

            m.Multiply(LocalTransformationMatrix, MatrixOrder.Append);

            return m;
        }

        internal Matrix LocalTransformationMatrix {
            get {
                //if (this.transformationMatrix == null) {
                Matrix m = new Matrix();
                m.Scale(Scale.X, Scale.Y);
                m.Rotate(-Rotation * MathUtility.RadToDegf, MatrixOrder.Append);
                m.Translate(LocalPosition.X, -LocalPosition.Y, MatrixOrder.Append);
                //}

                return m/*.Clone()*/;
            }
        }

        public override string ToString() => $"[{Position},{Scale},{Rotation}]";

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(GameObject), GameObject);
            info.AddValue(nameof(position), position);
            info.AddValue(nameof(scale), scale);
            info.AddValue(nameof(rotation), rotation);
        }
    }
}
