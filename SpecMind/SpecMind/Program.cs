using System;
using Avalonia;
using SpecMind.Modules.AI.Runtime;

namespace SpecMind
{
    internal sealed class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // ==== ТЕСТ QWEN ====
            LLamaTest.RunAsync().GetAwaiter().GetResult();

            // ==== ЗАПУСК ПРИЛОЖЕНИЯ ====
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
#if DEBUG
                .WithDeveloperTools()
#endif
                .WithInterFont()
                .LogToTrace();
    }
}