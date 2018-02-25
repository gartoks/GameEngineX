using System;
using System.Drawing;

namespace GameEngineX.Application {
    public class ApplicationInitializationParameters {
        private int updatesPerSecond;
        private int framesPerSecond;

        public Color BackgroundColor;
        private string defaultFont;

        public bool IsSimulation;

        public bool ShowLogWindow;
        public Color LoggingMessageColor;
        public Color LoggingWarningColor;
        public Color LoggingErrorColor;

        public ApplicationInitializationParameters(int updatesPerSecond, int framesPerSecond, bool isSimulation) {
            UpdatesPerSecond = updatesPerSecond;
            FramesPerSecond = framesPerSecond;

            IsSimulation = isSimulation;

            BackgroundColor = Color.Black;

            DefaultFont = "Arial";

            ShowLogWindow = true;
            LoggingMessageColor = Color.Black;
            LoggingWarningColor = Color.Orange;
            LoggingErrorColor = Color.Red;
        }

        public int UpdatesPerSecond {
            get => this.updatesPerSecond;
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Updates per second must be bigger than 0.");
                this.updatesPerSecond = value;
            }
        }

        public int FramesPerSecond {
            get => this.framesPerSecond;
            set {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Frames per second must be bigger than 0.");
                this.framesPerSecond = value;
            }
        }

        public string DefaultFont {
            get => this.defaultFont;
            set {
                try {
                    new Font(defaultFont, 1.0f);
                } catch (ArgumentException) {
                    throw new ArgumentException("Invalid default font.", nameof(defaultFont));
                }
                this.defaultFont = value;
            }
        }
    }
}
