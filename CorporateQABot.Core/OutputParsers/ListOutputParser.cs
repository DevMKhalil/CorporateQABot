using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Parses a newline-separated list (optionally numbered or bulleted) from an LLM output.
    /// </summary>
    public sealed class ListOutputParser : BaseOutputParser<IReadOnlyList<string>>
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
                .Select(StripListDecorations)
                .Where(s => s.Length > 0)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(items);
        }

        /// <inheritdoc/>
        public override string GetFormatInstructions() =>
            "Respond with a newline separated list. Optional numbering or bullet characters are allowed.";

        /// <inheritdoc/>
        protected override string Type() => "list";

        private static string StripListDecorations(string value)
        {
            var trimmed = value.Trim();

            if (trimmed.Length == 0)
            {
                return trimmed;
            }

            // Remove leading bullets (e.g., '-', '*') or ordered list markers (e.g., '1.')
            if (trimmed[0] is '-' or '*' || (char.IsNumber(trimmed[0]) && trimmed.Length > 1 && trimmed[1] == '.'))
            {
                var index = trimmed.IndexOf(' ');
                if (index > -1)
                {
                    return trimmed.Substring(index + 1).Trim();
                }
            }

            return trimmed;
        }
    }
}
