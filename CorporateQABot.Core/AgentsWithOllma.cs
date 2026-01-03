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
            // Use the Ollama Gemma model for this conversation with showing debug in console
            var llm = OllamaModelHelpers.OllamaGemmaModel.UseConsoleForDebug();

            //string htmlContent = await GetLatestEnrichedConfluenceHtml();
            string confluenceWikiURL = "https://wiki.elm.sa/spaces/BJS/pages/248936913";

            var myAgents = new Agents(llm);
            var agents = new List<CrewAgent> { myAgents.RequirementsAnalysisAgent };
            var agentTasks = new List<AgentTask> 
            { 
                Tasks.AnalyzeRequirementsFromConfluence(myAgents.RequirementsAnalysisAgent, confluenceWikiURL) 
            };
            var crew = new Crew(agents, agentTasks);
            var runAsync = await crew.RunAsync();
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