//using LangChain.Providers;
using LangChain.Prompts;
using LangChain.Prompts;
using LangChain.Providers;
using LangChain.Providers.Ollama;
using LangChain.Schema;
using System.Threading.Tasks;



namespace CorporateQABot.Core
{

    public class OllamaModelService
    {
        public const string OllamaGemmaModelName = "gemma2:2b-instruct-q4_K_M";
        public const string OllamaQwenModelName = "qwen3:1.7b-q4_K_M";

        /// <summary>
        /// Sends a single message to the specified Ollama chat model and writes the model response to the console.
        /// </summary>
        /// <param name="modelName">Identifier of the Ollama model to invoke.</param>
        /// <param name="message">The user message that will be passed to the model.</param>
        /// <returns>A task that completes when the response has been written to the console.</returns>
        /// <exception cref="Exception">Propagates any exception thrown by the underlying Ollama client.</exception>
        public async Task RunOllamaModelAsync(string modelName,string message)
        {
            var aiModel = new OllamaChatModel(new OllamaProvider(), modelName);
            try
            {
                var output = await aiModel.GenerateAsync(message);
                Console.WriteLine(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Demonstrates a structured conversation with the specified Ollama chat model using predefined system and human messages.
        /// </summary>
        /// <param name="modelName">Identifier of the Ollama model to invoke.</param>
        /// <returns>A task that completes when the model output has been written to the console.</returns>
        /// <exception cref="Exception">Propagates any exception thrown by the underlying Ollama client.</exception>
        public async Task GenerateOllamaChatAsync(string modelName)
        {
            var aiModel = new OllamaChatModel(new OllamaProvider(), modelName);
            try
            {
                var messages = new[]
                {
                    "You are a helpful assistant for software architects.".AsSystemMessage(),
                    "What are the design patterns most relevant to AI apps?".AsHumanMessage()
                };

                var request = new ChatRequest()
                {
                    Messages = messages,
                };

                var output = await aiModel.GenerateAsync(messages);
                Console.WriteLine(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Builds and formats a prompt template with dynamic values, then writes the rendered prompt to the console.
        /// </summary>
        /// <returns>A task that completes when the formatted prompt has been written to the console.</returns>
        public async Task PromptTemplate()
        {

            var input = new PromptTemplateInput(
                "You are an expert in {topic}. Answer the following question with detail and clarity: {question}",
               new List<string> { "topic", "question" });

            // Define a reusable prompt template
            var template = new PromptTemplate(input);

            var values = new InputValues(new Dictionary<string, object>
            {
                ["topic"] = "cloud security",
                ["question"] = "What is Zero Trust, and why is it important for enterprise architects?"
            });

            // Format the prompt with runtime values
            var formattedPrompt = await template.FormatAsync(values);

            Console.WriteLine(formattedPrompt);
        }

        //public async Task Test1(string apiKey)
        //{
        //    //string modelName = "gemini-2.5-pro";
        //    string modelName = "gemini-2.0-flash-lite";
        //    var aiModel = new GoogleChatModel(apiKey, modelName);
        //    try
        //    {
        //        var request = new ChatRequest()
        //        { Messages =
        //            [
        //                "You are a helpful assistant.".AsSystemMessage(),
        //                "Hello, AI! What can you do?".AsHumanMessage()
        //            ],
        //            Tools = [
        //                new Tool
        //                {
        //                    Name = "get_current_time",
        //                    Description = "Get the current time in ISO 8601 format.",
        //                    Parameters = new
        //                    {
        //                        type = "object",
        //                        properties = new
        //                        {
        //                        },
        //                        //required = []
        //                    },
        //                    ["tool_type"] = new
        //                    {
        //                        // Example placeholder — library expects one initialized variant here.
        //                        // Replace with the real variant, e.g. {"function": { ... }} or whatever your SDK requires.
        //                    }
        //                }
        //                ],
        //            Image = null
        //        };
        //        var output = await aiModel.GenerateAsync(request);
        //        Console.WriteLine(output);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        throw;
        //    }
        //}

        //public async Task Test(string apiKey)
        //{
        //    var openAiModel = new OpenAiChatModel(apiKey, "gpt-4.1-nano");

        //    try
        //    {
        //        await foreach (var chunk in OpenAiThrottler.StreamAsync(
        //            () => openAiModel.GenerateAsync("Hello, AI! What can you do?")))
        //        {
        //            Console.WriteLine(chunk?.ToString());
        //        }
        //    }
        //    catch (tryAGI.OpenAI.ApiException ex) when ((int)ex.StatusCode == 429)
        //    {
        //        // ده لو فشل كل الـretries جوّا الـThrottler
        //        Console.WriteLine("Rate limited. Try again later.");
        //    }
        //    catch (tryAGI.OpenAI.ApiException ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        if (ex.ResponseBody != null)
        //            Console.WriteLine(ex.ResponseBody);
        //        throw;
        //    }
        //}
    }
}
