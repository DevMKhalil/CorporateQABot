// See https://aka.ms/new-console-template for more information
using CorporateQABot.Core;
using CorporateQABot.Core.Confluence;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
    .AddUserSecrets<Program>();
var config = builder.Build();
//var apiKey = config["OpenAI:ApiKey"];
//var apiKey = config["Gemini:ApiKey"];

var ollamaService = new OllamaModelService();
var ollamaChains = new ChainsWithOllama();
var agentsWithOllma = new AgentsWithOllma();
var confluenceService = new ConfluenceService(string.Empty,string.Empty);

//await ollamaService.RunOllamaModelAsync(Class1.OllamaQwenModelName, "List three benefits of using .NET for AI development:");
//await ollamaService.GenerateOllamaChatAsync(OllamaModelService.OllamaQwenModelName);
//await ollamaService.PromptTemplateFunc();
//await ollamaService.ChatPromptTemplates();
//await ollamaService.ConversationPromptTemplates();
//await ollamaService.ContinuesConversationPromptTemplates();
//await ollamaService.BasicFewShotLearningPrompt();
//await ollamaService.AdvancedFewShotLearningPrompt();
//await ollamaService.AnotherAdvancedFewShotLearningPrompt();
//await ollamaService.CommaSeparatedListOutputParserResult();
//await ollamaService.CustomOutputParserResult();
//await ollamaService.PydanticOutputParserResult();
//await ollamaService.DealingWithSequentialChain();
//await ollamaService.DealingWithChainWithInputVariables();
//await ollamaService.DealingWithChainWithOutInputVariables();
//await ollamaService.DealingWithChainAndChatPromptTemplate();
//await ollamaService.DealingWithLCELBasicChain();
//await ollamaChains.CreateRunnableWith_ConversationBufferMemory_History();
//await ollamaChains.CreateRunnableWith_ConversationBufferWindowMemory_History();
//await ollamaChains.CreateRunnableWith_ConversationSummaryMemory_History();
//await ollamaChains.CreateRunnableWith_ConversationBufferMemory_History_WithSavingHistory();
await confluenceService.LoadPageContextAsync(string.Empty);

//await agentsWithOllma.Create_Agent_For_Generate_Keywords();

//Console.WriteLine("Hello, World!");

