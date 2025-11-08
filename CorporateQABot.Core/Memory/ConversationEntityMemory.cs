using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LangChain.Memory;
using LangChain.Providers;
using LangChain.Prompts;
using LangChain.Schema;
using static LangChain.Chains.Chain;

namespace CorporateQABot.Core.Memory
{
    /// <summary>
    /// Conversation memory that keeps lightweight per-entity summaries, emulating LangChain's Python ConversationEntityMemory.
    /// </summary>
    public class ConversationEntityMemory : BaseChatMemory
    {
        /// <summary>
        /// Default prompt used to extract entity names from the latest conversation snippet.
        /// </summary>
        private const string DefaultEntityExtractionPrompt = @"
You are an entity extraction assistant.
Given the latest conversation snippet, list the specific entities (people, organizations, places, or concrete things) that are mentioned.
Return a comma separated list of entity names. If no entities are present, return the word NONE.

Conversation:
{context}

Entities:";

        /// <summary>
        /// Default prompt used to update the summary for a tracked entity.
        /// </summary>
        private const string DefaultEntitySummarizationPrompt = @"
You are keeping track of information about the entity below.
Update the running summary with any new information from the latest conversation snippet.
If there is no new information, return the prior summary unchanged.

Entity: {entity}
Prior summary: {summary}

Conversation:
{context}

Updated summary:";

        /// <summary>
        /// Model instance used for both entity extraction and summary generation chains.
        /// </summary>
        private readonly IChatModel _model;
        /// <summary>
        /// Mutable store mapping entity names to their running summaries.
        /// </summary>
        private readonly Dictionary<string, string> _entitySummaries = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Prompt template that renders the entity extraction instructions.
        /// </summary>
        private readonly PromptTemplate _entityExtractionTemplate;
        /// <summary>
        /// Prompt template that renders entity summary updates.
        /// </summary>
        private readonly PromptTemplate _entitySummarizationTemplate;

        /// <summary>
        /// Initializes the memory with a chat model and optional overrides for history and prompt templates.
        /// </summary>
        /// <param name="model">Language model used to extract entities and generate summaries.</param>
        /// <param name="chatHistory">Existing chat history instance; defaults to a new <see cref="ChatMessageHistory"/>.</param>
        /// <param name="entityExtractionTemplate">Custom template for entity extraction.</param>
        /// <param name="entitySummarizationTemplate">Custom template for summary maintenance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        public ConversationEntityMemory(
            IChatModel model,
            BaseChatMessageHistory? chatHistory = null,
            PromptTemplate? entityExtractionTemplate = null,
            PromptTemplate? entitySummarizationTemplate = null)
            : base(chatHistory ?? new ChatMessageHistory())
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _entityExtractionTemplate = entityExtractionTemplate ?? PromptTemplate.FromTemplate(DefaultEntityExtractionPrompt);
            _entitySummarizationTemplate = entitySummarizationTemplate ?? PromptTemplate.FromTemplate(DefaultEntitySummarizationPrompt);
        }

        /// <summary>
        /// Formatter used to convert message collections into a single string for prompt context.
        /// </summary>
        public MessageFormatter Formatter { get; set; } = new MessageFormatter();

        /// <summary>
        /// Key name used for storing the textual conversation history inside memory variables.
        /// </summary>
        public string MemoryKey { get; set; } = "history";

        /// <summary>
        /// Key name used for storing the entity summary block inside memory variables.
        /// </summary>
        public string EntityStoreKey { get; set; } = "entities";

        /// <inheritdoc/>
        public override List<string> MemoryVariables => new() { MemoryKey, EntityStoreKey };

        /// <summary>
        /// Exposes the current set of tracked entities and their summaries.
        /// </summary>
        public IReadOnlyDictionary<string, string> EntitySummaries => _entitySummaries;

        /// <inheritdoc/>
        public override OutputValues LoadMemoryVariables(InputValues? inputValues)
        {
            var history = Formatter.Format(ChatHistory.Messages);
            var entityState = RenderEntityStore();

            return new OutputValues(new Dictionary<string, object>
            {
                { MemoryKey, history },
                { EntityStoreKey, entityState }
            });
        }

        /// <inheritdoc/>
        public override async Task SaveContext(InputValues inputValues, OutputValues outputValues)
        {
            await base.SaveContext(inputValues, outputValues).ConfigureAwait(false);

            var snippet = BuildLatestSnippet();
            if (string.IsNullOrWhiteSpace(snippet))
            {
                return;
            }

            var entities = await ExtractEntitiesAsync(snippet).ConfigureAwait(false);
            if (entities.Count == 0)
            {
                return;
            }

            foreach (var entity in entities)
            {
                var prior = _entitySummaries.TryGetValue(entity, out var summary) ? summary : "No known information.";
                var updated = await UpdateEntitySummaryAsync(entity, prior, snippet).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(updated))
                {
                    _entitySummaries[entity] = updated.Trim();
                }
            }
        }

        /// <inheritdoc/>
        public override async Task Clear()
        {
            await base.Clear().ConfigureAwait(false);
            _entitySummaries.Clear();
        }

        /// <summary>
        /// Builds a formatted snippet containing the most recent turns for entity extraction.
        /// </summary>
        private string BuildLatestSnippet()
        {
            if (ChatHistory.Messages.Count == 0)
            {
                return string.Empty;
            }

            // Use the last few turns (up to 4 messages) to give enough context for extraction.
            const int window = 4;
            var start = Math.Max(0, ChatHistory.Messages.Count - window);
            var recent = ChatHistory.Messages.Skip(start);

            return Formatter.Format(recent);
        }

        /// <summary>
        /// Invokes the extraction prompt to determine which entities were mentioned in the snippet.
        /// </summary>
        private async Task<IReadOnlyList<string>> ExtractEntitiesAsync(string context, CancellationToken cancellationToken = default)
        {
            var chain =
                Set(context, outputKey: "context")
                | Template(_entityExtractionTemplate.Template)
                | LLM(_model);

            var raw = await chain.RunAsync("text", cancellationToken: cancellationToken).ConfigureAwait(false);
            return ParseEntities(raw);
        }

        /// <summary>
        /// Updates or creates the summary for a single entity based on new conversation context.
        /// </summary>
        private async Task<string> UpdateEntitySummaryAsync(string entity, string priorSummary, string context, CancellationToken cancellationToken = default)
        {
            var chain =
                Set(entity, outputKey: "entity")
                | Set(priorSummary, outputKey: "summary")
                | Set(context, outputKey: "context")
                | Template(_entitySummarizationTemplate.Template)
                | LLM(_model);

            var updated = await chain.RunAsync("text", cancellationToken: cancellationToken).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(updated) ? priorSummary : updated;
        }

        /// <summary>
        /// Normalizes model output into a distinct list of entity names.
        /// </summary>
        private static IReadOnlyList<string> ParseEntities(string? response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return Array.Empty<string>();
            }

            var sanitized = response.Trim();
            if (sanitized.Equals("none", StringComparison.OrdinalIgnoreCase))
            {
                return Array.Empty<string>();
            }

            var split = sanitized
                .Split(new[] { ',', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => e.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return split;
        }

        /// <summary>
        /// Renders the entity summary dictionary into a human-readable block for downstream prompts.
        /// </summary>
        private string RenderEntityStore()
        {
            if (_entitySummaries.Count == 0)
            {
                return "No entities captured yet.";
            }

            var builder = new StringBuilder();
            foreach (var kvp in _entitySummaries.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                builder.AppendLine($"{kvp.Key}: {kvp.Value}");
            }

            return builder.ToString().TrimEnd();
        }
    }
}
