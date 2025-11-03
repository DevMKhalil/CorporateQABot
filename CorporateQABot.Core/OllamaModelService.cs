using LangChain.Prompts;
using LangChain.Prompts.Base;
using LangChain.Providers;
using LangChain.Providers.Ollama;
using LangChain.Schema;
using Newtonsoft.Json.Linq;
using OpenAI.Chat;
using System.Text;
using OllamaMsgExt = Ollama.StringExtensions;



namespace CorporateQABot.Core
{

    /// <summary>
    /// Provides sample routines for constructing prompts and interacting with
    /// Ollama chat models (e.g., Gemma, Qwen) using the LangChain providers.
    /// Methods demonstrate single message calls, chat history calls, and
    /// several prompt templating patterns (basic, chat, and few-shot).
    /// </summary>
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
        /// Sends a single message to the specified Ollama chat model and returns
        /// the model's last message content.
        /// </summary>
        /// <param name="modelName">Identifier of the Ollama model to invoke.</param>
        /// <param name="message">The user message that will be passed to the model.</param>
        /// <param name="temperature">Sampling temperature (controls randomness).</param>
        /// <returns>The model's last message content.</returns>
        /// <exception cref="Exception">Propagates any exception thrown by the underlying Ollama client.</exception>
        public async Task<string> RunOllamaModelAsync(string modelName,string message, float temperature = (float)0.7)
        {
            var aiModel = new OllamaChatModel(new OllamaProvider(), modelName);
            try
            {
                var chatSettings = OllamaChatSettings.Default;

                chatSettings.Temperature = temperature;

                var output = await aiModel.GenerateAsync(message, chatSettings);

                return output.LastMessageContent;
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
        /// <param name="temperature">Sampling temperature (controls randomness).</param>
        /// <returns>The model's last message content.</returns>
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
        /// <returns>A task that completes after writing the model output to the console.</returns>
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
        /// <remarks>
        /// Shows three approaches:
        /// - Using <see cref="PromptTemplate.RenderTemplate(string, TemplateFormatOptions, IDictionary{string, object})"/> with <see cref="TemplateFormatOptions.FString"/>.
        /// - Creating a <see cref="PromptTemplate"/> via <see cref="PromptTemplate.FromTemplate(string)"/> and formatting with <see cref="PromptTemplate.FormatAsync(InputValues, System.Threading.CancellationToken)"/>.
        /// - Defining <see cref="PromptTemplateInput"/> with explicit input variables, constructing a <see cref="PromptTemplate"/>, and formatting with <see cref="PromptTemplate.FormatAsync(InputValues, System.Threading.CancellationToken)"/>.
        /// Each formatted prompt is printed and then sent to the model.
        /// </remarks>
        /// <returns>A task that completes after printing formatted prompts and sending them to the model.</returns>
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
        /// Creates static and dynamic chat prompt templates, formats them with runtime values,
        /// and invokes the Gemma model with the resulting prompts.
        /// </summary>
        /// <returns>A task that completes after printing and sending both prompts.</returns>
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

            //var values = new InputValues(new Dictionary<string, object>
            //{
            //    ["domain"] = "human resources",
            //    ["question"] = "How should we handle remote work requests?"
            //});

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
        /// Builds a short example conversation as a sequence of chat messages and sends it to the configured model.
        /// </summary>
        /// <remarks>
        /// Creates a message array using the `AsSystemMessage`, `AsAiMessage`, and `AsHumanMessage` helpers,
        /// then submits the collection to <see cref="RunOllamaModelAsync(string, IReadOnlyCollection{Message}, float)"/>.
        /// Exceptions from the underlying call are not handled here and will propagate.
        /// </remarks>
        /// <returns>A task that completes after sending the conversation to the model.</returns>
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

        /// <summary>
        /// Builds and sends a basic few-shot prompt to infer a country’s
        /// capital and a related city given the country name, using the
        /// default Ollama Gemma model.
        /// </summary>
        /// <remarks>
        /// The prompt is created via <see cref="Helper.FewShotPrompt"/> using
        /// a small set of example country/capital/city tuples to guide the
        /// model. The composed prompt is printed, then sent to the model with
        /// default temperature settings, and the model’s response is written
        /// to the console.
        /// </remarks>
        /// <returns>
        /// A task that completes after the model response is written to the
        /// console.
        /// </returns>
        /// <exception cref="System.Exception">
        /// Propagates any exception thrown by the underlying model invocation.
        /// </exception>
        public async Task BasicFewShotLearningPrompt()
        {

            var examples = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    ["country"] = "France", ["capital"] = "Paris", ["city"] = "Nice"
                },
                new Dictionary<string, object>
                {
                    ["country"] = "Eygpt",["capital"] = "Cairo", ["city"] = "Aswan"
                },
                new Dictionary<string, object>
                {
                    ["country"] = "Saudi Arabia", ["capital"] = "Riyadh", ["city"] = "Jeddah"
                }
            };

            var prompt = await Helper.FewShotPrompt(
                "Country Name: {country} | Capital Name: {capital} | City Name: {city}",
                "Country Name: {country} |",
                new Dictionary<string, object> { ["country"] = "Italy" },
                examples
                );

            Console.WriteLine(prompt);

            var res = await RunOllamaModelAsync(OllamaGemmaModelName, prompt);

            Console.WriteLine("AI: " + res);
        }

