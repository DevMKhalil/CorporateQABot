using CorporateQABot.Core.Memory;
using LangChain.Memory;
using LangChain.Providers;
using static LangChain.Chains.Chain;

namespace CorporateQABot.Core
{
    /// <summary>
    /// Sample routines that demonstrate composing LangChain runnable chains with various memory implementations backed by Ollama models.
    /// </summary>
    public class ChainsWithOllama
    {
        /// <summary>
        /// Creates an interactive console loop that uses <see cref="ConversationBufferMemory"/> to retain the full chat history.
        /// </summary>
        /// <returns>A task that completes when the user exits the loop.</returns>
        public async Task CreateRunnableWith_ConversationBufferMemory_History()
        {
            var template = @"
The following is a friendly conversation between a human and an AI.

{history}
Human: {input}
AI:";

            var chatHistory = new ChatMessageHistory();

            // Create conversation buffer memory
            var memory = new ConversationBufferMemory(chatHistory)
            {
                Formatter = new MessageFormatter { AiPrefix = "AI", HumanPrefix = "abo khalil" },
            };

            // Use the Ollama Gemma model for this conversation with showing debug in console
            var llm = OllamaModelHelpers.OllamaGemmaModel.UseConsoleForDebug();

            var baseChain =
                LoadMemory(memory, outputKey: "history")
              | Template(template)
              | LLM(llm)
              | UpdateMemory(memory, requestKey: "input", responseKey: "text");

            baseChain.Name = @"Conversation Buffer Memory Chain.";

            while (true)
            {
                Console.Write("abo khalil: ");
                var input = Console.ReadLine();
                if (input is null || input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                var turn = Set(input, "input") | baseChain;

                // the rsult contains the full set of outputs from the chain
                var result = await turn.RunAsync();

                var reply = result.Value["text"];
                Console.WriteLine("AI: " + reply);
                Console.WriteLine("=====================================");
                Console.WriteLine($"LLM usage: {OllamaModelHelpers.OllamaGemmaModel.Usage}");
            }
        }

        /// <summary>
        /// Runs an interactive loop that leverages <see cref="ConversationWindowBufferMemory"/> to keep only the most recent turns.
        /// </summary>
        /// <returns>A task that completes when the user exits the loop.</returns>
        public async Task CreateRunnableWith_ConversationBufferWindowMemory_History()
        {
            var template = @"
The following is a friendly conversation between a human and an AI.

{history}
Human: {input}
AI:";

            var chatHistory = new ChatMessageHistory();

            // Create conversation window buffer memory with window size of 1
            var memory = new ConversationWindowBufferMemory(chatHistory)
            {
                Formatter = new MessageFormatter { AiPrefix = "AI", HumanPrefix = "abo khalil" },
                // for window buffer memory, set the window size that defines how many of the most recent messages to keep
                WindowSize = 1
            };

            // Use the Ollama Gemma model for this conversation with showing debug in console
            var llm = OllamaModelHelpers.OllamaGemmaModel.UseConsoleForDebug();

            var baseChain =
                LoadMemory(memory, outputKey: "history")
              | Template(template)
              | LLM(llm)
              | UpdateMemory(memory, requestKey: "input", responseKey: "text");

            baseChain.Name = @"Conversation Buffer Memory Chain.";

            while (true)
            {
                Console.Write("abo khalil: ");
                var input = Console.ReadLine();
                if (input is null || input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                var turn = Set(input, "input") | baseChain;

                // the rsult contains the full set of outputs from the chain
                var result = await turn.RunAsync();

                var reply = result.Value["text"];
                Console.WriteLine("AI: " + reply);
                Console.WriteLine("=====================================");
                Console.WriteLine($"LLM usage: {OllamaModelHelpers.OllamaGemmaModel.Usage}");
                Console.WriteLine("=====================================");
            }
        }

        /// <summary>
        /// Demonstrates <see cref="ConversationSummaryMemory"/> which summarizes older turns while maintaining conversational context.
        /// </summary>
        /// <returns>A task that completes when the user exits the loop.</returns>
        public async Task CreateRunnableWith_ConversationSummaryMemory_History()
        {
            var template = @"
The following is a friendly conversation between a human and an AI.

{history}
Human: {input}
AI:";

            // Use the Ollama Gemma model for this conversation with showing debug in console
            var llm = OllamaModelHelpers.OllamaGemmaModel.UseConsoleForDebug();

            var chatHistory = new ChatMessageHistory();

            // Create conversation window buffer memory with window size of 1
            var memory = new ConversationSummaryMemory(llm,chatHistory)
            {
                Formatter = new MessageFormatter { AiPrefix = "AI", HumanPrefix = "abo khalil" },
            };

            var baseChain =
                LoadMemory(memory, outputKey: "history")
              | Template(template)
              | LLM(llm)
              | UpdateMemory(memory, requestKey: "input", responseKey: "text");

            baseChain.Name = @"Conversation Buffer Memory Chain.";

            while (true)
            {
                //Console.Write("abo khalil: ");
                var input = Console.ReadLine();
                if (input is null || input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                var turn = Set(input, "input") | baseChain;

                // the rsult contains the full set of outputs from the chain
                var result = await turn.RunAsync();

                var reply = result.Value["text"];
                //Console.WriteLine("AI: " + reply);
                Console.WriteLine("=====================================");
                Console.WriteLine($"LLM usage: {OllamaModelHelpers.OllamaGemmaModel.Usage}");
                Console.WriteLine("=====================================");
            }
        }

        /// <summary>
        /// Builds a conversation window memory backed by <see cref="FileChatMessageHistoryNewVersion"/> to persist turns across sessions.
        /// </summary>
        /// <returns>A task that completes when the user exits the loop.</returns>
        public async Task CreateRunnableWith_ConversationBufferMemory_History_WithSavingHistory()
        {
            var template = @"
The following is a friendly conversation between a human and an AI.

{history}
Human: {input}
AI:";

            // Use the Ollama Gemma model for this conversation with showing debug in console
            var llm = OllamaModelHelpers.OllamaGemmaModel.UseConsoleForDebug();

            //var path = Helpers.GetDataChatsFilePath("chat_history.json");
            //var chatHistory = await FileChatMessageHistory.CreateAsync("chat_history.json");

            var path = Helper.GetDataChatsFilePath("chat_history_new_version.json");
            var chatHistory = new Memory.History.FileChatMessageHistoryNewVersion(path);

            // Create conversation window buffer memory with window size of 1
            var memory = new ConversationWindowBufferMemory(chatHistory)
            {
                Formatter = new MessageFormatter { AiPrefix = "AI", HumanPrefix = "abo khalil" },
                WindowSize = 4
            };

            var baseChain =
                LoadMemory(memory, outputKey: "history")
              | Template(template)
              | LLM(llm)
              | UpdateMemory(memory, requestKey: "input", responseKey: "text");

            baseChain.Name = @"Conversation Buffer Memory Chain.";

            while (true)
            {
                //Console.Write("abo khalil: ");
                var input = Console.ReadLine();
                if (input is null || input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

                var turn = Set(input, "input") | baseChain;

                // the rsult contains the full set of outputs from the chain
                var result = await turn.RunAsync();

                var reply = result.Value["text"];
                //Console.WriteLine("AI: " + reply);
                Console.WriteLine("=====================================");
                Console.WriteLine($"LLM usage: {OllamaModelHelpers.OllamaGemmaModel.Usage}");
                Console.WriteLine("=====================================");
            }
        }
    }
}
