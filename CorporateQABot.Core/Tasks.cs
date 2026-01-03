using CorporateQABot.Core.Confluence;
using LangChain.Chains.StackableChains.Agents.Crew;
using LangChain.Chains.StackableChains.Agents.Crew.Tools;
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

        public AgentTask AnalyzeRequirementsFromConfluence(
            CrewAgent agent,
            string confluenceWikiURL)
        {
            return new AgentTask(
                agent: agent,
                //tools: new List<CrewAgentTool> { new ConfluencePageLoaderTool() },
                description: $@"
═══════════════════════════════════════════════════════════════════════════════
🎯 IMMEDIATE ACTION REQUIRED
═══════════════════════════════════════════════════════════════════════════════

**Task**: Extract and Structure Complete Requirements from Confluence Use Case Specification

**Confluence Wiki URL (Use this NOW)**: {confluenceWikiURL}

**Step 1 - IMMEDIATE TOOL EXECUTION**:
Use the `confluence_loader` tool with the URL provided above. The tool will return enriched page content with all requirement definitions, cross-references, and structured data already parsed.

**Step 2 - COMPREHENSIVE REQUIREMENTS EXTRACTION**:
After receiving the tool output, structure ALL extracted data into a complete requirements document covering these 19 critical sections:

1. **Use Case Overview** - UC-XXX ID, title (all languages), description, JIRA tickets, module/domain
2. **Actors and Permissions** - Actor IDs (ACT-XXX), names (all languages), roles, capabilities, restrictions
3. **Pre-Conditions** - Prerequisites, system states, dependent use cases (UC-XXX), required data, excluded statuses
4. **Post-Conditions** - Success state, failure state, data persistence, status changes, side effects
5. **Triggering Events** - User actions, system events, scheduled triggers, integration triggers
6. **Basic Flow (Step-by-Step)** - Every step numbered, user actions, system responses, conditional logic, UI elements, field population rules, validations, integrations, status updates, notifications, templates, navigation
7. **Alternative Flows** - All A1, A2, A3... with triggers, steps, decision points, return points, outcomes
8. **Exception Flows** - All E1, E2, E3... with triggers, error detection, messages (MSG-XXX), retry logic, fallback behaviors, user feedback, logging
9. **Business Rules** - All BR-XXX references with descriptions (all languages), validation rules, conditional logic, calculations, display rules, notifications, transformations, authorization, priorities
10. **Field Specifications and Validations** - Names (all languages), labels, types, mandatory/optional, length limits, format requirements, ranges, conditional visibility, conditional editability, defaults, dropdown options, validation errors (MSG-XXX), dependencies, grouping
11. **API Integration Requirements** - Service names, integration guides, endpoints, request/response formats, parameter mapping tables (Field Name, Source, Mapped Value, Static Value, Data Type, Required/Optional, Validations), authentication, success handling, failure handling, status updates, timeouts, retry policies, data transformations
12. **UI/Wireframe Requirements** - Page names, layout, sections, field arrangement, buttons, links, tabs, modals, conditional UI, responsive design, accessibility, wireframe references
13. **Status Management and State Transitions** - All status codes (STATUS-XXX, CASE-EXP-STS-XXX, etc.), names (all languages), descriptions, transition rules, conditions, responsible actors, action types (ACT-XXX), system vs user-initiated, reversibility
14. **Notification Requirements** - Triggers, recipient determination, recipient lists, message templates (MSG-XXX), content (all languages), delivery channels, timing, priority, retry policies
15. **System Messages Catalog** - All MSG-XXX references, category, severity, text (all languages), display context, message type, user actions, parameters/placeholders
16. **Data and Entity Requirements** - Entities, attributes, relationships, persistence, validation rules, sources, transformations
17. **Security and Authorization** - Access control, role-based permissions, data sensitivity, audit logging, encryption, compliance
18. **Performance and Non-Functional Requirements** - Response time, throughput, concurrency, scalability, availability
19. **Related Use Cases and Dependencies** - Included/extended use cases, shared components, integration dependencies

═══════════════════════════════════════════════════════════════════════════════
✅ QUALITY VALIDATION CHECKLIST (Verify Before Completing)
═══════════════════════════════════════════════════════════════════════════════

Before providing your Final Answer, ensure:
☑ confluence_loader tool was called with the provided URL
☑ All numbered steps from Basic Flow are captured
☑ All exception flows (E1, E2, ...) documented with triggers and recovery steps
☑ All alternative flows (A1, A2, ...) documented with branching logic
☑ All business rules (BR-XXX) captured with complete descriptions
☑ All message references (MSG-XXX) preserved with text in all languages
☑ All status codes and transitions documented with conditions
☑ All actors (ACT-XXX) and permissions listed
☑ All pre-conditions and post-conditions captured
☑ All field specifications with complete validation rules
☑ Parameter mapping table complete with all required columns
☑ API integration includes success AND failure handling
☑ UI/wireframe details extracted
☑ Notification requirements specify recipients and channels
☑ Cross-references between sections maintained
☑ No section ignored or skipped
☑ Multilingual content (Arabic/English) preserved without translation
☑ All reference IDs (UC-XXX, BR-XXX, MSG-XXX, ACT-XXX, STATUS-XXX) intact

═══════════════════════════════════════════════════════════════════════════════
📊 OUTPUT FORMAT REQUIREMENTS
═══════════════════════════════════════════════════════════════════════════════

Structure your Final Answer as a hierarchical document with:
• **Section headers** for each of the 19 categories (include only sections with actual content)
• **Bullet points** for lists and itemized information
• **Numbered steps** for flows (preserve original step numbers)
• **Tables** in markdown format for parameter mappings, field specifications, and structured data
• **Reference IDs preserved** exactly as they appear (UC-XXX, BR-XXX, MSG-XXX, ACT-XXX, STATUS-XXX)
• **Multilingual content** clearly labeled (e.g., ""Arabic: X | English: Y"")
• **Cross-references** explicit and highlighted (e.g., ""See Exception E4"", ""Triggers Alternative Flow A1"")
• **Inline notes** for clarifications or implicit requirements

═══════════════════════════════════════════════════════════════════════════════
🎯 CRITICAL SUCCESS FACTORS
═══════════════════════════════════════════════════════════════════════════════

1. **Zero Information Loss**: Extract EVERY detail from the tool output, including hints, notes, inline comments
2. **Complete Traceability**: Keep all reference IDs intact exactly as written
3. **Multilingual Integrity**: Preserve all language variants without translation or loss
4. **Explicit Edge Cases**: List all exception scenarios with exact conditions and triggers
5. **Implementation-Ready**: Developers should know EXACTLY what to build from your output
6. **Clear Dependencies**: Show how components connect (flows → exceptions, fields → validations, statuses → transitions)
7. **Contextual Completeness**: Include background information, business context, and rationale where provided
8. **Actionable Precision**: Every requirement specific enough to implement or test

**Expected Outcome**: A self-contained, comprehensive requirements document that enables 100% accurate implementation without referring back to the Confluence page.
");
        }
    }
}
