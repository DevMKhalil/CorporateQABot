using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Parses an LLM output string into a comma‑separated list of strings.
    /// </summary>
    public class CommaSeparatedListOutputParser : BaseOutputParser<IReadOnlyList<string>>
    {
        /// <inheritdoc/>
        public override Task<IReadOnlyList<string>> Parse(string? text)
        {
            if (text is null)
            {
                return Task.FromResult((IReadOnlyList<string>)Array.Empty<string>());
            }

            // Split on commas
            IReadOnlyList<string> items = text
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .ToList()
                .AsReadOnly();

            return Task.FromResult(items);
        }

        /// <inheritdoc/>
        public override string GetFormatInstructions()
        {
            return "Your response should be a list of comma separated values, e.g.: `foo, bar, baz`";
        }

        /// <inheritdoc/>
        protected override string Type()
        {
            return "comma_separated_list";
        }
    }
}
