using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameEngineX.Application.Logging {
    public partial class LogWindow : Form {
        public LogWindow() {
            InitializeComponent();
        }

        public void SetUpsFps(float ups, float fps) {
            Invoke((Action)(() => Text = $"{ups}ups     {fps}fps"));

        }

        public void AddLine(string line, Color color) {
            Invoke((Action)(() => ProcessLine(line, color)));
        }

        private void ProcessLine(string line, Color color) {
            rtbx_log.SelectionColor = color;
            rtbx_log.AppendText($"{line}\n");

            rtbx_log.ScrollToCaret();
        }
    }

}
