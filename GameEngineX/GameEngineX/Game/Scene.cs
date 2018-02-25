using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GameEngineX.Game.GameObjects;
using GameEngineX.Game.GameObjects.GameObjectComponents;

namespace GameEngineX.Game {
    public class Scene : ISerializable {
        public static Scene Active => GameBase.Instance.ActiveScene;

        public readonly string Name;

        private readonly List<GameObject> gameObjects;

        public Viewport MainViewport { get; private set; } 
        private readonly HashSet<Viewport> viewports;

        internal Scene(string name) {
            Name = name;

            this.gameObjects = new List<GameObject>();
            this.viewports = new HashSet<Viewport>();
        }

        internal Scene(SerializationInfo info, StreamingContext ctxt) {
            Name = info.GetString(nameof(Name));

            this.gameObjects = (List<GameObject>)info.GetValue(nameof(gameObjects), typeof(List<GameObject>));
            this.viewports = (HashSet<Viewport>)info.GetValue(nameof(viewports), typeof(HashSet<Viewport>));
            this.updatingGameObjects = (List<GameObject>)info.GetValue(nameof(updatingGameObjects), typeof(List<GameObject>));
            this.toBeAddedGameObjects = (List<GameObject>)info.GetValue(nameof(toBeAddedGameObjects), typeof(List<GameObject>));
            this.toBeRemovedGameObjects = (List<GameObject>)info.GetValue(nameof(toBeRemovedGameObjects), typeof(List<GameObject>));
        }

        private readonly List<GameObject> updatingGameObjects = new List<GameObject>();
        private readonly List<GameObject> toBeRemovedGameObjects = new List<GameObject>();
        private readonly List<GameObject> toBeAddedGameObjects = new List<GameObject>();
        internal void Update(float deltaTime) {
            this.updatingGameObjects.Clear();
            lock (this.gameObjects) {
                this.updatingGameObjects.AddRange(this.gameObjects.Where(gO => gO.Parent == null));
            }

            foreach (GameObject gO in this.updatingGameObjects) {
                if (gO.IsAlive) {
                    gO.Update(deltaTime);
                } else
                    this.toBeRemovedGameObjects.Add(gO);
            }

            lock (this.gameObjects) {
                foreach (GameObject gO in this.toBeAddedGameObjects) {
                    if (gO.IsAlive) {
                        this.gameObjects.Add(gO);
                    }
                }
                this.toBeAddedGameObjects.Clear();

                foreach (GameObject gO in this.toBeRemovedGameObjects) {
                    this.gameObjects.Remove(gO);
                }
                this.toBeRemovedGameObjects.Clear();
            }
        }

        internal void Render() {
            lock (this.viewports) {
                if (MainViewport != null && (!MainViewport.IsActive || MainViewport.RenderTarget != null))
                    FindMainViewport();

                foreach (Viewport viewport in this.viewports) {
                    if (viewport.IsActive && viewport.RenderTarget == null && !viewport.Equals(MainViewport))
                        continue;

                    viewport.Render();
                }
            }
        }

        public IEnumerable<GameObject> GameObjects => this.gameObjects;

        internal void AddGameObject(GameObject gO) {
            this.toBeAddedGameObjects.Add(gO);
        }

        internal void AddViewport(Viewport viewport) {
            lock (this.viewports) {
                this.viewports.Add(viewport);

                if (viewport.IsActive && viewport.RenderTarget == null)
                    FindMainViewport();
            }
        }

        internal void RemoveViewport(Viewport viewport) {
            lock (this.viewports) {
                this.viewports.Remove(viewport);
                
                if (viewport.Equals(MainViewport))
                    FindMainViewport();
            }
        }

        private void FindMainViewport() {
            foreach (Viewport viewport in this.viewports) {
                if (!viewport.IsActive || viewport.RenderTarget != null)
                    continue;

                MainViewport = viewport;
                return;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(gameObjects), gameObjects);
            lock (this.viewports) {
                info.AddValue(nameof(viewports), viewports);
            }
            info.AddValue(nameof(updatingGameObjects), updatingGameObjects);
            info.AddValue(nameof(toBeAddedGameObjects), toBeAddedGameObjects);
            info.AddValue(nameof(toBeRemovedGameObjects), toBeRemovedGameObjects);
        }
    }
}
