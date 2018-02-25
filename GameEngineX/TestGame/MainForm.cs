using System;
using System.Drawing;
using System.Windows.Forms;
using GameEngineX.Application;

namespace TestGame {
    public partial class MainForm : Form {

        private static MainForm instance;
        public static MainForm Instance => MainForm.instance;

        public MainForm() {
            MainForm.instance = this;

            InitializeComponent();

            new TestApplication();

            // model - local object pos/rot/scale
            // view - camera pos/rot/scale
            // projection - pos/rot/scale to pixel
            // model view projection
        }

        private void Form1_Load(object sender, EventArgs e) {

            ApplicationInitializationParameters initializationParameters = new ApplicationInitializationParameters(
                15, 15, false);
            initializationParameters.BackgroundColor = Color.FromArgb(31, 31, 31);
            initializationParameters.ShowLogWindow = false;

            TestApplication.Instance.Initialize(RenderPanel, new TestGame(), initializationParameters);
            //Log.OnLog += (s, t) => Debug.WriteLine(s);
            TestApplication.Instance.Start();
        }
    }
}
