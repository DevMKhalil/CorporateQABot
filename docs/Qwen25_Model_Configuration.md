# Qwen 2.5 Model Configuration for ReAct Agent Workflows

## Overview

This document describes the configuration and usage of the **Qwen 2.5 4B Thinking model** for ReAct (Reasoning + Acting) agent workflows in the CorporateQABot project.

## Model Information

- **Model Name**: `qwen2.5:4b-thinking-2507-q4_K_M`
- **Model Type**: Qwen 2.5 Thinking variant
- **Model Size**: 4 billion parameters
- **Quantization**: Q4_K_M (4-bit quantization for optimal performance/quality balance)
- **Purpose**: Requirements extraction agent with superior ReAct pattern compliance

## Why Qwen 2.5 Over Gemma 2?

### Qwen 2.5 4B Thinking Advantages

| Feature | Gemma 2 (2B) | Qwen 2.5 (4B Thinking) |
|---------|--------------|------------------------|
| **ReAct Format Adherence** | ⚠️ Poor - tends to produce conversational text | ✅ Excellent - follows structured format |
| **Instruction Following** | ⚠️ Moderate | ✅ Superior |
| **Reasoning Quality** | ⚠️ Basic | ✅ Advanced (thinking-tuned) |
| **Multilingual (AR/EN)** | ⚠️ Limited | ✅ Excellent |
| **Tool Usage Reliability** | ❌ Unreliable | ✅ Consistent |
| **Context Window** | 8K tokens | ✅ 16-32K tokens |
| **Structured Output** | ⚠️ Inconsistent | ✅ Highly reliable |

### Key Benefits for Requirements Extraction

1. **Format Compliance**: Consistently follows `Thought: → Action: → Action Input: → Final Answer:` pattern
2. **Multilingual Excellence**: Handles Arabic/English requirements without loss
3. **Large Context**: 16K+ context window handles comprehensive requirement documents
4. **Reasoning Capabilities**: "Thinking" variant excels at step-by-step analysis
5. **Tool Reliability**: Correctly identifies when to use `confluence_loader` tool

## Configuration Details

### Model Settings (OllamaModelHelpers.cs)

```csharp
Temperature = 0.0f          // Maximum consistency - no randomness
TopP = 0.95f               // Slight diversity for reasoning
TopK = 40                  // Limit vocabulary for structured output
RepeatPenalty = 1.15f      // Prevent repetitive responses
NumPredict = 8192          // Max output tokens for comprehensive documents
NumCtx = 16384             // Large context window (supports up to 32K)
```

### Setting Explanations

- **Temperature 0.0**: Critical for ReAct compliance - ensures model follows format exactly
- **TopP 0.95**: Allows high-probability tokens while maintaining structure
- **TopK 40**: Restricts token selection to top candidates, improving coherence
- **RepeatPenalty 1.15**: Reduces likelihood of repetitive text in long outputs
- **NumPredict 8192**: Allows generation of complete 19-section requirements documents
- **NumCtx 16384**: Handles long prompts with comprehensive task descriptions

## Setup Instructions

### 1. Pull the Model from Ollama

```bash
ollama pull qwen2.5:4b-thinking-2507-q4_K_M
```

### 2. Verify Model Installation

```bash
ollama list
```

You should see:
```
NAME                                   ID              SIZE
qwen2.5:4b-thinking-2507-q4_K_M       abc123...       2.5GB
```

### 3. Test Model Locally (Optional)

```bash
ollama run qwen2.5:4b-thinking-2507-q4_K_M
```

Test ReAct format:
```
You are a helpful agent. You must respond in this format:
Thought: Your reasoning
Action: tool_name
Action Input: input_value

What would you do to search for information about cats?
```

Expected response should follow the format exactly.

### 4. Use in Code

The model is already configured in your project:

```csharp
var llm = OllamaModelHelpers.OllamaQwen25ThinkingModel.UseConsoleForDebug();
```

## Usage in Requirements Extraction

### Code Example

```csharp
public async Task Create_Agent_For_Generate_Prompts()
{
    // Use Qwen 2.5 Thinking model for ReAct compliance
    var llm = OllamaModelHelpers.OllamaQwen25ThinkingModel.UseConsoleForDebug();
    
    string confluenceWikiURL = "https://wiki.elm.sa/spaces/BJS/pages/248936913";
    
    var myAgents = new Agents(llm);
    var agents = new List<CrewAgent> { myAgents.RequirementsAnalysisAgent };
    var agentTasks = new List<AgentTask> 
    { 
        Tasks.AnalyzeRequirementsFromConfluence(myAgents.RequirementsAnalysisAgent, confluenceWikiURL) 
    };
    
    var crew = new Crew(agents, agentTasks);
    var result = await crew.RunAsync();
}
```

## Expected Behavior

### ✅ Correct ReAct Output Format

