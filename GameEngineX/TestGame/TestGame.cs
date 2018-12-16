using System;
using System.Drawing;
using System.Linq;
using GameEngineX.Application.Logging;
using GameEngineX.Game;
using GameEngineX.Game.GameObjects;
using GameEngineX.Game.GameObjects.GameObjectComponents;
using GameEngineX.Game.GameObjects.GameObjectComponents.UserInterface;
using GameEngineX.Game.UserInterface;
using GameEngineX.Graphics.Renderables;
using GameEngineX.Utility.Math;
using GameEngineX.Utility.Pathfinding;
using TestGame.CellularAutomatas;
using TestGame.CellularAutomatas.CellularEmpire;
using TestGame.CellularAutomatas.ForestFire;
using TestGame.FunctionPlotting;
using TestGame.Pathfinding;

namespace TestGame {
    public class TestGame : GameBase {

        public override void OnInitialize() {
            CreateScene("MainScene");
            ActivateScene("MainScene");
        }

        protected override void OnSceneActivated(Scene scene) {
            //RunTest_GUI();

            RunTest_CA();

            //RunTest_Menu();
            
            //RunTest_FunctionPlotter();
            //RunTest_DataFunctionPlotter();
            
            //RunTest_Collision();

            //RunTest_Pathfinding();
        }

        protected override void OnSceneDeactivated(Scene scene) {
        }

        private static void RunTest_CA() {
            GameObject cameraGO = new GameObject("Camera", new Vector2(0, 0));
            Viewport viewport = cameraGO.AddComponent<Viewport>();
            viewport.RenderTarget = null;
            viewport.Width = 200;
            viewport.Height = 200;

            GameObject gO = new GameObject("CA", new Vector2(0, 0));
            ConwaysGameOfLife ca = gO.AddComponent<ConwaysGameOfLife>();
        }

        private static void RunTest_Pathfinding() {
            GameObject cameraGO = new GameObject("Camera", new Vector2(0, 0));
            Viewport viewport = cameraGO.AddComponent<Viewport>();
            viewport.RenderTarget = null;
            viewport.Width = 200;
            viewport.Height = 200;

            GameObject guiGO = new GameObject("GUI", cameraGO);
            guiGO.AddComponent<GUIDebugDisplay>().Show = true;

            Path<Node> path = null;

            AStar<Node> aStar = new AStar<Node>(
                (from, goal) => Vector2.Distance(from.Transform.Position, goal.Transform.Position),
                (from, to) => from.Edges.Single(e => e.ConnectsNode(to)).Length, node => node.ConnectedNodes);

            GameObject pathGO = new GameObject("Path");
            Sprite sprite = pathGO.AddComponent<Sprite>();
            sprite.RenderLayer = -10;
            sprite.Renderable = new CustomShape((renderLayer, renderer) => {
                if (path == null)
                    return;

                for (int i = 0; i < path.Length - 1; i++) {
                    Node fNode = path[i];
                    Node tNode = path[i + 1];

                    renderer.DrawLine(renderLayer, Color.Crimson, 0.25f, fNode.Transform.Position, tNode.Transform.Position);
                }
            });

            Node[] nodes = new Node[21];

            GameObject node_main_gO = new GameObject("node_main", new Vector2(0, 0));
            Node node_main = node_main_gO.AddComponent<Node>();
            node_main.Color = Color.Chartreuse;
            nodes[0] = node_main;

            Random random = new Random();
            for (int i = 0; i < 20; i++) {


                //random.NextRandomInCircleUniformly(100, out double x, out double y);
                float x = (25f + 75f * (float)random.NextDouble()) * (float)Math.Cos(i * (MathUtility.TwoPIf / 20));
                float y = (25f + 75f * (float)random.NextDouble()) * (float)Math.Sin(i * (MathUtility.TwoPIf / 20));


                GameObject node_gO = new GameObject("node_" + i, new Vector2((float)x, (float)y));
                Node node = node_gO.AddComponent<Node>();
                node.Collider.OnCollisionEnter += (me, other, data) => {
                    path = aStar.FindPath(node, node_main);

                    if (path == null)
                        return;

                    foreach (Edge edge in node.Edges)
                        edge.Color = Color.DarkCyan;

                    Log.WriteLine(node.GameObject.Name + " " + path.Cost);
                };
                node.Collider.OnCollisionExit += (me, other, data) => {
                    path = null;

                    foreach (Edge edge in node.Edges)
                        edge.Color = Color.Aquamarine;
                };

                nodes[i + 1] = node;

                //GameObject edge_gO = new GameObject("edge_" + i);
                //Edge edge = edge_gO.AddComponent<Edge>(("from", node_main), ("to", node));
            }

            for (int i = 0; i < 40;) {
                Node fromNode = nodes[random.Next(21)];
                Node toNode = nodes[random.Next(21)];

                if (fromNode.IsConnectedTo(toNode))
                    continue;

                GameObject edge_gO = new GameObject("edge_" + i);
                Edge edge = edge_gO.AddComponent<Edge>(("from", fromNode), ("to", toNode));

                i++;
            }

        }

