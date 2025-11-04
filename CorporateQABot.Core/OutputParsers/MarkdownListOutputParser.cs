using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Parses Markdown bullet lists (e.g., "- item") into plain string collections.
    /// </summary>
    public sealed class MarkdownListOutputParser : BaseOutputParser<IReadOnlyList<string>>
    {
        /// <inheritdoc/>
        public override Task<IReadOnlyList<string>> Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Task.FromResult((IReadOnlyList<string>)Array.Empty<string>());
            }

            IReadOnlyList<string> items = text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.TrimStart())
                .Where(line => line.StartsWith("- ") || line.StartsWith("* ") || line.StartsWith("+ "))
                .Select(line => line.Substring(2).Trim())
                .Where(line => line.Length > 0)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(items);
        }

        /// <inheritdoc/>
        public override string GetFormatInstructions() =>
            "Respond with a Markdown bullet list (e.g., '- item').";

        /// <inheritdoc/>
        protected override string Type() => "markdown_list";
    }
}
