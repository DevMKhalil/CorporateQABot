using LangChain.Prompts;
using LangChain.Prompts.Base;
using LangChain.Providers;
using LangChain.Providers.Ollama;
using LangChain.Schema;
using Newtonsoft.Json.Linq;
using OllamaMsgExt = Ollama.StringExtensions;



namespace CorporateQABot.Core
{

    public class OllamaModelService
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
        /// Sends a chat-style request constructed from an array of <see cref="Message"/> to the specified Ollama model.
        /// The collection is converted to a <see cref="ChatRequest"/> before sending.
        /// </summary>
        /// <param name="modelName">Identifier of the Ollama model to invoke.</param>
        /// <param name="Messages">Collection of <see cref="Message"/> objects that form the conversation history.</param>
        /// <returns>
        /// A task that resolves to the model's last message content as a <see cref="string"/>.
        /// Returns the same content the underlying client exposes as <c>LastMessageContent</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="Messages"/> is <see langword="null"/> (propagated by helpers).</exception>
        /// <exception cref="Exception">Propagates any exception thrown by the underlying Ollama client.</exception>
        public async Task<string> RunOllamaModelAsync(string modelName, IReadOnlyCollection<Message> Messages,float temperature = (float)0.7)
        {
            var chat = ChatRequest.ToChatRequest(Messages);

            var aiModel = new OllamaChatModel(new OllamaProvider(), modelName);
            try
            {
                var chatSettings = OllamaChatSettings.Default;

                chatSettings.Temperature = temperature;

                var output = await aiModel.GenerateAsync(chat, chatSettings);

                return output.LastMessageContent;
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
        /// Demonstrates multiple ways to format prompt templates and submits the rendered prompts to the Gemma model.
        /// </summary>
        /// <returns>A task that completes when the prompts have been formatted, logged, and sent to the model.</returns>
        public async Task PromptTemplateFunc()
        {
            // one easy way to format a prompt
            var oneFormatedPrompt = PromptTemplate.RenderTemplate(
                "You are an expert in {topic}. Answer the following question with detail and clarity: {question}",
                TemplateFormatOptions.FString,
                new Dictionary<string, object>
                {
                    ["topic"] = "cloud security",
                    ["question"] = "What is Zero Trust, and why is it important for enterprise architects?"
                });

            Console.WriteLine(oneFormatedPrompt);

            await RunOllamaModelAsync(OllamaGemmaModelName, oneFormatedPrompt);

            //*****************************************************************************//
            // another way using PromptTemplate class
            var template = PromptTemplate.FromTemplate(
                "You are an expert in {topic}. Answer the following question with detail and clarity: {question}");

            var values = new InputValues(new Dictionary<string, object>
            {
                ["topic"] = "cloud security",
                ["question"] = "What is Zero Trust, and why is it important for enterprise architects?"
            });

            // Format the prompt with runtime values
            var formattedPrompt = await template.FormatAsync(values);

            Console.WriteLine(formattedPrompt);

            await RunOllamaModelAsync(OllamaGemmaModelName, oneFormatedPrompt);

            //*****************************************************************************//
            // more complex way using PromptTemplateInput
            var input = new PromptTemplateInput(
                "You are an expert in {topic}. Answer the following question with detail and clarity: {question}",
               new List<string> { "topic", "question" });

            // Define a reusable prompt template
            var hardTemplate = new PromptTemplate(input);

            var hardValues = new InputValues(new Dictionary<string, object>
            {
                ["topic"] = "cloud security",
                ["question"] = "What is Zero Trust, and why is it important for enterprise architects?"
            });

            // Format the prompt with runtime values
            var hardFormattedPrompt = await hardTemplate.FormatAsync(hardValues);

            Console.WriteLine(formattedPrompt);
        }

        /// <summary>
        /// Creates static and dynamic chat prompt templates, formats them with runtime values, and invokes the Gemma model with the resulting prompts.
        /// </summary>
        /// <returns>A task that completes when both prompts have been emitted and the model responses retrieved.</returns>
        /// <summary>
        /// Creates static and dynamic chat prompt templates, formats them with runtime values, and invokes the Gemma model with the resulting prompts.
        /// </summary>
        /// <returns>A task that completes when both prompts have been emitted and the model responses retrieved.</returns>
        public async Task ChatPromptTemplates()
        {
            // Static Chat Prompt Template
            var chatPrompt = ChatPromptTemplate.FromPromptMessages([
                SystemMessagePromptTemplate.FromTemplate(
                "You are an AI financial advisor."),
                 HumanMessagePromptTemplate.FromTemplate("What are the tax benefits of a Roth IRA?")
            ]);

            var formattedPrompt = await chatPrompt.FormatAsync(new InputValues(new Dictionary<string, object> { }));

            Console.WriteLine(formattedPrompt);

            await RunOllamaModelAsync(OllamaGemmaModelName, formattedPrompt);

            //*****************************************************************************//
            // Dynamic Chat Prompt Template
            var dynamicChatTemplate = ChatPromptTemplate.FromPromptMessages([
                SystemMessagePromptTemplate.FromTemplate(
                "You are a helpful assistant specialized in {domain}."),
                 HumanMessagePromptTemplate.FromTemplate(
                     "{question}")
            ]);

            var values = new InputValues(new Dictionary<string, object>
            {
                ["domain"] = "human resources",
                ["question"] = "How should we handle remote work requests?"
            });

            var chatMessage = await dynamicChatTemplate.FormatAsync(
                new InputValues(
                    new Dictionary<string, object>
                    {
                        ["domain"] = "human resources",
                        ["question"] = "How should we handle remote work requests?"
                    }
                )
            );

            Console.WriteLine(chatMessage);

            await RunOllamaModelAsync(OllamaGemmaModelName, chatMessage);
        }


        /// <summary>
        /// Builds a short, example conversation as a sequence of chat messages and sends it to the configured model.
        /// </summary>
        /// <remarks>
        /// The method creates a `messages` array containing a sequence of messages with different roles using
        /// the `AsSystemMessage`, `AsAiMessage`, and `AsHumanMessage` helpers. It writes the assembled messages to
        /// the console for debugging/inspection and then invokes `RunModelAsync` to submit the conversation
        /// to the `OllamaGemmaModelName`.
        ///
        /// The method does not catch exceptions that may originate from `RunModelAsync`; such exceptions will
        /// propagate to the caller.
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Propagates any exception thrown while sending the conversation to the model.</exception>
        public async Task ConversationPromptTemplates()
        {
            var messages = new[]
                {
                    "You are a chief, you want to ask me some things to find out my favorite Asian dish".AsSystemMessage(),
                    "Where are you from".AsAiMessage(),
                    "Egypt".AsHumanMessage(),
                    "Do you like spicy food?".AsAiMessage(),
                    "Not too much".AsHumanMessage(),
                    "Do you suffer from diabetes?".AsAiMessage(),
                    "yes".AsHumanMessage()
                };

            await RunOllamaModelAsync(OllamaGemmaModelName, messages);
        }

        /// <summary>
        /// Demonstrates continuing a conversation across multiple iterations:
        /// - Sends the current message history to the model.
        /// - Appends the model response to the history.
        /// - Reads a user reply from the console and appends it as a human message.
        /// The loop repeats several times and then asks a final question to produce a concluding response.
        /// </summary>
        /// <returns>A task that completes when the interactive conversation demonstration finishes.</returns>
        /// <exception cref="Exception">Propagates any exception thrown by the underlying Ollama client or console I/O.</exception>
        public async Task ContinuesConversationPromptTemplates()
        {
            var messages = new[]
                {
                    "You are a chief, you want to ask me some things to find out my favorite Asian dish".AsSystemMessage(),
                };

            for (int i = 0; i < 3; i++)
            {
                var res = await RunOllamaModelAsync(OllamaGemmaModelName, messages);

                Console.WriteLine("AI: " + res);

                messages = messages.Append(res.AsAiMessage()).ToArray();

                var userInput = Console.ReadLine() ?? string.Empty;

                messages = messages.Append(userInput.AsHumanMessage()).ToArray();
            }

            messages = messages.Append("And Now. Which plate do you think I will like. just type the plate name".AsHumanMessage()).ToArray();

            var finalResponse = await RunOllamaModelAsync(OllamaGemmaModelName, messages);

            Console.WriteLine("AI: " + finalResponse);
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
