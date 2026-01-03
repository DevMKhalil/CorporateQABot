using LangChain.Chains.StackableChains.Agents.Crew.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateQABot.Core.Confluence
{
    /// <summary>
    /// CrewAgentTool for loading and extracting content from Confluence Wiki pages.
    /// Implements the required ToolTask abstract method from CrewAgentTool base class.
    /// </summary>
    public class ConfluencePageLoaderTool : CrewAgentTool
    {
        private readonly ConfluenceService _confluenceService;

        public ConfluencePageLoaderTool() : base(
            name: "confluence_loader",
            description: "Loads and extracts business requirements from a Confluence Wiki page URL. " +
                         "Input should be the full Confluence Wiki URL. " +
                         "Returns the page content with all requirement definitions enriched and ready for analysis.")
        {
            _confluenceService = new ConfluenceService("https://wiki.elm.sa", "");
        }

        /// <summary>
        /// Implements the abstract ToolTask method from CrewAgentTool.
        /// This is called when the agent uses this tool.
        /// </summary>
        /// <param name="confluenceWikiURL">The Confluence Wiki URL to load</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The extracted page content with requirement definitions</returns>
        public override async Task<string> ToolTask(string confluenceWikiURL, CancellationToken cancellationToken = default)
        {
            try
            {
                // Clean the input URL
                var wikiUrl = confluenceWikiURL.Trim();

                if (string.IsNullOrWhiteSpace(wikiUrl))
                {
                    return "Error: No URL provided. Please provide a valid Confluence Wiki URL.";
                }

                // Load the context using the 2-step extraction
                var context = await _confluenceService.LoadPageContextAsync(wikiUrl);

                // Format the result for the agent
                var result = $@"# Wiki Page Information
## Full Page Content
{context}";

                return result;
            }
            catch (Exception ex)
            {
                return $"Error loading Wiki page: {ex.Message}\n\nPlease ensure the URL is a valid Confluence page URL with proper authentication.";
            }
        }
    }
}
