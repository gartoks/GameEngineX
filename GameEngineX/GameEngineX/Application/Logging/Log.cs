using System;
using System.Drawing;

namespace GameEngineX.Application.Logging {
    public static class Log {

        private static Color messageColor;
        private static Color warningColor;
        private static Color errorColor;

        internal static void Initialize(Color messageColor, Color warningColor, Color errorColor) {
            Log.messageColor = messageColor;
            Log.warningColor = warningColor;
            Log.errorColor = errorColor;
        }

        public static event Action<string, Color> OnLog;

        public static void LogLine(string text, LogType messageType = LogType.Message) {
            DateTime timestamp = DateTime.Now;
            text = $"[{timestamp.Hour.ToString().PadLeft(2, '0')}:{timestamp.Minute.ToString().PadLeft(2, '0')}:{timestamp.Second.ToString().PadLeft(2, '0')}]: {text}";
            OnLog?.Invoke(text, LogTypeToColor(messageType));
        }

        public static Color MessageColor {
            get => Log.messageColor;
            set => Log.messageColor = value;
        }

        public static Color WarningColor {
            get => Log.warningColor;
            set => Log.warningColor = value;
        }

        public static Color ErrorColor {
            get => Log.errorColor;
            set => Log.errorColor = value;
        }

        private static Color LogTypeToColor(LogType logType) {
            switch (logType) {
                case LogType.Message:
                    return MessageColor;
                case LogType.Warning:
                    return WarningColor;
                case LogType.Error:
                    return ErrorColor;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
            }
        }

    }
}
