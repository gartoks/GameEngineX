using System;
using System.Collections.Generic;
using System.Linq;
using GameEngineX.Game.GameObjects;
using GameEngineX.Game.GameObjects.GameObjectComponents;

namespace GameEngineX.Game {
    [Serializable]
    public class Scene {
        public readonly string Name;

        private readonly List<GameObject> gameObjects;

        private readonly HashSet<Viewport> viewports;

        //internal XMLElement Serialize() {
        //    XMLElement root = new XMLElement("Scene");

        //    root.AddDataElement("Name", Name);

        //    XMLElement gOElement = new XMLElement("GameObjects");
        //    root.AddElement(gOElement);

        //    foreach (GameObject gameObject in this.gameObjects.Except(this.toBeRemovedGameObjects).Union(this.toBeAddedGameObjects).Where(gO => gO.Parent == null)) {
        //        gOElement.AddElement(gameObject.Serialize());
        //    }

        //    return root;
        //}

        //internal static Scene Deserialize(XMLElement dataElement) {
        //    if (!dataElement.HasNestedElements)
        //        throw new SerializationException("Cannot deserialize scene.");

        //    if (!dataElement.HasElement("Name") || !dataElement.GetElement("Name").HasData)
        //        throw new SerializationException("Cannot deserialize scene.");
        //    string name = dataElement.GetElement("Name").Data;

        //    GameBase.Instance.CreateScene(name);
        //    Scene scene = GameBase.Instance.GetScene(name);

        //    if (!dataElement.HasElement("GameObjects") || dataElement.GetElement("GameObjects").HasData)
        //        throw new SerializationException("Cannot deserialize scene.");

        //    XMLElement gameObjectsElement = dataElement.GetElement("GameObjects");
        //    foreach (XMLElement gOElement in gameObjectsElement.NestedElements) {
        //        GameObject.Deserialize(gOElement, scene);
        //    }

        //    return scene;
        //}

        internal Scene(string name) {
            Name = name;

            this.gameObjects = new List<GameObject>();
            this.viewports = new HashSet<Viewport>();
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
                foreach (Viewport viewport in this.viewports) {
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
            }
        }

        internal void RemoveViewport(Viewport viewport) {
            lock (this.viewports) {
                this.viewports.Remove(viewport);
            }
        }
    }
}
