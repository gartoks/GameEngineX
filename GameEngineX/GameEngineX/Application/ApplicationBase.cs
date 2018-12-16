using GameEngineX.Game;
using GameEngineX.Input;
using GameEngineX.Utility;
using System;
using System.Threading;
using System.Windows.Forms;
using GameEngineX.Application.Logging;
using GameEngineX.Graphics;
using GameEngineX.Resources;

namespace GameEngineX.Application {
    public abstract class ApplicationBase {

        private static ApplicationBase instance;
        public static ApplicationBase Instance => instance;

        public bool IsSimulation { get; private set; }

        private Control gameArea;
        //private int gameAreaWidth;
        //private int gameAreaHeight;

        private Thread resourceThread;
        private Thread renderThread;
        private Thread logicThread;
        private bool running;

        private LogWindow logWindow;

        private GameBase game;

        private TimeTracker upsTimer;
        private TimeTracker fpsTimer;

        protected ApplicationBase() {
            ApplicationBase.instance = this;
        }

        public void Initialize(Control gameArea, GameBase game, ApplicationInitializationParameters parameters) {
            Renderer.DEFAULT_FONT_NAME = parameters.DefaultFont;
            Renderer.BACKGROUND_COLOR = parameters.BackgroundColor;

            Form.FormClosed += (sender, args) => Exit();
            gameArea.Dock = DockStyle.Fill;
            if (parameters.Maximize) {
                Form.FormBorderStyle = FormBorderStyle.None;
                Form.WindowState = FormWindowState.Maximized;
            } else {
                Form.FormBorderStyle = FormBorderStyle.FixedSingle;
                Form.WindowState = FormWindowState.Normal;
            }

            IsSimulation = parameters.IsSimulation;

            this.gameArea = gameArea;
            //this.gameAreaWidth = gameArea.Width;
            //this.gameAreaHeight = gameArea.Height;
            this.gameArea.SizeChanged += GameArea_SizeChanged;

            // initialize logging
            Log.Initialize(parameters.LoggingMessageColor, parameters.LoggingWarningColor, parameters.LoggingErrorColor);

            if (parameters.ShowLogWindow) {
                this.logWindow = new LogWindow();
                Log.OnLog += this.logWindow.AddLine;
                this.logWindow.Show();
            }

            ResourceManager.Initialize();

            InputHandler.Initialize(gameArea);

            this.game = game;

            this.upsTimer = new TimeTracker(parameters.UpdatesPerSecond);
            this.fpsTimer = new TimeTracker(parameters.FramesPerSecond);

            this.resourceThread = new Thread(ResourceLoop);
            this.resourceThread.Name = "ResourceThread";
            this.renderThread = new Thread(RenderLoop);
            this.renderThread.Name = "RenderThread";
            this.logicThread = new Thread(UpdateLoop);
            this.logicThread.Name = "LogicThread";
        }

        public void Start() {
            this.running = true;

            this.resourceThread.Start();

            this.renderThread.Start();

            this.logicThread.Start();

            this.game.Initialize();
        }

        public void Exit() {
            Thread exitThread = new Thread(() => {
                this.running = false;

                try {
                    Form.Invoke(new Action(() => {
                        this.resourceThread.Join();
                        this.renderThread.Join();
                        this.logicThread.Join();

                        System.Windows.Forms.Application.Exit();
                    }));
                } catch (Exception) {}
            });

            exitThread.Start();
        }

        private void UpdateLoop() {
            while (this.running) {
                InputHandler.Update();

                this.upsTimer.FullTick(this.game.Update, !IsSimulation);
                logWindow?.SetUpsFps(UpdatesPerSecond, FramesPerSecond);

                InputHandler.LateUpate();
            }
        }

        private void RenderLoop() {
            while (this.running) {
                if (!GameArea.Visible || !GameArea.Enabled || !IsWindowVisible) {
                    Thread.Sleep(100);
                    continue;
                }

                this.fpsTimer.FullTick(dT => this.game.Render(), !IsSimulation);
            }
        }

        private void ResourceLoop() {
            while (this.running) {
                if (ResourceManager.IsLoading)
                    ResourceManager.ContinueLoading();
                else
                    Thread.Sleep(250);
            }
        }

        private void GameArea_SizeChanged(object sender, EventArgs e) {
            //int ow = this.gameAreaWidth;
            //int oh = this.gameAreaHeight;

            //this.gameAreaWidth = this.gameArea.Width;
            //this.gameAreaHeight = this.gameArea.Height;

            //this.game.Viewport.Size = (this.gameAreaWidth, this.gameAreaHeight);
            //this.game.OnRenderAreaSizeChanged(this.gameAreaWidth, this.gameAreaHeight, ow, oh);
        }

        public float Time => this.upsTimer.RunTimeMilliseconds() / 1000f;

        internal Control GameArea => this.gameArea;

        public abstract string Name { get; }

        public int UpdatesPerSecond => this.upsTimer.TicksPerSecond;

        public int FramesPerSecond => this.fpsTimer.TicksPerSecond;

        public abstract Form Form { get; }

        public bool IsWindowVisible => Form.WindowState != FormWindowState.Minimized;
    }
}
