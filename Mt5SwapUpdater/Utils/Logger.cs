using System;

namespace SwapUpdater.Utils
{
    public static class Logger
    {
        public static void Info(string message) => Console.WriteLine($"[INFO] {message}");
        public static void Error(string message) => Console.WriteLine($"[ERROR] {message}");
        public static void Warn(string message) => Console.WriteLine($"[WARN] {message}");
        public static void Exception(string message, Exception ex) =>
            Console.WriteLine($"[EXCEPTION] {message}\n{ex}");
    }
}
