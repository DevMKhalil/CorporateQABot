using System;
using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Parses string outputs into an enum value using case-insensitive matching.
    /// </summary>
    public sealed class EnumOutputParser<TEnum> : BaseOutputParser<TEnum> where TEnum : struct, Enum
    {
        /// <inheritdoc/>
        public override Task<TEnum> Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException($"Cannot parse an empty response to {typeof(TEnum).Name}.");
            }

            if (Enum.TryParse(text.Trim(), ignoreCase: true, out TEnum value))
            {
                return Task.FromResult(value);
            }

            throw new FormatException($"Value '{text}' cannot be parsed as {typeof(TEnum).Name}.");
        }

        /// <inheritdoc/>
        public override string GetFormatInstructions()
        {
            var allowed = string.Join(", ", Enum.GetNames(typeof(TEnum)));
            return $"Respond with one of: {allowed}.";
        }

        /// <inheritdoc/>
        protected override string Type() => "enum";
    }
}