        /// <summary>
        /// Builds and sends an advanced few-shot prompt that demonstrates
        /// step-by-step (chain-of-thought) reasoning for simple math/logic
        /// word problems using the default Ollama Gemma model.
        /// </summary>
        /// <remarks>
        /// The prompt is composed via <see cref="Helper.FewShotPrompt"/> with
        /// multiple worked examples to guide the model toward structured
        /// reasoning. The model is then invoked with temperature set to 0 to
        /// encourage deterministic output, and the response is written to the
        /// console.
        /// </remarks>
        /// <returns>
        /// A task that completes after the model response is written to the
        /// console.
        /// </returns>
        /// <exception cref="System.Exception">
        /// Propagates any exception thrown by the underlying model invocation.
        /// </exception>
        public async Task AdvancedFewShotLearningPrompt()
        {

            var problemPrompt = "Total with what my family is 50 oranges. if we subtract one-fifth of the number from them, and add ten more oranges, how many oranges will there be in the end?";

            var examples = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>
                {
                    ["problem"] = "When I was seven, my sister was twice my age. now I am seventy years old, how old can my sister be?",
                    ["answer"] = string.Join("\n", new List<string>
                    {
                        "We will followup some questions to get the answer.",
                        "Follow up: How old was your sister when you were seven?",
                        "Intermediate answer: Twice, which mean 14 years.",
                        "Follow up: What is the difference between your age and your sister's age?",
                        "Intermediate answer: 14 years - 7 years = 7 years.",
                        "Follow up: When you were seventy years old, how old would your sister be?",
                        "Intermediate answer: my age (70) + The difference between me and my sister's age (7) = 77 years.",
                        "Final Answer: 77 years."
                    })
                },
                new Dictionary<string, object>
                {
                    ["problem"] = "I have two apples, and my friend has three times what I have plus four. how many apples does my friend have?",
                    ["answer"] = string.Join("\n",new List<string>
                    {
                        "We will followup some questions to get the answer.",
                        "Follow up: How many apples do I have?",
                        "Intermediate answer: I have 2 apples.",
                        "Follow up: How many times more does my friend have than I do?",
                        "Intermediate answer: My friend has 3 times what I have plus 4 more.",
                        "Follow up: when you have two apples, how many apples does your friend have?",
                        "Intermediate answer: your apples (2) × times (3) = 6, then 6 + the plus appels (4) = 10.",
                        "Final Answer: My friend has 10 apples."
                    })
                },
                new Dictionary<string, object>
                {
                    ["problem"] = "I have ten bananas, and my brother has half of what I have minus two. how many bananas does my brother have?",
                    ["answer"] = string.Join("\n",new List<string>
                    {
                        "We will followup some questions to get the answer.",
                        "Follow up: How many bananas do I have?",
                        "Intermediate answer: I have 10 bananas.",
                        "Follow up: How many bananas does my brother have?",
                        "Intermediate answer: He has half of what I have minus 2.",
                        "Follow up: when you have ten bananas, how many bananas does your brother have?",
                        "Intermediate answer: Half of your bannanas (10) is (5) bannanas, and 5 - the minus (2) = 3.",
                        "Final Answer: My brother has 3 bananas."
                    })
                },
                new Dictionary<string, object>
                {
                    ["problem"] = "I have eight grapes, and my sister has four times what I have plus one. how many grapes does my sister have?",
                    ["answer"] = string.Join("\n",new List<string>
                    {
                        "We will followup some questions to get the answer.",
                        "Follow up: How many grapes do I have?",
                        "Intermediate answer: I have 8 grapes.",
                        "Follow up: How many times more does my sister have?",
                        "Intermediate answer: She has 4 times what I have plus 1 more.",
                        "Follow up: when you have eight grapes, how many grapes does your sister have?",
                        "Intermediate answer: your grapes (8) × times (4) = 32, and 32 + the plus (1) = 33.",
                        "Final Answer: My sister has 33 grapes."
                    })
                },
                new Dictionary<string, object>
                {
                    ["problem"] = "I have five oranges, and the sum of what my sister and brother have is three times what I have plus thirty-five.",
                    ["answer"] = string.Join("\n", new List<string>
                    {
                        "We will followup some questions to get the answer.",
                        "Follow up: How many oranges do I have?",
                        "Intermediate answer: I have 5 oranges.",
                        "Follow up: What is the sum of what my sister and brother have?",
                        "Intermediate answer: It is three times what I have plus 35.",
                        "Follow up: When you have five oranges, how many do your sister and brother have together?",
                        "Intermediate answer: your oranges (5) × times (3) = 15, then 15 + the plus oranges (35) = 50.",
                        "Final Answer: Together, my sister and brother have 50 oranges."
                    })
                }
            };

            var prompt = await Helper.FewShotPrompt(
                "\nProblem: {problem}\n-------------\nAnswer: {answer}",
                "Problem: {problem} \n-------------\n",
                new Dictionary<string, object> { ["problem"] = problemPrompt },
                examples
                );

            //Console.WriteLine(prompt);

            var res = await RunOllamaModelAsync(OllamaGemmaModelName, prompt, 0);

            Console.WriteLine("AI: " + res);
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
