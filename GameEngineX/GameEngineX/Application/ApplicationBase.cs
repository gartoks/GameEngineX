﻿using GameEngineX.Game;
using GameEngineX.Input;
using GameEngineX.Utility;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using GameEngineX.Graphics;
using GameEngineX.Resources;

namespace GameEngineX.Application {
    public abstract class ApplicationBase : IRenderTarget {

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

        private GameBase game;

        private TimeTracker upsTimer;
        private TimeTracker fpsTimer;

        protected ApplicationBase() {
            ApplicationBase.instance = this;
        }

        public void Initialize(Control gameArea, GameBase game, ApplicationInitializationParameters parameters) {
            Renderer.DEFAULT_FONT_NAME = parameters.DefaultFont;
            Renderer.BACKGROUND_COLOR = parameters.BackgroundColor;

            IsSimulation = parameters.IsSimulation;

            this.gameArea = gameArea;
            //this.gameAreaWidth = gameArea.Width;
            //this.gameAreaHeight = gameArea.Height;
            this.gameArea.SizeChanged += GameArea_SizeChanged;

            ResourceManager.Initialize();

            InputHandler.Initialize(gameArea);

            this.game = game;

            this.upsTimer = new TimeTracker(parameters.UpdatesPerSecond);
            this.fpsTimer = new TimeTracker(parameters.FramesPerSecond);

            this.resourceThread = new Thread(ResourceLoop);
            this.renderThread = new Thread(RenderLoop);
            this.logicThread = new Thread(UpdateLoop);
        }

        public void Start() {
            this.running = true;

            this.game.Initialize();

            this.resourceThread.Start();

            this.renderThread.Start();

            this.logicThread.Start();
        }

        public void Exit() {
            this.running = false;
        }

        private void UpdateLoop() {
            while (this.running) {
                InputHandler.Update();

                this.upsTimer.FullTick(this.game.Update, !IsSimulation);

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

        public Control GameArea => this.gameArea;

        public abstract string Name { get; }

        public int UpdatesPerSecond => this.upsTimer.TicksPerSecond;

        public int FramesPerSecond => this.fpsTimer.TicksPerSecond;

        public abstract bool IsWindowVisible { get; }

        public System.Drawing.Graphics Graphics => System.Drawing.Graphics.FromHwnd(GameArea.Handle);

        public Rectangle TargetRectangle => GameArea.ClientRectangle;
    }
}
