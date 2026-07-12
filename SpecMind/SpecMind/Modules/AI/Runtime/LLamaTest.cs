using LLama;
using LLama.Common;

namespace SpecMind.Modules.AI.Runtime;

public static class LLamaTest
{
    public static async Task RunAsync()
    {
        try
        {
            string modelPath = Path.Combine(
                AppContext.BaseDirectory,
                "Models",
                "qwen2.5-3b-instruct-q4_k_m.gguf");

            Console.WriteLine("=== LLama TEST ===");
            Console.WriteLine(modelPath);

            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 4096,
                GpuLayerCount = 0
            };

            Console.WriteLine("Loading weights...");

            var weights = LLamaWeights.LoadFromFile(parameters);

            Console.WriteLine("Creating context...");

            using var context = weights.CreateContext(parameters);

            Console.WriteLine("Creating executor...");

            var executor = new InteractiveExecutor(context);

            Console.WriteLine("Generating...");

            var inference = new InferenceParams()
            {
                MaxTokens = 128
            };

            string result = "";

            await foreach (var token in executor.InferAsync(
                "Привет! Представься.",
                inference))
            {
                Console.Write(token);

                result += token;
            }

            Console.WriteLine();
            Console.WriteLine("==============");
            Console.WriteLine(result);
            Console.WriteLine("==============");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            throw;
        }
    }
}