        private static void RunTest_Collision() {
            GameObject cameraGO = new GameObject("Camera", new Vector2(0, 0));
            Viewport viewport = cameraGO.AddComponent<Viewport>();
            viewport.RenderTarget = null;
            viewport.Width = 200;
            viewport.Height = 200;

            GameObject guiGO = new GameObject("GUI", cameraGO);
            guiGO.AddComponent<GUIDebugDisplay>();

            GameObject gO_collider = new GameObject("Collider_0");
            gO_collider.Transform.Scale.X = 0.5f;
            gO_collider.Transform.Scale.Y = 0.5f;
            Collider collider = gO_collider.AddComponent<Collider>();
            Sprite sprite = gO_collider.AddComponent<Sprite>();
            sprite.Renderable = new CircleShape(Color.Red, 10);

            collider.OnCollisionEnter += (me, other, data) => Log.WriteLine("Enter");
            collider.OnCollisionExit += (me, other, data) => Log.WriteLine("Exit");

            GameObject gO_collider2 = new GameObject("Collider_0");
            //Sprite sprite2 = gO_collider2.AddComponent<Sprite>();
            //sprite2.Renderable = new CircleShape(Color.GreenYellow, 10);
            //sprite2.RenderLayer = -1;

        }

        private static void RunTest_FunctionPlotter() {
            GameObject cameraGO = new GameObject("Camera", new Vector2(0, 0));
            Viewport viewport = cameraGO.AddComponent<Viewport>();
            viewport.RenderTarget = null;
            viewport.Width = 200;
            viewport.Height = 200;
            cameraGO.AddComponent<GUIDebugDisplay>();

            //cameraGO.AddComponent<ForestFire>();
            //cameraGO.AddComponent<CellularEmpire>();
            //cameraGO.AddComponent<Wireworld>();
            FunctionPlotter functionPlotter = cameraGO.AddComponent<FunctionPlotter>();
            functionPlotter.SetBounds(-10.5f, -10.5f, 10.5f, 10.5f);
            functionPlotter.StepSize = 0.01f;
            //functionPlotter.AddFunction("F1",
            //    x => (float)(-2.0 / (1.0 + Math.Exp(-5.5 * x * x)) + 2), Color.Crimson);
            //functionPlotter.AddFunction("F2",
            //    x => (float)(1 / (1.0 + Math.Exp(-2.0 * Math.Log(999) * Math.Pow(x, 1) + Math.Log(999)))), Color.LawnGreen);
            //functionPlotter.AddFunction("F3",
            //    x => (float)(0.5 * Math.Tanh(6 * x - 3) + 0.5), Color.DodgerBlue);

            double I = 300;
            double dm = 1;
            double g = 9.81;
            double m0 = 220;

            double Mass(float t) => m0 - t * dm;

            functionPlotter.AddFunction("F4",
                x => (float)(I * dm * g / Mass(x) - g), Color.Gold);

            functionPlotter.AddFunction("F5",
                x => (float)(g * I * dm / (Mass(x) * g)), Color.Violet);
        }

