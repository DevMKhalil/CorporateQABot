using System;
using System.Globalization;
using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Parses floating-point responses from an LLM.
    /// </summary>
    public sealed class FloatOutputParser : BaseOutputParser<double>
    {
        private readonly NumberStyles _styles;
        private readonly IFormatProvider _formatProvider;

        public FloatOutputParser(NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands, IFormatProvider? formatProvider = null)
        {
            _styles = styles;
            _formatProvider = formatProvider ?? CultureInfo.InvariantCulture;
        }

        /// <inheritdoc/>
        public override Task<double> Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException("Cannot parse an empty response to a floating point number.");
            }

            if (double.TryParse(text.Trim(), _styles, _formatProvider, out var result))
            {
                return Task.FromResult(result);
            }

            throw new FormatException($"Value '{text}' cannot be parsed as a floating point number.");
        }

        /// <inheritdoc/>
        public override string GetFormatInstructions() => "Respond with a numeric value (e.g., 42, 3.14).";

        /// <inheritdoc/>
        protected override string Type() => "float";
    }
}
