using CorporateQABot.Core.Confluence;
using LangChain.Chains.StackableChains.Agents.Crew;
using LangChain.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateQABot.Core
{
    public class AgentsWithOllma
    {
        public async Task Create_Agent_For_Generate_Keywords()
        {
            // Use the Ollama Gemma model for this conversation with showing debug in console
            var llm = OllamaModelHelpers.OllamaGemmaModel.UseConsoleForDebug();

            const string keyWord = "oven";

            var myAgents = new Agents(llm);

            var agents = new List<CrewAgent> { myAgents.KeyWordGenerationAgent };

            var agentTasks = new List<AgentTask> { Tasks.GenerateKeyWords(myAgents.KeyWordGenerationAgent, keyWord) };

            var crew = new Crew(agents, agentTasks);
            var runAsync = await crew.RunAsync();
        }

        public async Task Create_Agent_For_Generate_Prompts()
        {
            // ✅ RECOMMENDED: Use Qwen 3 4B Thinking model for superior ReAct pattern compliance
            // This model excels at:
            // - Following structured output formats (Thought -> Action -> Action Input)
            // - Handling multilingual requirements (Arabic/English)
            // - Generating comprehensive, detailed requirements documents
            // - Reliable tool usage and reasoning
            var llm = OllamaModelHelpers.OllamaQwen3ThinkingModel.UseConsoleForDebug();
            
            // Alternative models (uncomment to test if needed):
            // var llm = OllamaModelHelpers.OllamaGemmaModel.UseConsoleForDebug();   // Original (less reliable for ReAct)
            // var llm = OllamaModelHelpers.OllamaQwenModel.UseConsoleForDebug();    // Smaller Qwen 1.7B

            //string htmlContent = await GetLatestEnrichedConfluenceHtml();
            string confluenceWikiURL = "https://wiki.elm.sa/spaces/BJS/pages/248936913";

            var myAgents = new Agents(llm);
            //var agents = new List<CrewAgent> { myAgents.RequirementsAnalysisAgent };
            //var agentTasks = new List<AgentTask> 
            //{ 
            //    Tasks.AnalyzeRequirementsFromConfluence(myAgents.RequirementsAnalysisAgent, confluenceWikiURL) 
            //};

            //var crew = new Crew(agents, agentTasks);

            var tasks = new Tasks();

            var ss = tasks.AnalyzeRequirementsFromConfluence(myAgents.RequirementsAnalysisAgent, confluenceWikiURL);

            ss.Tools.Add(new ConfluencePageLoaderTool());

            try
            {
                Console.WriteLine("═══════════════════════════════════════════════════════════════════");
                Console.WriteLine("🚀 Starting Requirements Analysis Agent with Qwen 3 4B Thinking");
                Console.WriteLine($"📄 Confluence URL: {confluenceWikiURL}");
                Console.WriteLine("═══════════════════════════════════════════════════════════════════\n");

                //var runAsync = await crew.RunAsync();
                var res = await ss.ExecuteAsync(string.Empty);

                Console.WriteLine("\n═══════════════════════════════════════════════════════════════════");
                Console.WriteLine("✅ Agent task completed successfully!");
                Console.WriteLine("═══════════════════════════════════════════════════════════════════");
                //Console.WriteLine(runAsync);
                Console.WriteLine(res);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n═══════════════════════════════════════════════════════════════════");
                Console.WriteLine("❌ Agent task failed!");
                Console.WriteLine("═══════════════════════════════════════════════════════════════════");
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"\nInner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Type: {ex.InnerException.GetType().Name}");
                }
                Console.WriteLine("\n💡 Troubleshooting Tips:");
                Console.WriteLine("   1. Ensure Ollama is running: 'ollama serve'");
                Console.WriteLine("   2. Verify model is pulled: 'ollama pull qwen2.5:4b-thinking-2507-q4_K_M'");
                Console.WriteLine("   3. Check model name matches exactly in Ollama");
                Console.WriteLine("   4. Review agent backstory for ReAct format instructions");
                Console.WriteLine("   5. Ensure agent uses ONLY confluence_loader tool (no collaboration tools)");
                throw;
            }
        }

        private static async Task<string> GetLatestEnrichedConfluenceHtml()
        {
            // Get the newest enriched_page_** file from ConfluenceOutput directory
            var confluenceOutputDir = "C:\\Users\\NTG\\source\\repos\\CorporateQABot\\CorporateQABot.Core\\Confluence\\ConfluenceOutput";
            var newestEnrichedFile = Directory.GetFiles(confluenceOutputDir, "enriched_page_*.html")
                .OrderByDescending(f => File.GetCreationTime(f))
                .FirstOrDefault();

            if (string.IsNullOrEmpty(newestEnrichedFile))
            {
                throw new FileNotFoundException("No enriched_page_*.html files found in ConfluenceOutput directory");
            }

            Console.WriteLine($"Loading newest enriched file: {Path.GetFileName(newestEnrichedFile)}");
            var htmlContent = await File.ReadAllTextAsync(newestEnrichedFile);
            return htmlContent;
        }
    }
}