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
   - Use Case ID (e.g., UC-353)
   - Title (both Arabic and English)
   - Description
   - Related JIRA tickets (e.g., BJS-5488)
   - Module/Component information

### 2. **Actors and Permissions**
   - Primary Actors (with reference IDs like ACT-008, ACT-006)
   - Actor names in Arabic and English
   - Actor capabilities and restrictions
   - Permission-based behavior differences

### 3. **Pre-Conditions**
   - All prerequisite conditions that must be met
   - Required system states
   - Dependent use cases (with UC-XXX references)
   - Excluded statuses (e.g., CASE-EXP-STS-011, CASE-EXP-STS-017)

### 4. **Post-Conditions**
   - Expected system state after completion
   - Data persistence requirements
   - Status changes

### 5. **Basic Flow (Step-by-Step)**
   - Extract EVERY step with its step number
   - User actions and system responses
   - Conditional logic (If/Else/Or scenarios)
   - UI elements to display (fields, buttons, dropdowns)
   - Field population rules (auto-populate, dimmed, editable)
   - System integrations/API calls (with integration guide references)
   - Status updates and transitions
   - Notification/messaging requirements (MSG-XXX references)
   - Template references (e.g., JDF-049)

### 6. **Alternative Flows**
   - All alternative flow IDs (A1, A2, etc.)
   - Trigger conditions for each alternative flow
   - Steps within each alternative flow
   - Return points to main flow

### 7. **Exception Flows**
   - All exception IDs (E1, E2, E3... E10)
   - Exception trigger conditions
   - Error messages (MSG-XXX references with Arabic text)
   - Retry logic (e.g., ""retry 10 times: 3 immediate + 7 exponential polling"")
   - Fallback behaviors
   - User feedback requirements

### 8. **Business Rules**
   - All BR-XXX references (e.g., BR-404, BR-405, BR-406, BR-407, BR-408)
   - Validation rules (required fields, length limits, conditional requirements)
   - Conditional logic rules
   - Display rules
   - Notification rules

### 9. **Field Specifications and Validations**
   - Field names (Arabic and English)
   - Field types (text, dropdown, checkbox, etc.)
   - Mandatory vs optional fields
   - Field length limits (e.g., max 500 characters)
   - Conditional visibility rules
   - Default values
   - Validation error messages

### 10. **API Integration Requirements**
   - Integration service name (e.g., ""Cancel Expert Request"")
   - Integration guide URL
   - Parameter mapping table fields:
     * Field Name
     * Field Source (Mapped Field / Static Value)
     * Mapped Value (source field/variable)
     * Static Value
     * Validations/Comments
   - Success response handling
   - Failure response handling
   - Status updates on success/failure

### 11. **UI/Wireframe Requirements**
   - Page/screen name
   - Layout description
   - Field arrangement
   - Button labels and actions
   - Conditional UI elements

### 12. **Status Management**
   - All status codes (e.g., CANCEL-CASE-EXP-STS-001, CANCEL-CASE-EXP-STS-002, CASE-EXP-STS-017)
   - Status transition rules
   - Actor responsible for each status change
   - Action type references (e.g., EXP-ACT-002)

### 13. **Notification Requirements**
   - Notification triggers
   - Recipient lists (including representatives, delegates, attorneys)
   - Message templates (MSG-XXX)
   - Delivery channels (SMS, Email, Portal)

### 14. **System Messages Catalog**
   - All MSG-XXX references
   - Message category (Error, Info, Confirmation, Warning)
   - Message text in Arabic
   - Display context

**Output Format Requirements**:

Your output MUST be structured as a clear, hierarchical document with:
- **Section headers** for each category above
- **Bullet points** for lists
- **Numbered steps** for flows
- **Tables** for parameter mappings (if applicable in text format)
- **Reference IDs preserved** exactly as they appear (UC-XXX, BR-XXX, MSG-XXX, ACT-XXX, etc.)
- **Bilingual content** clearly separated (Arabic | English)
- **Cross-references** highlighted (e.g., ""See Exception E4"", ""Triggers Alternative Flow A1""

**Critical Requirements**:
1. ✅ **Zero Information Loss**: Extract EVERY detail, even seemingly minor ones
2. ✅ **Preserve Traceability**: Keep all reference IDs intact (UC-XXX, BR-XXX, MSG-XXX, etc.)
3. ✅ **Maintain Arabic Text**: Do not translate or lose Arabic content
4. ✅ **Explicit Edge Cases**: List all exception scenarios with their exact conditions
5. ✅ **Implementation-Ready**: Structure output so developers know EXACTLY what to build
6. ✅ **Clear Dependencies**: Show how components connect (flows → exceptions, fields → validations)

**Quality Check**:
Before finalizing your output, verify:
- [ ] All step numbers from Basic Flow are captured (1, 2, 3, 4, 5, 6, 9, 10, 13, 15)
- [ ] All exception flows are documented (E1 through E10)
- [ ] All alternative flows are documented (A1)
- [ ] All business rules are captured (BR-404, BR-405, BR-406, BR-407, BR-408)
- [ ] All message references are preserved (MSG-665, MSG-358, MSG-660, MSG-664, MSG-006, MSG-116, MSG-661, MSG-662, MSG-663, MSG-159, MSG-058, MSG-146)
- [ ] All status codes are documented
- [ ] Parameter mapping table is complete
- [ ] API integration details are clear

**Expected Outcome**:
A comprehensive, structured requirements document that a developer or AI coding assistant can use to implement the ""Cancel Expert Support Request"" feature with 100% accuracy, covering all flows, validations, integrations, and edge cases without needing to refer back to the original HTML specification.
");
        }
    }
}
