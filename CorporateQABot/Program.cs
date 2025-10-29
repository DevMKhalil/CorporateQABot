// See https://aka.ms/new-console-template for more information
using CorporateQABot.Core;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>();
var config = builder.Build();
//var apiKey = config["OpenAI:ApiKey"];
//var apiKey = config["Gemini:ApiKey"];

var class1 = new OllamaModelService();

//await class1.RunOllamaModelAsync(Class1.OllamaQwenModelName, "List three benefits of using .NET for AI development:");
//await class1.GenerateOllamaChatAsync(OllamaModelService.OllamaQwenModelName);
await class1.PromptTemplate();

//Console.WriteLine("Hello, World!");

