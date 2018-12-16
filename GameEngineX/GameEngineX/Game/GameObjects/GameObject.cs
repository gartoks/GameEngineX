using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Game.GameObjects.Utility;
using GameEngineX.Game.UserInterface;
using GameEngineX.Graphics;
using GameEngineX.Utility.Exceptions;
using GameEngineX.Utility.Math;

namespace GameEngineX.Game.GameObjects {
    public sealed class GameObject : ISerializable {
        public delegate void GameObjectComponentModificationEventHandler(GameObject gameObject, GameObjectComponent component);

        private readonly Guid ID;

        private bool isAlive;

        private string name;

        public bool IsEnabled;

        private readonly List<GameObjectComponent> components;
        private readonly List<IRendering> renderables;

        private GameObject parent;
        private readonly HashSet<GameObject> children;

        private readonly Transform transform;

        public event GameObjectComponentModificationEventHandler OnComponentAdd;
        public event GameObjectComponentModificationEventHandler OnComponentRemove;

        public GameObject(string name)
            : this(name, null, 0, null) { }

        public GameObject(string name, Vector2 position = null, float rotation = 0, Vector2 scale = null)
            : this(name, null, position, rotation, scale) { }

        public GameObject(string name, GameObject parent = null, Vector2 position = null, float rotation = 0, Vector2 scale = null)
            : this(name, parent, position, rotation, scale, GameBase.Instance.ActiveScene) {
        }

        internal GameObject(string name, GameObject parent = null, Vector2 position = null, float rotation = 0, Vector2 scale = null, Scene scene = null) {
            if (scene == null)
                scene = GameBase.Instance.ActiveScene;

            if (position == null)
                position = new Vector2();

            if (scale == null)
                scale = new Vector2(1, 1);

            ID = Guid.NewGuid();

            Name = name;
            IsEnabled = true;

            this.components = new List<GameObjectComponent>();
            this.renderables = new List<IRendering>();

            this.transform = new Transform(this, position, rotation, scale);

            Parent = parent;
            this.children = new HashSet<GameObject>();

            if (GameBase.Instance.ActiveScene == null)
                throw new GameStateException("Cannote create GameObject when no scene is active.");

            scene.AddGameObject(this);

            this.isAlive = true;
        }

