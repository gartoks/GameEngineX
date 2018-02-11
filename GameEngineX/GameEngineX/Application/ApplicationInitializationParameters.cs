using System;
using System.Drawing;

namespace GameEngineX.Application {
    public class ApplicationInitializationParameters {
        public int UpdatesPerSecond;
        public int FramesPerSecond;

        public Color BackgroundColor;
        public string DefaultFont;

        public bool IsSimulation;

        public ApplicationInitializationParameters(int updatesPerSecond, int framesPerSecond, Color backgroundColor, string defaultFont, bool isSimulation) {
            if (UpdatesPerSecond <= 0)
                throw new ArgumentOutOfRangeException(nameof(updatesPerSecond), "Updates per second must be bigger than 0.");
            UpdatesPerSecond = updatesPerSecond;

            if (FramesPerSecond <= 0)
                throw new ArgumentOutOfRangeException(nameof(framesPerSecond), "frames per second must be bigger than 0.");
            FramesPerSecond = framesPerSecond;

            if (backgroundColor == null)
                throw new ArgumentNullException(nameof(backgroundColor));
            BackgroundColor = backgroundColor;

            try {
                new Font(defaultFont, 1.0f);
            }
            catch (ArgumentException) {
                throw new ArgumentException("Invalid default font.", nameof(defaultFont));
            }
            DefaultFont = defaultFont;

            IsSimulation = isSimulation;
        }
    }
}
