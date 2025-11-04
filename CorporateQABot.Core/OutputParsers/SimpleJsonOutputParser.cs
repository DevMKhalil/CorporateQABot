using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Parses JSON objects into a dictionary of <see cref="JsonElement"/> values without requiring a predefined schema.
    /// </summary>
    public sealed class SimpleJsonOutputParser : BaseOutputParser<IReadOnlyDictionary<string, JsonElement>>
    {
        private readonly JsonSerializerOptions _options;

        public SimpleJsonOutputParser(JsonSerializerOptions? options = null)
        {
            _options = options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
        }

        /// <inheritdoc/>
        public override Task<IReadOnlyDictionary<string, JsonElement>> Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException("Cannot parse an empty response to JSON.");
            }

            try
            {
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(text, _options);

                if (dictionary is null)
                {
                    throw new FormatException("The JSON output could not be parsed into a dictionary.");
                }

                return Task.FromResult((IReadOnlyDictionary<string, JsonElement>)dictionary);
            }
            catch (JsonException ex)
            {
                throw new FormatException("The response is not valid JSON.", ex);
            }
        }

        /// <inheritdoc/>
        public override string GetFormatInstructions() => "Respond with a flat JSON object (e.g., {\"key\": \"value\"}).";

        /// <inheritdoc/>
        protected override string Type() => "simple_json";
    }
}
