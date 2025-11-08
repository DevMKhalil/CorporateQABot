using LangChain.Providers.Ollama;

namespace CorporateQABot.Core
{
    internal static class OllamaModelHelpers
    {
        /// <summary>
        /// Default Gemma model identifier used by sample calls.
        /// </summary>
        public const string OllamaGemmaModelName = "gemma2:2b-instruct-q4_K_M";

        /// <summary>
        /// Default Qwen model identifier used by sample calls.
        /// </summary>
        public const string OllamaQwenModelName = "qwen3:1.7b-q4_K_M";

        /// <summary>
        /// Shared Gemma chat model instance configured for deterministic responses in sample chains.
        /// </summary>
        private static OllamaChatModel ollamaGemmaModel = new OllamaChatModel(new OllamaProvider(), OllamaGemmaModelName)
        {
            Settings = new OllamaChatSettings()
            {
                Temperature = 0.0f
            }
        };

        /// <summary>
        /// Shared Qwen chat model instance used by examples that target Qwen-specific behavior.
        /// </summary>
        private static OllamaChatModel ollamaQwenModel = new OllamaChatModel(new OllamaProvider(), OllamaQwenModelName);

        public static OllamaChatModel OllamaGemmaModel { get => ollamaGemmaModel; private set => ollamaGemmaModel = value; }
        public static OllamaChatModel OllamaQwenModel { get => ollamaQwenModel; private set => ollamaQwenModel = value; }
    }
}