using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Graphics;
using GameEngineX.Utility.Exceptions;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.GameObjects {
    public sealed class GameObject {
        private static uint NextID = 0;

        private readonly uint ID;

        private bool isAlive;

        private string name;

        public bool IsEnabled;

        private readonly List<GameObjectComponent> components;
        private readonly List<IRendering> renderables;

        private GameObject parent;
        private readonly HashSet<GameObject> children;

        public Transform Transform { get; }

        public GameObject(string name)
            : this(name, null, 0, null) { }

        public GameObject(string name, Vector2 position = null, float rotation = 0, Vector2 scale = null)
            : this(name, null, position, rotation, scale) { }

        public GameObject(string name, GameObject parent = null, Vector2 position = null, float rotation = 0, Vector2 scale = null)
            : this(name, parent, position, rotation, scale, GameBase.Instance.ActiveSceneObj) {
        }

        internal GameObject(string name, GameObject parent = null, Vector2 position = null, float rotation = 0, Vector2 scale = null, Scene scene = null) {
            if (scene == null)
                scene = GameBase.Instance.ActiveSceneObj;

            if (position == null)
                position = new Vector2();

            if (scale == null)
                scale = new Vector2(1, 1);

            ID = NextID++;

            Name = name;
            IsEnabled = true;

            this.components = new List<GameObjectComponent>();
            this.renderables = new List<IRendering>();

            Transform = new Transform(this, position, rotation, scale);

            Parent = parent;
            this.children = new HashSet<GameObject>();

            if (GameBase.Instance.ActiveSceneObj == null)
                throw new GameStateException("Cannote create GameObject when no scene is active.");

            scene.AddGameObject(this);

            this.isAlive = true;
        }

        //Position.OnChanged += (v, x, y) => {
        //    Collider.Polygon.SetOffset(Position);
        //};

        //public virtual CollisionResult Collide(GameObject gameObject, Vector2 translationAmount) {
        //    return Collider.Collide(gameObject.Collider, translationAmount);
        //}

        private readonly List<GameObject> updatingChildren = new List<GameObject>();
        internal void Update(float deltaTime) {
            if (!IsEnabled)
                return;

            this.updatingChildren.Clear();
            this.updatingChildren.AddRange(Children);

            foreach (GameObjectComponent component in this.components) {
                if (!component.IsEnabled)
                    continue;

                component.Update(deltaTime);
            }

            foreach (GameObject child in this.updatingChildren) {
                child.Update(deltaTime);
            }

            /*if (!Rigidbody.IsKinematic)
                Speed.Y -= Data.PHYSICS_GRAV * Rigidbody.Mass;

            appliedVelocity.Set(Speed).Scale(deltaTime);

            // collision handling
            if (!Rigidbody.IsKinematic) {
                for (int i = 0; i < Game.Instance.GameObjects.Count(); i++) {
                    GameObject gO = Game.Instance.GameObjects.ElementAt(i);

                    if (this == gO)
                        continue;

                    Vector2 relativeTranslation = new Vector2(gO.Speed).Subtract(Speed).Scale(deltaTime);
                    CollisionResult cR = Collide(gO, relativeTranslation);

                    if (cR.IsTriggerCollision)
                        continue;

                    if (cR.WillIntersect) {
                        appliedVelocity.Add(cR.TranslationVector);

                        if (cR.TranslationVector.X != 0)
                            Speed.X = 0;

                        if (cR.TranslationVector.Y != 0)
                            Speed.Y = 0;
                    }

                }

            }

            Position.Add(appliedVelocity);

            Speed.Set(0, 0);

            //if (wouldHaveIntersected)
            //    Speed.Set();*/
        }

        private readonly List<GameObject> renderingChildren = new List<GameObject>();
        internal void Render(Renderer renderer) {
            if (!IsEnabled)
                return;

            renderer.ApplyTransformation(Transform);

            foreach (IRendering r in this.renderables) {
                if (!((GameObjectComponent)r).IsEnabled)
                    continue;

                r.Renderable.Render(r.RenderLayer, renderer);
            }

            this.renderingChildren.Clear();
            this.renderingChildren.AddRange(Children);

            foreach (GameObject child in this.renderingChildren) {
                child.Render(renderer);
            }

            renderer.RevertTransform();
        }

        public T AddComponent<T>(params (string fieldName, object fieldValue)[] initializationData) where T : GameObjectComponent {
            return (T)AddComponent(typeof(T), true, initializationData);
        }

        internal GameObjectComponent AddComponent(Type t, bool isEnabled, IEnumerable<(string fieldName, object fielValue)> fields) {
            ConstructorInfo ctor = t.GetConstructor(Type.EmptyTypes);
            GameObjectComponent component = ctor.Invoke(new object[0]) as GameObjectComponent;
            component.GameObject = this;

            this.components.Add(component);

            component.IsEnabled = isEnabled;

            component.Initialize();

            if (component is IRendering r) {
                this.renderables.Add(r);
                this.renderables.Sort((x, y) => y.RenderLayer.CompareTo(x.RenderLayer));
            }

            if (fields == null)
                return component;

            foreach ((string fieldName, object fielValue) fieldData in fields) {
                FieldInfo field = t.GetField(fieldData.fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (field == null)
                    throw new SerializationException("Cannot initialize component field.");

                field.SetValue(component, fieldData.fielValue);
            }

            return component;
        }

        internal void RemoveComponent(GameObjectComponent component) {
            if (!this.components.Contains(component))
                return;

            component.Death();

            this.components.Remove(component);
            if (!(component is IRendering r))
                return;

            this.renderables.Remove(r);
            this.renderables.Sort((x, y) => y.RenderLayer.CompareTo(x.RenderLayer));
        }

        public T GetComponent<T>() {
            return GetComponents<T>().FirstOrDefault();
        }

        public IEnumerable<T> GetComponents<T>(bool includeChildren = false) {
            IEnumerable<T> comps = FindComponents(c => c is T).Cast<T>();

            if (!includeChildren)
                return comps;

            foreach (GameObject child in this.children) {
                comps = comps.Concat(child.GetComponents<T>(true));
            }

            return comps;
        }

        public IEnumerable<GameObjectComponent> FindComponents(Func<GameObjectComponent, bool> selector, bool includeChildren = false) {
            IEnumerable<GameObjectComponent> comps = this.components.Where(selector);

            if (!includeChildren)
                return comps;

            foreach (GameObject child in this.children) {
                comps = comps.Concat(child.FindComponents(selector, true));
            }

            return comps;
        }

        public GameObject Parent {
            get => this.parent;
            set {
                if (Equals(Parent, value))
                    return;
                
                if (value != null)
                    value.MakeChild(this);
                else
                    Parent?.UnmakeChild(this);
            }
        }

        private void MakeChild(GameObject child) {
            if (child == null)
                throw new ArgumentNullException();

            if (Children.Contains(child))
                throw new GameStateException("Cannot make child. Is already child of this GameObject.");

            this.children.Add(child);
            child.parent = this;
        }

        private void UnmakeChild(GameObject child) {
            if (child == null)
                throw new ArgumentNullException();

            if (!Children.Contains(child))
                throw new GameStateException("Cannot unmake child. Is not child of this GameObject.");

            this.children.Remove(child);
            child.parent = null;
        }

        public IEnumerable<GameObject> Children => this.children;

        public IEnumerable<GameObject> FindChildren(Func<GameObject, bool> selector, bool includeChildrensChildren = false) {
            IEnumerable<GameObject> chs = this.children.Where(selector);

            if (!includeChildrensChildren)
                return chs;

            foreach (GameObject child in this.children) {
                chs = chs.Concat(child.FindChildren(selector, true));
            }

            return chs;
        }

        public void Kill() {
            Destroy();
        }

        public void Destroy() {
            this.isAlive = false;

            foreach (GameObjectComponent component in this.components) {
                component.Destroy();    // TODO may not work as it modifies the list
            }
        }

        public bool IsAlive => this.isAlive;

        public string Name {
            get => this.name;
            set => this.name = string.IsNullOrEmpty(value) ? "GameObject" : value;
        }

        public override bool Equals(object obj) {
            if (obj == null || !(obj is GameObject gO))
                return false;

            return ID.Equals(gO.ID);
        }

        public override int GetHashCode() {
            return ID.GetHashCode();
        }

        //internal XMLElement Serialize() {
        //    XMLElement root = new XMLElement("GameObject");

        //    root.AddDataElement("IsAlive", IsAlive.ToString());
        //    root.AddDataElement("Name", Name);
        //    root.AddDataElement("IsEnabled", IsEnabled.ToString());

        //    root.AddElement(Transform.Serialize());

        //    XMLElement componentsElement = new XMLElement("Components");
        //    root.AddElement(componentsElement);
        //    foreach (GameObjectComponent component in this.components) {
        //        componentsElement.AddElement(component.Serialize());
        //    }

        //    XMLElement childrenElement = new XMLElement("Children");
        //    root.AddElement(childrenElement);
        //    foreach (GameObject child in Children) {
        //        childrenElement.AddElement(child.Serialize());
        //    }

        //    return root;
        //}

        //internal static GameObject Deserialize(XMLElement dataElement, Scene scene) {

        //    if (!dataElement.GetElement("Name").HasData)
        //        throw new SerializationException("Cannot deserialize game object.");
        //    string name = dataElement.GetElement("Name").Data;

        //    if (!dataElement.HasElement("IsAlive"))
        //        throw new SerializationException("Cannot deserialize game object.");

        //    bool isAlive = true;
        //    if (!dataElement.GetElement("IsAlive").HasData || !bool.TryParse(dataElement.GetElement("IsAlive").Data, out isAlive))
        //        throw new SerializationException("Cannot deserialize game object.");

        //    if (!dataElement.GetElement("IsEnabled").HasData || !bool.TryParse(dataElement.GetElement("IsEnabled").Data, out var isEnabled))
        //        throw new SerializationException("Cannot deserialize game object.");

        //    GameObject gO = new GameObject(name, null, null, 0, null, scene);
        //    gO.isAlive = isAlive;
        //    gO.IsEnabled = isEnabled;

        //    if (!dataElement.HasElement("Transform"))
        //        throw new SerializationException("Cannot deserialize game object.");
        //    gO.Transform.Deserialize(dataElement.GetElement("Transform"));

        //    if (!dataElement.HasElement("Components") || dataElement.GetElement("Components").HasData)
        //        throw new SerializationException("Cannot deserialize game object.");

        //    foreach (XMLElement e in dataElement.GetElement("Components").NestedElements) {
        //        GameObjectComponent.Deserialize(e, gO);
        //    }

        //    if (!dataElement.HasElement("Children") || dataElement.GetElement("Children").HasData)
        //        throw new SerializationException("Cannot deserialize game object.");

        //    foreach (XMLElement e in dataElement.GetElement("Children").NestedElements) {
        //        GameObject child = Deserialize(e, scene);
        //        child.Parent = gO;
        //    }

        //    return gO;
        //}
    }
}