        private static void RunTest_DataFunctionPlotter() {
            GameObject cameraGO = new GameObject("Camera", new Vector2(0, 0));
            Viewport viewport = cameraGO.AddComponent<Viewport>();
            viewport.RenderTarget = null;
            viewport.Width = 200;
            viewport.Height = 200;
            cameraGO.AddComponent<GUIDebugDisplay>();

            string filePath = "E:\\Coding\\C#\\PhysicsCalculator\\PhysicsCalculator\\bin\\Debug\\summary.csv";
            FunctionPlotter functionPlotter = FunctionPlotBuilder.BuildFromData(filePath, cameraGO);
        }

        private static void RunTest_Menu() {
            GameObject cameraGO = new GameObject("Camera", new Vector2(0, 0));
            Viewport viewport = cameraGO.AddComponent<Viewport>();
            viewport.RenderTarget = null;
            viewport.Width = 200;
            viewport.Height = 200;

            GameObject guiGO = new GameObject("GUI", cameraGO);
            guiGO.AddComponent<GUIDebugDisplay>();

            GameObject titleGO = new GameObject("Title", guiGO);
            GUILabel titleLabel = titleGO.AddComponent<GUILabel>();
            titleLabel.Width = 0.5f;
            titleLabel.Height = 0.15f;
            titleLabel.Y = 0.475f;
            titleLabel.FontName = "Old English Text MT";
            titleLabel.TextColor = Color.FromArgb(204, 0, 0);
            titleLabel.Text = "Node Builder X";
            titleLabel.Dock = GUIDock.TopCenter;

            GameObject btn_new_GO = new GameObject("New Game Button", guiGO);
            GUIButton btn_newGame = btn_new_GO.AddComponent<GUIButton>();
            btn_newGame.OnMouseEntered += Entered;
            btn_newGame.OnMouseEntered += Exited;
            btn_newGame.Width = 0.25f;
            btn_newGame.Height = 0.1f;
            btn_newGame.Y = titleLabel.Y - 0.1f - titleLabel.Height;
            btn_newGame.FontName = "Old English Text MT";
            btn_newGame.TextColor = Color.FromArgb(204, 0, 0);
            btn_newGame.Text = "New Game";
            btn_newGame.Dock = GUIDock.TopCenter;
            btn_newGame.InteractionGraphics = new GUIInteractionColors(Color.Black, Color.SlateGray, Color.Black);

            GameObject btn_load_GO = new GameObject("Load Game Button", guiGO);
            GUIButton btn_loadGame = btn_load_GO.AddComponent<GUIButton>();
            btn_loadGame.OnMouseEntered += Entered;
            btn_loadGame.OnMouseEntered += Exited;
            btn_loadGame.Width = 0.25f;
            btn_loadGame.Height = 0.1f;
            btn_loadGame.Y = btn_newGame.Y - 0.05f - btn_newGame.Height;
            btn_loadGame.FontName = "Old English Text MT";
            btn_loadGame.TextColor = Color.FromArgb(204, 0, 0);
            btn_loadGame.Text = "Load";
            btn_loadGame.Dock = GUIDock.TopCenter;
            btn_loadGame.InteractionGraphics = new GUIInteractionColors(Color.Black, Color.SlateGray, Color.Black);

            GameObject btn_settings_GO = new GameObject("Settings Game Button", guiGO);
            GUIButton btn_settings = btn_settings_GO.AddComponent<GUIButton>();
            btn_settings.Width = 0.25f;
            btn_settings.Height = 0.1f;
            btn_settings.Y = btn_loadGame.Y - 0.05f - btn_loadGame.Height;
            btn_settings.FontName = "Old English Text MT";
            btn_settings.TextColor = Color.FromArgb(204, 0, 0);
            btn_settings.Text = "Settings";
            btn_settings.Dock = GUIDock.TopCenter;
            btn_settings.InteractionGraphics = new GUIInteractionColors(Color.Black, Color.SlateGray, Color.Black);
            btn_settings.OnMouseEntered += Entered;
            btn_settings.OnMouseEntered += Exited;

            GameObject btn_exit_GO = new GameObject("Exit Game Button", guiGO);
            GUIButton btn_exit = btn_exit_GO.AddComponent<GUIButton>();
            btn_exit.OnMouseEntered += Entered;
            btn_exit.OnMouseEntered += Exited;
            btn_exit.Width = 0.25f;
            btn_exit.Height = 0.1f;
            btn_exit.Y = btn_settings.Y - 0.1f - btn_settings.Height;
            btn_exit.FontName = "Old English Text MT";
            btn_exit.TextColor = Color.FromArgb(204, 0, 0);
            btn_exit.Text = "Exit";
            btn_exit.Dock = GUIDock.TopCenter;
            btn_exit.InteractionGraphics = new GUIInteractionColors(Color.Black, Color.SlateGray, Color.Black);
            btn_exit.OnButtonClick += button => TestApplication.Instance.Exit();
        }

