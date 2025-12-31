using LangChain.Chains.StackableChains.Agents.Crew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateQABot.Core
{
    public class Tasks
    {
        public static AgentTask GenerateKeyWords(
        CrewAgent agent,
        string searchTerm)
        {
            return new AgentTask(
                agent: agent,
                description: $@"
                **Task**: Generate Top 10 Search Keywords
                **Description**: Analyze the provided search term and generate a list of the top 10 most relevant and effective keywords to use for internet search. Focus on identifying synonyms, related concepts, and variations that would improve search results and information discovery. Prioritize keywords by their potential impact on search quality.

                **Parameters**: 
                - Search Term: {searchTerm}

                **Note**: Provide exactly 10 keywords that maximize coverage and relevance for online search engines, ordered by importance.
            ");
        }
    }
}
