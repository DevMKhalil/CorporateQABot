using System;
using System.Globalization;
using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Parses an ISO-8601 (or generally recognized) datetime string into a <see cref="DateTimeOffset"/>.
    /// </summary>
    public sealed class DatetimeOutputParser : BaseOutputParser<DateTimeOffset>
    {
        private readonly IFormatProvider _formatProvider;
        private readonly DateTimeStyles _styles;

        public DatetimeOutputParser(IFormatProvider? formatProvider = null, DateTimeStyles styles = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
        {
            _formatProvider = formatProvider ?? CultureInfo.InvariantCulture;
            _styles = styles;
        }

        /// <inheritdoc/>
        public override Task<DateTimeOffset> Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException("Cannot parse an empty response to a datetime value.");
            }

            if (DateTimeOffset.TryParse(text.Trim(), _formatProvider, _styles, out var value))
            {
                return Task.FromResult(value);
            }

            throw new FormatException($"Value '{text}' cannot be parsed as a datetime.");
        }

        /// <inheritdoc/>
        public override string GetFormatInstructions() => "Respond with an ISO-8601 datetime (e.g., 2024-03-15T10:30:00Z).";

        /// <inheritdoc/>
        protected override string Type() => "datetime";
    }
}