        private static void RunTest_GUI() {
            GameObject cameraGO = new GameObject("Camera", new Vector2(0, 0));
            Viewport viewport = cameraGO.AddComponent<Viewport>();
            viewport.RenderTarget = null;
            viewport.Width = 200;
            viewport.Height = 200;
            //Sprite cameraGO_c1_sprite = cameraGO.AddComponent<Sprite>();
            //cameraGO_c1_sprite.Renderable = new CircleShape(Color.Orange, 15f);
            //Sprite cameraGO_c2_sprite = cameraGO.AddComponent<Sprite>();
            //cameraGO_c2_sprite.Renderable = new CircleShape(Color.DimGray, 0.1f, 50f);

            GameObject guiGO = new GameObject("GUI", cameraGO);
            guiGO.AddComponent<GUIDebugDisplay>();

            //GameObject pnlGO = new GameObject("PanelGO");
            //GUILabel pnl = pnlGO.AddComponent<GUILabel>();
            //pnl.Width = 0.25f;
            //pnl.Height = 0.25f;
            //pnl.X = 0;
            //pnl.Y = 0;
            //pnl.Dock = GUIDock.BottomCenter;

            GameObject button0GO = new GameObject("Button 0", guiGO);
            GUIButton button0GO_button = button0GO.AddComponent<GUIButton>();
            button0GO_button.X = -0.3f;
            button0GO_button.Y = 0.5f - 0.08f;
            button0GO_button.Width = 0.2f;
            button0GO_button.Height = 0.08f;
            button0GO_button.OnMouseEntered += (c, x, y) => Log.WriteLine($"Entered {c.GameObject.Name} {x}, {y} - {c.Bounds}");
            button0GO_button.OnMouseExited += (c, x, y) => Log.WriteLine($"Exited {c.GameObject.Name} {x}, {y} - {c.Bounds}");
            button0GO_button.OnMouseClicked += (c, x, y) => Log.WriteLine($"Clicked {c.GameObject.Name} {x}, {y} - {c.Bounds}");
            button0GO_button.OnMouseReleased += (c, x, y) => Log.WriteLine($"Released {c.GameObject.Name} {x}, {y} - {c.Bounds}");
            button0GO_button.OnButtonClick += button => Log.WriteLine($"Button [{button.GameObject.Name}] clicked.");
            button0GO_button.InteractionGraphics = new GUIInteractionColors(Color.DodgerBlue, Color.CornflowerBlue, Color.DeepSkyBlue);
            button0GO_button.Text = "Button";
            button0GO_button.FontName = "Calibri";
            button0GO_button.TextColor = Color.DarkBlue;
            button0GO_button.Dock = GUIDock.BottomCenter;

            GameObject titleGO = new GameObject("Title");
            GUILabel titleLabel = titleGO.AddComponent<GUILabel>();
            titleLabel.Width = 0.5f;
            titleLabel.Height = 0.15f;
            //titleLabel.Y = 0.45f;
            titleLabel.FontName = "Old English Text MT";
            titleLabel.Text = "Node Builder X";
            titleLabel.Dock = GUIDock.TopCenter;

            //GameObject progressbar0GO = new GameObject("Progress Bar 0", guiGO);
            //GUIProgressbar progressbar0GO_progressbar = progressbar0GO.AddComponent<GUIProgressbar>();
            //progressbar0GO_progressbar.X = -0.3f;
            //progressbar0GO_progressbar.Y = 0.5f - 0.06f;
            //progressbar0GO_progressbar.Width = 0.2f;
            //progressbar0GO_progressbar.Height = 0.04f;

            //GameObject slider0GO = new GameObject("Slider 0", guiGO);
            //GUISlider slider0GO_slider = slider0GO.AddComponent<GUISlider>();
            //slider0GO_slider.X = -0.3f;
            //slider0GO_slider.Y = 0.4f - 0.06f;
            //slider0GO_slider.Width = 0.2f;
            //slider0GO_slider.Height = 0.04f;
            //slider0GO_slider.Minimum = 0;
            //slider0GO_slider.Maximum = 10;
            //slider0GO_slider.StepSize = 1;
            //slider0GO_slider.StateValue = 5;

            //GameObject sliderLabelGO = new GameObject("Slider Label 0", guiGO);
            //GUILabel sliderLabel0GO_label = sliderLabelGO.AddComponent<GUILabel>();
            //sliderLabel0GO_label.X = slider0GO_slider.X;
            //sliderLabel0GO_label.Y = slider0GO_slider.Y + slider0GO_slider.Height;
            //sliderLabel0GO_label.Width = slider0GO_slider.Width;
            //sliderLabel0GO_label.Height = slider0GO_slider.Height;
            //sliderLabel0GO_label.FontName = "Consolas";
            //sliderLabel0GO_label.TextColor = Color.WhiteSmoke;
            //slider0GO_slider.OnValueChanged += slider => sliderLabel0GO_label.Text = slider0GO_slider.StateValue.ToString();

            //GameObject textbox0GO = new GameObject("Textbox 0", guiGO);
            //GUITextbox textbox0GO_textbox = textbox0GO.AddComponent<GUITextbox>();
            //textbox0GO_textbox.X = 0.1f;
            //textbox0GO_textbox.Y = slider0GO_slider.Y + 0.04f;
            //textbox0GO_textbox.Width = 0.2f;
            //textbox0GO_textbox.Height = 0.08f;
            //textbox0GO_textbox.FontName = "Consolas";


            //GameObject spriteGO = new GameObject("SpriteGO", new Vector2(), MathUtility.QuarterPIf);
            //Sprite sprite = spriteGO.AddComponent<Sprite>();
            //sprite.RenderLayer = 0;
            //sprite.Renderable = new RectangleShape(Color.Red, 10, 10);

            //GameObject gO1 = new GameObject("gO1", new Vector2(0f, 0f));
            //Sprite gO1_c1_sprite = gO1.AddComponent<Sprite>();
            //gO1_c1_sprite.Renderable = new RectangleShape(Color.GreenYellow, 20f, 20f);
            //Sprite gO1_c2_sprite = gO1.AddComponent<Sprite>();
            //gO1_c2_sprite.Renderable = new CircleShape(Color.DimGray, 0.1f, 25f);
            //Rotation gO1_c3_rotation = gO1.AddComponent<Rotation>();
            //gO1_c3_rotation.Radius = 50;

            //GameObject gO2 = new GameObject("gO1", new Vector2(0f, 0f));
            //gO2.Transform.LocalPosition.Set(0, 0);
            //gO2.Parent = gO1;
            //Sprite gO2_c1_sprite = gO2.AddComponent<Sprite>();
            //gO2_c1_sprite.Renderable = new RectangleShape(Color.CadetBlue, 10f, 10f);
            //Rotation gO2_c2_rotation = gO2.AddComponent<Rotation>();
            //gO2_c2_rotation.Radius = 25;
            //gO2_c2_rotation.AngularVelocity *= 5;

        }

        private static void Entered(GUIComponent c, float x, float y) {
            Log.WriteLine($"Entered {c.GameObject.Name} {c.Bounds}");
        }

        private static void Exited(GUIComponent c, float x, float y) {
            Log.WriteLine($"Exited {c.GameObject.Name} {c.Bounds}");
        }
    }
}
