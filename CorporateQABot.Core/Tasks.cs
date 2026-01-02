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

        public static AgentTask AnalyzeRequirementsFromConfluence(
            CrewAgent agent,
            string htmlContent)
        {
            return new AgentTask(
                agent: agent,
                description: $@"
**Task**: Extract and Structure Complete Requirements from Confluence Use Case Specification

**Objective**: 
Analyze the provided Confluence HTML use case specification document and extract ALL requirements with complete accuracy, zero information loss, and perfect traceability. Transform the raw specification into a structured, developer-ready requirements document that can be directly used by AI coding assistants for implementation.

**Input Document Content**:
```html
{htmlContent}
```

**Your Analysis MUST Extract and Structure the Following Sections**:

### 1. **Use Case Overview**
   - Use Case ID (e.g., UC-XXX) - extract the actual ID from the document
   - Title (in all available languages - typically Arabic and English)
   - Description/Purpose of the use case
   - Related JIRA tickets or tracking IDs (e.g., BJS-XXXX)
   - Module/Component/Domain information
   - Use case type and category

### 2. **Actors and Permissions**
   - Primary Actors (with reference IDs like ACT-XXX)
   - Actor names in all available languages
   - Actor roles and responsibilities
   - Actor capabilities and permissions
   - Actor restrictions and limitations
   - Permission-based behavior differences
   - Secondary actors (if any)

### 3. **Pre-Conditions**
   - All prerequisite conditions that must be met before the use case can execute
   - Required system states
   - Dependent use cases (with UC-XXX references)
   - Required data or entities
   - Excluded statuses or states (with status code references)
   - User authentication/authorization requirements
   - Any other constraints

### 4. **Post-Conditions**
   - Expected system state after successful completion
   - Expected system state after failure
   - Data persistence requirements
   - Status changes and state transitions
   - Side effects on other systems or components

### 5. **Triggering Events**
   - Events or actions that initiate this use case
   - User actions
   - System events
   - Scheduled triggers
   - Integration triggers

### 6. **Basic Flow (Step-by-Step)**
   - Extract EVERY step with its step number
   - User actions at each step
   - System responses and behaviors
   - Conditional logic (If/Else/Or/Switch scenarios)
   - UI elements to display (pages, forms, fields, buttons, dropdowns, checkboxes, etc.)
   - Field population rules (auto-populate, dimmed/disabled, editable, read-only)
   - Data validation at each step
   - System integrations/API calls (with integration guide references or URLs)
   - Status updates and state transitions
   - Notification/messaging requirements (with message reference IDs)
   - Template references (document/report templates)
   - Navigation and flow control
   - Any included sub-use cases

### 7. **Alternative Flows**
   - All alternative flow IDs (A1, A2, A3, etc.)
   - Descriptive name/title for each alternative flow
   - Trigger conditions for each alternative flow
   - Steps within each alternative flow (numbered)
   - Decision points and branching logic
   - Return points to main flow or other flows
   - Final outcomes of each alternative flow

### 8. **Exception Flows**
   - All exception IDs (E1, E2, E3, E4, E5, E6, E7, E8, E9, E10, etc.)
   - Descriptive name/title for each exception
   - Exception trigger conditions (what causes this exception)
   - Error detection mechanism
   - Error messages (with MSG-XXX references and text in all languages)
   - Retry logic and policies (e.g., retry counts, exponential backoff)
   - Fallback behaviors and recovery mechanisms
   - User feedback and guidance
   - Logging and auditing requirements
   - Impact on system state

### 9. **Business Rules**
   - All BR-XXX or business rule references
   - Rule descriptions in all available languages
   - Validation rules (required fields, length limits, format requirements, range constraints)
   - Conditional logic rules (if X then Y)
   - Calculation rules
   - Display rules (show/hide, enable/disable)
   - Notification rules
   - Data transformation rules
   - Authorization rules
   - Priority and ordering rules

### 10. **Field Specifications and Validations**
   - Field names (in all available languages)
   - Field labels and placeholders
   - Field types (text, number, date, dropdown, checkbox, radio, file upload, etc.)
   - Mandatory vs optional fields
   - Field length limits (min/max characters)
   - Format requirements (regex, patterns)
   - Range constraints (min/max values, date ranges)
   - Conditional visibility rules (show/hide based on other fields)
   - Conditional editability (enable/disable based on conditions)
   - Default values and initial states
   - Dropdown/select options and data sources
   - Validation error messages (with MSG-XXX references)
   - Field dependencies and relationships
   - Field grouping and sections

### 11. **API Integration Requirements**
   - Integration service/API names
   - Integration guide URLs or documentation references
   - Endpoint details (if available)
   - Request/response formats
   - Parameter mapping tables with:
     * Field Name / Parameter Name
     * Field Source (Mapped Field, Static Value, Calculated, System Generated)
     * Mapped Value (source field/variable/expression)
     * Static Value (if applicable)
     * Data Type
     * Required/Optional
     * Validations/Comments/Constraints
   - Authentication/authorization requirements
   - Success response handling and expected outcomes
   - Failure response handling and error recovery
   - Status updates on success/failure
   - Timeout and retry policies
   - Data transformation requirements

### 12. **UI/Wireframe Requirements**
   - Page/screen names
   - Page layout and structure
   - Section organization
   - Field arrangement and grouping
   - Button labels, positions, and actions
   - Link labels and destinations
   - Tab organization
   - Modal/popup specifications
   - Conditional UI elements (show/hide rules)
   - Responsive design considerations
   - Accessibility requirements
   - Wireframe images or references (note their presence)

### 13. **Status Management and State Transitions**
   - All status codes (e.g., STATUS-XXX, CASE-EXP-STS-XXX, etc.)
   - Status names in all available languages
   - Status descriptions
   - Status transition rules (from status X to status Y)
   - Conditions for each transition
   - Actor responsible for each status change
   - Action type references (e.g., ACT-XXX)
   - System vs user-initiated transitions
   - Reversible vs irreversible transitions
   - Status-based permissions and visibility

### 14. **Notification Requirements**
   - Notification triggers (events that cause notifications)
   - Recipient determination rules
   - Recipient lists (users, roles, representatives, delegates, attorneys, stakeholders)
   - Message templates (with MSG-XXX references)
   - Message content (in all available languages)
   - Delivery channels (SMS, Email, Portal, Push, etc.)
   - Notification timing (immediate, scheduled, batched)
   - Notification priority
   - Retry policy for failed notifications

### 15. **System Messages Catalog**
   - All MSG-XXX or message references
   - Message category (Error, Info, Confirmation, Warning, Success)
   - Message severity/priority
   - Message text in all available languages
   - Display context (when/where the message appears)
   - Message type (modal, toast, inline, etc.)
   - User actions available in response to message
   - Message parameters/placeholders

### 16. **Data and Entity Requirements**
   - Entities involved in the use case
   - Data attributes and properties
   - Data relationships
   - Data persistence requirements
   - Data validation rules
   - Data sources
   - Data transformations

### 17. **Security and Authorization**
   - Access control requirements
   - Role-based permissions
   - Data sensitivity classifications
   - Audit logging requirements
   - Encryption requirements
   - Compliance requirements

### 18. **Performance and Non-Functional Requirements**
   - Response time requirements
   - Throughput requirements
   - Concurrency requirements
   - Scalability considerations
   - Availability requirements

### 19. **Related Use Cases and Dependencies**
   - Use cases that are included/extended by this use case
   - Use cases that this use case includes/extends
   - Shared components or services
   - Integration dependencies

**Output Format Requirements**:

Your output MUST be structured as a clear, hierarchical document with:
- **Section headers** for each category above (only include sections that have content in the input document)
- **Bullet points** for lists
- **Numbered steps** for flows (preserve original step numbers if present)
- **Tables** for parameter mappings, field specifications, and structured data (use markdown or text format)
- **Reference IDs preserved** exactly as they appear (UC-XXX, BR-XXX, MSG-XXX, ACT-XXX, STATUS-XXX, etc.)
- **Multilingual content** clearly separated and labeled (e.g., Arabic | English)
- **Cross-references** highlighted and explicit (e.g., ""See Exception E4"", ""Triggers Alternative Flow A1"", ""References BR-404"")
- **Inline notes** for clarifications or implicit requirements discovered during analysis

**Critical Requirements**:
1. ✅ **Zero Information Loss**: Extract EVERY detail, even seemingly minor ones like hints, notes, or inline comments
2. ✅ **Preserve Traceability**: Keep all reference IDs intact exactly as written (UC-XXX, BR-XXX, MSG-XXX, etc.)
3. ✅ **Maintain Multilingual Text**: Do not translate or lose content in any language (especially Arabic, English, or others)
4. ✅ **Explicit Edge Cases**: List all exception scenarios with their exact conditions and triggers
5. ✅ **Implementation-Ready**: Structure output so developers know EXACTLY what to build, including technical details
6. ✅ **Clear Dependencies**: Show how components connect (flows → exceptions, fields → validations, statuses → transitions)
7. ✅ **Complete Context**: Include background information, business context, and rationale where provided
8. ✅ **Actionable Details**: Every requirement should be specific enough to implement or test

**Intelligent Analysis Instructions**:
- **Identify implicit requirements**: If the specification implies a requirement without stating it explicitly, note it
- **Detect patterns**: Recognize common patterns (e.g., CRUD operations, approval workflows, status machines)
- **Flag ambiguities**: If something is unclear or contradictory, note it explicitly
- **Suggest clarifications**: If critical information seems missing, note what should be clarified
- **Preserve original numbering**: Keep step numbers, flow IDs, and reference IDs exactly as in source
- **Extract from tables**: Parse HTML tables completely, preserving structure and relationships
- **Handle nested structures**: Preserve hierarchical relationships in lists and tables

**Quality Validation Checklist**:
Before finalizing your output, systematically verify:
- [ ] All step numbers from Basic Flow are captured with their content
- [ ] All exception flows are documented with their IDs, triggers, and steps
- [ ] All alternative flows are documented with their IDs, triggers, and steps
- [ ] All business rules are captured with their BR-XXX IDs and complete descriptions
- [ ] All message references (MSG-XXX) are preserved with their text in all languages
- [ ] All status codes and transitions are documented
- [ ] All actors and their permissions are listed
- [ ] All pre-conditions and post-conditions are captured
- [ ] All field specifications including validations are complete
- [ ] Parameter mapping table is complete with all columns
- [ ] API integration details include success and failure handling
- [ ] UI/wireframe details are extracted
- [ ] Notification requirements specify recipients and channels
- [ ] Cross-references between sections are maintained
- [ ] No section of the original document is ignored or skipped

**Expected Outcome**:
A comprehensive, structured requirements document that a developer or AI coding assistant can use to implement the use case with 100% accuracy, covering all flows, validations, integrations, and edge cases without needing to refer back to the original HTML specification. The document should be self-contained, complete, and immediately actionable.
");
        }
    }
}