```
Thought: I need to load the Confluence page using the confluence_loader tool. The URL has been provided in my task description.
Action: confluence_loader
Action Input: https://wiki.elm.sa/spaces/BJS/pages/248936913

[Tool executes and returns content]

Thought: I have successfully loaded the Confluence page content. Now I will analyze and structure all the requirements according to the 19-section framework defined in my task.
Final Answer: [Complete 19-section requirements document...]
```

### ❌ Previous Gemma 2 Behavior (Fixed)

```
Do you want me to analyze the Confluence page and generate a structured requirements document based on that?

I can do this by following these steps:
1. Access the Confluence Page: You will need to provide me with the URL of the Confluence page...
```

## Troubleshooting

### Model Not Found Error

**Error**: `Could not find model 'qwen2.5:4b-thinking-2507-q4_K_M'`

**Solution**: Ensure model name matches exactly:
```bash
ollama pull qwen2.5:4b-thinking-2507-q4_K_M
```

### Still Getting Conversational Responses

**Issue**: Model responds conversationally instead of using ReAct format

**Solutions**:
1. Verify Temperature is set to 0.0
2. Check that agent backstory includes ReAct format instructions (already configured in `Agents.cs`)
3. Ensure task description emphasizes immediate tool execution (already configured in `Tasks.cs`)
4. Try increasing RepeatPenalty to 1.2

### Performance Issues

**Issue**: Model is slow on your hardware

**Solutions**:
1. Use Q4_K_M quantization (already configured) - good balance
2. Reduce NumPredict if output is too long
3. Consider smaller model: `qwen2.5:3b-instruct-q4_K_M`
4. Or use original Qwen 1.7B: `OllamaModelHelpers.OllamaQwenModel`

### Context Window Exceeded

**Issue**: Task description + tool output exceeds 16K tokens

**Solutions**:
1. Model supports up to 32K - increase NumCtx to 32768
2. Simplify task description (but current version is optimized)
3. Use Qwen 2.5 7B with full 32K context window

## Performance Metrics (Expected)

| Metric | Value |
|--------|-------|
| **Inference Speed (Q4_K_M)** | ~15-25 tokens/sec (on modern CPU) |
| **Memory Usage** | ~3-4 GB RAM |
| **First Token Latency** | ~1-2 seconds |
| **Full Document Generation** | ~2-5 minutes (for 8192 tokens) |

## Alternative Models

If Qwen 2.5 doesn't meet your needs, consider:

### Qwen 2.5 7B (More Powerful)

```csharp
// Add to OllamaModelHelpers.cs
public const string OllamaQwen25_7BModelName = "qwen2.5:7b-instruct-q4_K_M";

private static OllamaChatModel ollamaQwen25_7BModel = new OllamaChatModel(
    new OllamaProvider(), 
    OllamaQwen25_7BModelName)
{
    Settings = new OllamaChatSettings()
    {
        Temperature = 0.0f,
        TopP = 0.95f,
        RepeatPenalty = 1.15f,
        NumPredict = 8192,
        NumCtx = 32768  // Full 32K context
    }
};
```

### Llama 3.1 8B (Alternative)

```bash
ollama pull llama3.1:8b-instruct-q4_K_M
```

Good instruction following, but larger and slower than Qwen 2.5 4B.

## Monitoring and Debugging

### Console Output with Debug Mode

The `.UseConsoleForDebug()` extension enables real-time monitoring:

```
[14:23:45] System: You are a Technical Requirements Extraction Specialist...
[14:23:47] Assistant: Thought: I need to use the confluence_loader tool...
[14:23:47] Tool: confluence_loader called with: https://wiki.elm.sa/...
[14:24:15] Tool Result: # Wiki Page Information...
[14:24:17] Assistant: Thought: I have loaded the page content...
[14:26:32] Assistant: Final Answer: # Use Case Overview...
```

### Logging Recommendations

For production use, consider:
1. Logging tool calls and responses
2. Tracking token usage with `NumPredict` monitoring
3. Recording execution time for performance tuning
4. Saving Final Answer to file for review

## Best Practices

1. **Always use Temperature 0.0** for ReAct agents
2. **Monitor console output** during initial testing
3. **Verify model is pulled** before running code
4. **Test with simple tasks first** before complex requirements extraction
5. **Compare outputs** between models if uncertain
6. **Keep agent backstory updated** with latest ReAct format instructions
7. **Review generated documents** for completeness against 19-section checklist

## References

- **Qwen 2.5 Model Card**: https://ollama.com/library/qwen2.5
- **LangChain Documentation**: https://github.com/tryAGI/LangChain
- **ReAct Pattern Paper**: https://arxiv.org/abs/2210.03629
- **Ollama Provider Settings**: https://github.com/tryAGI/LangChain/tree/main/src/Providers/Ollama

## Version History

| Date | Version | Changes |
|------|---------|---------|
| 2025-01-XX | 1.0 | Initial Qwen 2.5 4B Thinking configuration |

---

**Last Updated**: January 2025  
**Maintained By**: CorporateQABot Development Team
