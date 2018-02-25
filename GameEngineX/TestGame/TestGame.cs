using System.Drawing;
using GameEngineX.Application.Logging;
using GameEngineX.Game;
using GameEngineX.Game.GameObjects;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface;
using GameEngineX.Game.UserInterface;
using GameEngineX.Graphics.Renderables;
using GameEngineX.Utility.Math;

namespace TestGame {
    public class TestGame : GameBase {

        public override void OnInitialize() {
            CreateScene("MainScene");
            ActivateScene("MainScene");
        }

        protected override void OnSceneActivated(Scene scene) {
            GameObject cameraGO = new GameObject("Camera", new Vector2(0, 0));
            Viewport viewport = cameraGO.AddComponent<Viewport>();
            viewport.RenderTarget = null;
            viewport.Width = 200;
            viewport.Height = 200;
            cameraGO.AddComponent<GUIDebugDisplay>();

            //GameObject forestFireGO = new GameObject("ForestFire");
            //ForestFire forestFire = forestFireGO.AddComponent<ForestFire>();

            GameObject cGoLGO = new GameObject("Conway's Game of Life");
            ConwaysGameOfLife cGoL = cGoLGO.AddComponent<ConwaysGameOfLife>();
        }

        protected override void OnSceneDeactivated(Scene scene) {
        }

        private static void RunTest() {
            GameObject cameraGO = new GameObject("Camera", new Vector2(0, 0));
            Viewport viewport = cameraGO.AddComponent<Viewport>();
            viewport.RenderTarget = null;
            viewport.Width = 200;
            viewport.Height = 200;
            Sprite cameraGO_c1_sprite = cameraGO.AddComponent<Sprite>();
            cameraGO_c1_sprite.Renderable = new CircleShape(Color.Orange, 15f);
            Sprite cameraGO_c2_sprite = cameraGO.AddComponent<Sprite>();
            cameraGO_c2_sprite.Renderable = new CircleShape(Color.DimGray, 0.1f, 50f);

            GameObject guiGO = new GameObject("GUI", cameraGO);
            guiGO.AddComponent<GUIDebugDisplay>();

            GameObject button0GO = new GameObject("Button 0", guiGO);
            GUIButton button0GO_button = button0GO.AddComponent<GUIButton>();
            button0GO_button.X = -0.5f;
            button0GO_button.Y = 0.5f - 0.08f;
            button0GO_button.Width = 0.2f;
            button0GO_button.Height = 0.08f;
            button0GO_button.OnMouseEntered += (x, y) => Log.LogLine($"Entered {x}, {y}");
            button0GO_button.OnMouseExited += (x, y) => Log.LogLine($"Exited {x}, {y}");
            button0GO_button.OnMouseClicked += (x, y) => Log.LogLine($"Clicked {x}, {y}");
            button0GO_button.OnMouseReleased += (x, y) => Log.LogLine($"Released {x}, {y}");
            button0GO_button.OnButtonClick += button => Log.LogLine($"Button [{button.GameObject.Name}] clicked.");
            button0GO_button.InteractionGraphics = new GUIInteractionColors(Color.DodgerBlue, Color.CornflowerBlue, Color.DeepSkyBlue);
            button0GO_button.Text = "Button";
            button0GO_button.FontName = "Calibri";
            button0GO_button.TextColor = Color.DarkBlue;

            GameObject progressbar0GO = new GameObject("Progress Bar 0", guiGO);
            GUIProgressbar progressbar0GO_progressbar = progressbar0GO.AddComponent<GUIProgressbar>();
            progressbar0GO_progressbar.X = -0.3f;
            progressbar0GO_progressbar.Y = 0.5f - 0.06f;
            progressbar0GO_progressbar.Width = 0.2f;
            progressbar0GO_progressbar.Height = 0.04f;

            GameObject slider0GO = new GameObject("Slider 0", guiGO);
            GUISlider slider0GO_slider = slider0GO.AddComponent<GUISlider>();
            slider0GO_slider.X = -0.3f;
            slider0GO_slider.Y = 0.4f - 0.06f;
            slider0GO_slider.Width = 0.2f;
            slider0GO_slider.Height = 0.04f;
            slider0GO_slider.Minimum = 0;
            slider0GO_slider.Maximum = 10;
            slider0GO_slider.StepSize = 1;
            slider0GO_slider.Value = 5;

            GameObject sliderLabelGO = new GameObject("Slider Label 0", guiGO);
            GUILabel sliderLabel0GO_label = sliderLabelGO.AddComponent<GUILabel>();
            sliderLabel0GO_label.X = -0.1f;
            sliderLabel0GO_label.Y = slider0GO_slider.Y;
            sliderLabel0GO_label.Width = 0.1f;
            sliderLabel0GO_label.Height = 0.04f;
            sliderLabel0GO_label.FontName = "Consolas";
            sliderLabel0GO_label.TextColor = Color.WhiteSmoke;
            slider0GO_slider.OnValueChanged += slider => sliderLabel0GO_label.Text = slider0GO_slider.Value.ToString();

            GameObject spriteGO = new GameObject("SpriteGO", new Vector2(), MathUtility.QuarterPIf);
            Sprite sprite = spriteGO.AddComponent<Sprite>();
            sprite.RenderLayer = 0;
            sprite.Renderable = new RectangleShape(Color.Red, 10, 10);

            GameObject gO1 = new GameObject("gO1", new Vector2(0f, 0f));
            Sprite gO1_c1_sprite = gO1.AddComponent<Sprite>();
            gO1_c1_sprite.Renderable = new RectangleShape(Color.GreenYellow, 20f, 20f);
            Sprite gO1_c2_sprite = gO1.AddComponent<Sprite>();
            gO1_c2_sprite.Renderable = new CircleShape(Color.DimGray, 0.1f, 25f);
            Rotation gO1_c3_rotation = gO1.AddComponent<Rotation>();
            gO1_c3_rotation.Radius = 50;

            GameObject gO2 = new GameObject("gO1", new Vector2(0f, 0f));
            gO2.Transform.LocalPosition.Set(0, 0);
            gO2.Parent = gO1;
            Sprite gO2_c1_sprite = gO2.AddComponent<Sprite>();
            gO2_c1_sprite.Renderable = new RectangleShape(Color.CadetBlue, 10f, 10f);
            Rotation gO2_c2_rotation = gO2.AddComponent<Rotation>();
            gO2_c2_rotation.Radius = 25;
            gO2_c2_rotation.AngularVelocity *= 5;

            //GameObject forestFireGO = new GameObject("ForestFire");
            //ForestFire forestFire = forestFireGO.AddComponent<ForestFire>();

            //GameObject cGoLGO = new GameObject("Conway's Game of Life");
            //ConwaysGameOfLife cGoL = cGoLGO.AddComponent<ConwaysGameOfLife>();
        }
    }
}
