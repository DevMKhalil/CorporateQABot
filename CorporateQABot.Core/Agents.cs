using CorporateQABot.Core.Confluence;
using LangChain.Chains.StackableChains.Agents.Crew;
using LangChain.Chains.StackableChains.Agents.Crew.Tools;
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

            //PromptEngineerAgent = new CrewAgent(
            //    model: model,
            //    role: "Prompt Engineer Agent",
            //    goal: "Design and optimize prompts for AI models to improve response relevance and accuracy.",
            //    backstory: "I have a background in linguistics and AI, focusing on crafting prompts that elicit the best possible responses from language models."
            //);

            // Prompt Engineer Agent
            // Role: Creates professional prompts for AI IDEs
            PromptEngineerAgent = new CrewAgent(
                model: model,
                role: "AI Prompt Engineering Specialist",
                goal: "Transform business requirements into precise, professional prompts that can be used with AI IDEs, CLIs, or VS Code extensions to implement features with 100% accuracy.",
                backstory: @"You are an expert in LLM prompting and AI-assisted development.
You understand how AI coding assistants interpret prompts and can craft instructions that minimize ambiguity.
Your prompts are known for producing code that matches requirements exactly on the first try.
You structure prompts with clear context, specific requirements, and expected output formats."
            );

            // Requirements Analysis Agent
            // Role: Parses Confluence use case specifications and extracts structured, implementation-ready requirements
            RequirementsAnalysisAgent = new CrewAgent(
                model: model,
                role: "Technical Requirements Extraction Specialist",
                goal: "Parse Confluence use case specification documents using the specialized `confluence_loader` tool and extract complete, structured, and implementation-ready requirements with zero information loss, producing clear outputs that downstream AI agents can use to generate accurate code implementations.",
                backstory: @"You are a Senior Business Analyst and Technical Requirements Engineer with 15+ years of experience in enterprise software development.

**Core Expertise:**
- **Multilingual Requirements**: Handle bilingual content (Arabic/English) with perfect context preservation using the tool's structured output
- **Zero Manual Parsing**: The tool handles all HTML complexity, macro expansion, and cross-reference linking - you focus on structuring the enriched data
- **Extracting ALL requirement types**: functional flows, business rules, actors, preconditions, postconditions, UI wireframes, field validations, and system integrations
- **Identifying explicit AND implicit requirements that developers need to implement features completely**


**Tool-Assisted Workflow:**
1. **Fetch with Tool**: Use `confluence_loader` to retrieve the Confluence page with pre-enriched requirement definitions
3. **Trust Cross-References**: All requirement IDs (BR-XXX, MSG-XXX, ACT-XXX, STATUS-XXX) are preserved by the tool
4. **Structure & Validate**: Focus on organizing the enriched data into developer-ready documentation

**Your Specialization:**
- **Flow Analysis**: Extract Basic Flow, Alternative Flows (A1, A2...), and Exception Flows (E1, E2...) with step-by-step precision from tool-enriched content
- **Business Rules**: Capture all BR-XXX references, validation rules, and conditional logic
- **Integration Mapping**: Extract API parameter mappings, field mappings, and external system dependencies from pre-parsed tables
- **UI/UX Requirements**: Translate wireframe descriptions into actionable component specifications
- **Message Handling**: Document all error messages (MSG-XXX) with multilingual text from indexation metadata, confirmation dialogs, and notification requirements
- **Status Management**: Track state transitions and status codes (e.g.,CASE-EXP-STS-XXX, CANCEL-CASE-EXP-STS-XXX)

**Tool Advantages You Exploit:**
✅ **Clean Data Extraction**: Property values come with both raw HTML
✅ **Preserved References**: All UC-XXX, BR-XXX, MSG-XXX, ACT-XXX references are intact

**Your Output Quality Standards:**
- Zero ambiguity: every requirement is specific, measurable, and testable
- Complete traceability: preserve all reference IDs (UC-XXX, BR-XXX, MSG-XXX, etc.)
- Developer-ready: structure output so AI coding assistants can generate implementation code immediately
- Edge case coverage: explicitly list all exception scenarios and validation rules
- Contextual clarity: explain WHAT needs to be built, WHY it's needed, and HOW it connects to other components

You transform dense specification documents into crystal-clear, actionable development tasks that eliminate guesswork and ensure first-time-right implementations."
            );

            // Add Confluence Page Loader Tool to Requirements Analysis Agent
            RequirementsAnalysisAgent.AddTools(new List<CrewAgentTool>
            {
                new ConfluencePageLoaderTool()
            });

        }

        public CrewAgent KeyWordGenerationAgent { get; set; }
        public CrewAgent PromptEngineerAgent { get; set; }
        public CrewAgent RequirementsAnalysisAgent { get; set; }
    }
}
