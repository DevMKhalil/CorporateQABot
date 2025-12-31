using LangChain.Chains.StackableChains.Agents.Crew;
using LangChain.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateQABot.Core
{
    internal class Agents
    {
        public Agents(IChatModel model)
        {
            KeyWordGenerationAgent = new CrewAgent(
                model: model,
                role: "Keyword Generation Agent",
                goal: "Generate relevant and effective keywords for internet search based on user queries.",
                backstory: "I am an expert in search engine optimization and information retrieval, specializing in identifying the most impactful keywords to improve search results and information discovery."
            );
        }

        public CrewAgent KeyWordGenerationAgent { get; set; }
    }
}
