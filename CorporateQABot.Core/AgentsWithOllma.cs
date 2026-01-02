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

        //public async Task Create_Agent_For_Generate_Questions()
        //{
        //    // Use the Ollama Gemma model for this conversation with showing debug in console
        //    var llm = OllamaModelHelpers.OllamaGemmaModel.UseConsoleForDebug();
        //    const string keyWord = "oven";
        //    var myAgents = new Agents(llm);
        //    var agents = new List<CrewAgent> { myAgents.QuestionGenerationAgent };
        //    var agentTasks = new List<AgentTask> { Tasks.GenerateQuestions(myAgents.QuestionGenerationAgent, keyWord) };
        //    var crew = new Crew(agents, agentTasks);
        //    var runAsync = await crew.RunAsync();
        //}
    }
}