        public GameObject(SerializationInfo info, StreamingContext ctxt) {
            ID = Guid.Parse(info.GetString(nameof(ID)));
            isAlive = info.GetBoolean(nameof(isAlive));
            name = info.GetString(nameof(name));
            IsEnabled = info.GetBoolean(nameof(IsEnabled));
            components = (List<GameObjectComponent>)info.GetValue(nameof(components), typeof(List<GameObjectComponent>));
            parent = (GameObject)info.GetValue(nameof(parent), typeof(GameObject));
            children = (HashSet<GameObject>)info.GetValue(nameof(children), typeof(HashSet<GameObject>));
            transform = (Transform)info.GetValue(nameof(transform), typeof(Transform));
            renderingChildren = (List<GameObject>)info.GetValue(nameof(renderingChildren), typeof(List<GameObjectComponent>));
            updatingChildren = (List<GameObject>)info.GetValue(nameof(updatingChildren), typeof(List<GameObjectComponent>));
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

            for (int i = this.components.Count - 1; i >= 0; i--) {
                GameObjectComponent component = this.components[i];

                if (!component.IsActive)
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

            lock (this.renderables) {
                foreach (IRendering r in this.renderables) {
                    if (!((GameObjectComponent)r).IsEnabled)
                        continue;

                    r.Renderable.Render(r.RenderLayer, renderer);
                }
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
            if (t.IsAbstract)
                throw new ArgumentException("Cannot add an abstract component.", nameof(t));

            IEnumerable<RequiredComponents> requiredComponentCollection = t.GetCustomAttributes<RequiredComponents>(true);
            foreach (RequiredComponents requiredComponents in requiredComponentCollection) {
                GameObjectComponentSearchMode searchMode = requiredComponents.InHierarchy ? GameObjectComponentSearchMode.ParentalHierarchy : GameObjectComponentSearchMode.This;
                foreach (Type requiredComponent in requiredComponents.Required) {
                    if (!GetComponents(requiredComponent, searchMode, true).Any())
                        AddComponent(requiredComponent, true, new(string fieldName, object fielValue)[0]);
                }
            }

            ConstructorInfo ctor = t.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            GameObjectComponent component = ctor.Invoke(new object[0]) as GameObjectComponent;
            component.GameObject = this;

            this.components.Add(component);

            component.IsEnabled = isEnabled;

            if (component is IRendering r) {
                lock (this.renderables) {
                    this.renderables.Add(r);
                    this.renderables.Sort((x, y) => y.RenderLayer.CompareTo(x.RenderLayer));
                }
            }

            if (fields != null && fields.Any()) {
                foreach ((string fieldName, object fielValue) fieldData in fields) {
                    Type tmpT = t;
                    FieldInfo fI = null;
                    while (tmpT != null) {
                        fI = tmpT.GetField(fieldData.fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        
                        if (fI != null)
                            break;

                        tmpT = tmpT.BaseType;
                    }


                    if (fI == null)
                        throw new SerializationException("Cannot initialize component field.");

                    fI.SetValue(component, fieldData.fielValue);
                }
            }

            component.Initialize();

            OnComponentAdd?.Invoke(this, component);

            return component;
        }

        internal void RemoveComponent(GameObjectComponent component) {
            if (!this.components.Contains(component))
                return;


            component.Death();

            OnComponentRemove?.Invoke(this, component);

            this.components.Remove(component);
            if (!(component is IRendering r))
                return;

            lock (this.renderables) {
                this.renderables.Remove(r);
                this.renderables.Sort((x, y) => y.RenderLayer.CompareTo(x.RenderLayer));
            }
        }

        public T GetComponent<T>(GameObjectComponentSearchMode searchMode = GameObjectComponentSearchMode.This, bool includeDerivations = true) where T : GameObjectComponent {
            return GetComponents<T>(searchMode, includeDerivations).FirstOrDefault();
        }

        public IEnumerable<T> GetComponents<T>(GameObjectComponentSearchMode searchMode = GameObjectComponentSearchMode.This, bool includeDerivations = true) where T : GameObjectComponent {
            return GetComponents(typeof(T), searchMode, includeDerivations).Cast<T>();
        }

        private IEnumerable<GameObjectComponent> GetComponents(Type t, GameObjectComponentSearchMode searchMode, bool includeDerivations) {
            if (!t.IsSubclassOf(typeof(GameObjectComponent)))
                throw new ArgumentException(nameof(t));

            IEnumerable<GameObjectComponent> comps;
            if (includeDerivations)
                comps = FindComponents(c => c.GetType() == t || c.GetType().IsSubclassOf(t));
            else
                comps = FindComponents(c => c.GetType() == t);

            if (searchMode == GameObjectComponentSearchMode.This)
                return comps;

            if ((searchMode & GameObjectComponentSearchMode.ChildHierarchy) > 0) {
                foreach (GameObject child in this.children) {
                    comps = comps.Concat(child.GetComponents(t, GameObjectComponentSearchMode.ChildHierarchy, includeDerivations));
                }
            }

            if ((searchMode & GameObjectComponentSearchMode.ParentalHierarchy) > 0 && Parent != null) {
                comps = comps.Concat(Parent.GetComponents(t, GameObjectComponentSearchMode.ParentalHierarchy, includeDerivations));
            }

            return comps;
        }

        public IEnumerable<GameObjectComponent> FindComponents(Func<GameObjectComponent, bool> selector, GameObjectComponentSearchMode searchMode = GameObjectComponentSearchMode.This) {
            IEnumerable<GameObjectComponent> comps = this.components.Where(selector);

            if (searchMode == GameObjectComponentSearchMode.This)
                return comps;

            if ((searchMode & GameObjectComponentSearchMode.ChildHierarchy) > 0) {
                foreach (GameObject child in this.children) {
                    comps = comps.Concat(child.FindComponents(selector, GameObjectComponentSearchMode.ChildHierarchy));
                }
            }

            if ((searchMode & GameObjectComponentSearchMode.ParentalHierarchy) > 0 && Parent != null) {
                comps = comps.Concat(Parent.FindComponents(selector, GameObjectComponentSearchMode.ParentalHierarchy));
            }

            return comps;
        }

        public IEnumerable<GameObject> GetParentalHierarchy(bool includeCurrent = true) {
            GameObject gO = includeCurrent ? this : this.parent;
            while (gO != null) {
                yield return gO;
                gO = gO.Parent;
            }
        }

        public GameObject GetRootGameObject() {
            return Parent == null ? this : Parent.GetRootGameObject();
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

            for (int i = this.components.Count - 1; i >= 0; i--) {
                this.components[i].Destroy();
            }
        }

        public Transform Transform => this.transform;

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

        public override string ToString() {
            return $"GO [{Name}]";
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(ID), ID.ToString());
            info.AddValue(nameof(isAlive), isAlive);
            info.AddValue(nameof(name), name);
            info.AddValue(nameof(IsEnabled), IsEnabled);
            info.AddValue(nameof(components), components);
            info.AddValue(nameof(parent), parent);
            info.AddValue(nameof(children), children);
            info.AddValue(nameof(transform), transform);
            info.AddValue(nameof(renderingChildren), renderingChildren);
            info.AddValue(nameof(updatingChildren), updatingChildren);
        }
    }
}