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
        }

        public CrewAgent KeyWordGenerationAgent { get; set; }
        public CrewAgent PromptEngineerAgent { get; set; }
    }
}
