using System;
using System.Text.Json;
using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Parses a JSON response into a strongly typed object by leveraging System.Text.Json.
    /// </summary>
    /// <typeparam name="T">Type to deserialize.</typeparam>
    public sealed class JsonOutputParser<T> : BaseOutputParser<T>
    {
        private readonly JsonSerializerOptions _options;
        private readonly string? _customFormatInstructions;

        public JsonOutputParser(JsonSerializerOptions? options = null, string? formatInstructions = null)
        {
            _options = options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
            _customFormatInstructions = formatInstructions;
        }

        /// <inheritdoc/>
        public override Task<T> Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException("Cannot parse an empty response to JSON.");
            }

            try
            {
                var result = JsonSerializer.Deserialize<T>(text, _options);

                if (result is null)
                {
                    throw new FormatException("The JSON output could not be deserialized into the requested type.");
                }

                return Task.FromResult(result);
            }
            catch (JsonException ex)
            {
                throw new FormatException("The response is not valid JSON.", ex);
            }
        }

        /// <inheritdoc/>
        public override string GetFormatInstructions() =>
            _customFormatInstructions ?? "Respond with valid JSON that matches the expected schema.";

        /// <inheritdoc/>
        protected override string Type() => "json";
    }
}
