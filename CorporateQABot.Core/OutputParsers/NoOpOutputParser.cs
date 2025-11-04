using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Returns the model output without additional parsing; useful as a placeholder parser.
    /// </summary>
    public sealed class NoOpOutputParser : BaseOutputParser<string>
    {
        /// <inheritdoc/>
        public override Task<string> Parse(string? text) => Task.FromResult(text ?? string.Empty);

        /// <inheritdoc/>
        public override string GetFormatInstructions() => "No specific format required.";

        /// <inheritdoc/>
        protected override string Type() => "no_op";
    }
}
