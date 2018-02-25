using System.Collections.Generic;
using GameEngineX.Resources;

namespace GameEngineX.Game {
    public abstract class GameBase {
        private static GameBase instance;
        public static GameBase Instance => instance;

        private Scene activeScene;
        private Dictionary<string, Scene> scenes;

        protected GameBase() {
            GameBase.instance = this;

            this.scenes = new Dictionary<string, Scene>();
        }

        internal void Initialize() {
            OnInitialize();
        }

        public abstract void OnInitialize();

        internal void Update(float deltaTime) {
            this.activeScene?.Update(deltaTime);
        }

        internal void Render() {
            this.activeScene?.Render();
        }
        
        public void CreateScene(string sceneName) {
            if (this.scenes.ContainsKey(sceneName))
                return;

            Scene scene = new Scene(sceneName);

            this.scenes[sceneName] = scene;
        }

        //public void LoadScene(string filePath) {
        //    XMLElement sceneElement = XMLReader.Read(filePath);

        //    Scene.Deserialize(sceneElement);
        //} TODO

        public void ActivateScene(string sceneName) {
            if (this.activeScene == null && sceneName == null || this.activeScene != null && this.activeScene.Name.Equals(sceneName))
                return;

            if (this.activeScene != null)
                OnSceneDeactivated(this.activeScene);

            if (sceneName == null)
                this.activeScene = null;
            else if (this.scenes.TryGetValue(sceneName, out Scene scene))
                this.activeScene = scene;

            ResourceManager.ClearSceneResources();

            OnSceneActivated(this.activeScene);
        }

        //public void SaveScene(string filePath, string sceneName = null) {
        //    if (sceneName == null)
        //        sceneName = ActiveScene;

        //    if (!this.scenes.ContainsKey(sceneName))
        //        return;

        //    Scene scene = this.scenes[sceneName];
        //    XMLElement sceneElement = scene.Serialize();

        //    XMLWriter.Write(sceneElement, filePath);
        //}     TODO

        internal Scene GetScene(string sceneName) {
            return this.scenes[sceneName];
        }

        protected abstract void OnSceneActivated(Scene scene);

        protected abstract void OnSceneDeactivated(Scene scene);

        internal Scene ActiveScene => activeScene;

        //public bool IsOnScreen(GameObject gameObject) {
        //    IEnumerable<Collider> colliders = gameObject.GetComponents<Collider>();
        //    if (!colliders.Any()) {
        //        return Viewport.IsVisible(gameObject.Transform.Position.X, gameObject.Transform.Position.Y);
        //    } else {
        //        foreach (Collider collider in colliders) {
        //            Rect boundingRect = collider.Polygon.BoundingRect;

        //            if (!Viewport.IsVisible((float)boundingRect.BottomLeft.X, (float)boundingRect.BottomLeft.Y) ||
        //                !Viewport.IsVisible((float)boundingRect.BottomRight.X, (float)boundingRect.BottomRight.Y) ||
        //                !Viewport.IsVisible((float)boundingRect.TopLeft.X, (float)boundingRect.TopLeft.Y) ||
        //                !Viewport.IsVisible((float)boundingRect.TopRight.X, (float)boundingRect.TopRight.Y))
        //                return false;

        //            foreach (Vector2 point in collider.Polygon.Points) {
        //                if (!Viewport.IsVisible(gameObject.Transform.Position.X, gameObject.Transform.Position.Y))
        //                    return false;
        //            }
        //        }

        //        return true;
        //    }
        //}

        //private bool IsInRenderArea(GameObject gameObject) {
        //    (float x, float y, float width, float height) bounds = Viewport.WorldBounds;
        //    float ox = bounds.width / 4f;
        //    float oy = bounds.height / 4f;
        //    bounds = bounds.Expand(ox, ox, oy, oy);
        //    //Debug.WriteLine(bounds);

        //    IEnumerable<Collider> colliders = gameObject.GetComponents<Collider>();
        //    if (!colliders.Any()) {
        //        return bounds.Intersects(gameObject.Transform.Position.X, gameObject.Transform.Position.Y, 0);
        //    } else {
        //        foreach (Collider collider in colliders) {
        //            Rect boundingRect = collider.Polygon.BoundingRect;

        //            if (!bounds.Intersects((float)boundingRect.BottomLeft.X, (float)boundingRect.BottomLeft.Y, 0) ||
        //                !bounds.Intersects((float)boundingRect.BottomRight.X, (float)boundingRect.BottomRight.Y, 0) ||
        //                !bounds.Intersects((float)boundingRect.TopLeft.X, (float)boundingRect.TopLeft.Y, 0) ||
        //                !bounds.Intersects((float)boundingRect.TopRight.X, (float)boundingRect.TopRight.Y, 0))
        //                return false;

        //            foreach (Vector2 point in collider.Polygon.Points) {
        //                if (!Viewport.IsVisible(gameObject.Transform.Position.X, gameObject.Transform.Position.Y))
        //                    return false;
        //            }
        //        }

        //        return true;
        //    }
        //}

        //public bool IsInsidePlayArea(Vector2 p) {
        //    return IsInsidePlayArea(p.X, p.Y);
        //}

        //public bool IsInsidePlayArea(float x, float y) {
        //    return Viewport.IsVisible(x, y);
        //}

        //public XMLElement ToXML() {
        //    XMLElement e = XMLElement.NewEmptyXMLElement(Data.SAVE_XML_ROOT);
        //    e.AddElement(this.position.ToXML("position"));
        //    e.AddElement(this.gameObjects.ToXML("gameObjects", gO => gO.ToXML("GameObject")));

        //}

        //public static GameBase CreateFromXML(XMLElement xmlElement) {

        //}

    }
}
