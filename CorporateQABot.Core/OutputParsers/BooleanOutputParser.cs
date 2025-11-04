using System;
using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Parses boolean outputs from an LLM, mapping common affirmative/negative responses.
    /// </summary>
    public sealed class BooleanOutputParser : BaseOutputParser<bool>
    {
        private static readonly string[] TrueTokens = { "true", "t", "yes", "y", "1" };
        private static readonly string[] FalseTokens = { "false", "f", "no", "n", "0" };

        /// <inheritdoc/>
        public override Task<bool> Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException("Cannot parse an empty response to a boolean value.");
            }

            var normalized = text.Trim().ToLowerInvariant();

            if (Array.Exists(TrueTokens, token => token == normalized))
            {
                return Task.FromResult(true);
            }

            if (Array.Exists(FalseTokens, token => token == normalized))
            {
                return Task.FromResult(false);
            }

            throw new FormatException($"Value '{text}' cannot be parsed as a boolean.");
        }

        /// <inheritdoc/>
        public override string GetFormatInstructions() => "Respond with either `true` or `false`.";

        /// <inheritdoc/>
        protected override string Type() => "boolean";
    }
}
