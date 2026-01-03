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
        /// Qwen 3 4B Thinking model - RECOMMENDED for ReAct agent workflows.
        /// Superior instruction following, reasoning capabilities, and structured output generation.
        /// Excellent multilingual support (Arabic/English) for requirements extraction.
        /// </summary>
        public const string OllamaQwen3ThinkingModelName = "qwen3:4b-thinking-2507-q4_K_M";

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
        private static OllamaChatModel ollamaQwenModel = new OllamaChatModel(new OllamaProvider(), OllamaQwenModelName)
        {
            Settings = new OllamaChatSettings()
            {
                // Critical settings for ReAct pattern compliance
                Temperature = 0.0f,              // Zero temperature for maximum consistency and format adherence
                TopP = 0.95f,                   // Slight diversity for reasoning while maintaining structure
                TopK = 40,                      // Limit vocabulary for structured output
                RepeatPenalty = 1.15f,          // Prevent repetitive responses
                NumPredict = 8192,              // Large token limit for comprehensive requirements documents
                NumCtx = 16384                  // Large context window (Qwen 3 supports up to 256k)

                // Note: Stop sequences would prevent conversational drift but are not available in this LangChain version.
                // The agent backstory includes explicit format instructions to compensate.
            }
        };

        /// <summary>
        /// Qwen 3 4B Thinking model optimized for ReAct pattern compliance.
        /// Configured for maximum consistency and structured output generation.
        /// Ideal for agent workflows requiring strict format adherence (Thought -> Action -> Action Input).
        /// </summary>
        private static OllamaChatModel ollamaQwen3ThinkingModel = new OllamaChatModel(new OllamaProvider(), OllamaQwen3ThinkingModelName)
        {
            Settings = new OllamaChatSettings()
            {
                // Critical settings for ReAct pattern compliance
                Temperature = 0.0f,              // Zero temperature for maximum consistency and format adherence
                TopP = 0.95f,                   // Slight diversity for reasoning while maintaining structure
                TopK = 40,                      // Limit vocabulary for structured output
                RepeatPenalty = 1.15f,          // Prevent repetitive responses
                NumPredict = 8192,              // Large token limit for comprehensive requirements documents
                NumCtx = 16384                  // Large context window (Qwen 3 supports up to 256k)
                
                // Note: Stop sequences would prevent conversational drift but are not available in this LangChain version.
                // The agent backstory includes explicit format instructions to compensate.
            }
        };

        public static OllamaChatModel OllamaGemmaModel { get => ollamaGemmaModel; private set => ollamaGemmaModel = value; }
        public static OllamaChatModel OllamaQwenModel { get => ollamaQwenModel; private set => ollamaQwenModel = value; }
        
        /// <summary>
        /// Qwen 3 4B Thinking model - RECOMMENDED for ReAct agents.
        /// Use this for requirements extraction and other agent workflows requiring structured output.
        /// </summary>
        public static OllamaChatModel OllamaQwen3ThinkingModel { get => ollamaQwen3ThinkingModel; private set => ollamaQwen3ThinkingModel = value; }
    }
}