using BepInEx.Logging;
using System;


namespace NoMapTools.common {
    internal class Logger {
        public static LogLevel Level = LogLevel.Info;

        public static void enableDebugLogging(object sender, EventArgs e) {
            if (ValConfig.EnableDebugMode.Value) {
                Level = LogLevel.Debug;
            } else {
                Level = LogLevel.Info;
            }
            // set log level
        }

        public static void setDebugLogging(bool state) {
            if (state) {
                Level = LogLevel.Debug;
            } else {
                Level = LogLevel.Info;
            }
        }

        public static void LogDebug(string message) {
            if (Level >= LogLevel.Debug) {
                NoMapTools.Log.LogInfo(message);
            }
        }
        public static void LogInfo(string message) {
            if (Level >= LogLevel.Info) {
                NoMapTools.Log.LogInfo(message);
            }
        }

        public static void LogWarning(string message) {
            if (Level >= LogLevel.Warning) {
                NoMapTools.Log.LogWarning(message);
            }
        }

        public static void LogError(string message) {
            if (Level >= LogLevel.Error) {
                NoMapTools.Log.LogError(message);
            }
        }
    }
}
