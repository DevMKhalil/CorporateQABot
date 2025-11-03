// See https://aka.ms/new-console-template for more information
using CorporateQABot.Core;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>();
var config = builder.Build();
//var apiKey = config["OpenAI:ApiKey"];
//var apiKey = config["Gemini:ApiKey"];

var ollamaService = new OllamaModelService();

//await ollamaService.RunOllamaModelAsync(Class1.OllamaQwenModelName, "List three benefits of using .NET for AI development:");
//await ollamaService.GenerateOllamaChatAsync(OllamaModelService.OllamaQwenModelName);
//await ollamaService.PromptTemplateFunc();
//await ollamaService.ChatPromptTemplates();
//await ollamaService.ConversationPromptTemplates();
//await ollamaService.ContinuesConversationPromptTemplates();
//await ollamaService.BasicFewShotLearningPrompt();
//await ollamaService.AdvancedFewShotLearningPrompt();
await ollamaService.AnotherAdvancedFewShotLearningPrompt();

//Console.WriteLine("Hello, World!");

