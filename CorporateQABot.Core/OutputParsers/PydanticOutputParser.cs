using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LangChain.Schema;

namespace CorporateQABot.Core.OutputParsers
{
    /// <summary>
    /// Parses an LLM response into a strongly-typed object using <see cref="JsonSerializer"/> and
    /// validates the result with .NET data annotations, similar to LangChain's Pydantic tools parser.
    /// </summary>
    /// <typeparam name="TModel">The model type that represents the expected JSON schema.</typeparam>
    public sealed class PydanticOutputParser<TModel> : BaseOutputParser<TModel> where TModel : class
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly string _formatInstructions;

        public PydanticOutputParser(JsonSerializerOptions? serializerOptions = null)
        {
            _serializerOptions = serializerOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNameCaseInsensitive = true
            };
            _formatInstructions = BuildFormatInstructions();
        }

        /// <inheritdoc/>
        public override Task<TModel> Parse(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException("Cannot parse an empty response to the requested schema.");
            }

            var normalized = StripMarkdownCodeFence(text);

            TModel? model;
            try
            {
                model = JsonSerializer.Deserialize<TModel>(normalized, _serializerOptions);
            }
            catch (JsonException ex)
            {
                throw new FormatException("The response is not valid JSON for the requested schema.", ex);
            }

            if (model is null)
            {
                throw new FormatException("The JSON could not be converted to the requested schema.");
            }

            Validate(model);
            return Task.FromResult(model);
        }

        /// <inheritdoc/>
        public override string GetFormatInstructions() => _formatInstructions;

        /// <inheritdoc/>
        protected override string Type() => "pydantic";

        private static void Validate(TModel model)
        {
            var validationContext = new ValidationContext(model);
            var results = new List<ValidationResult>();

            if (!Validator.TryValidateObject(model, validationContext, results, validateAllProperties: true))
            {
                var combined = string.Join("; ", results.Select(r => r.ErrorMessage ?? "Validation error"));
                throw new FormatException($"The JSON did not satisfy the schema constraints: {combined}");
            }
        }

        private static string BuildFormatInstructions()
        {
            var type = typeof(TModel);
            var sb = new StringBuilder();

            sb.AppendLine("The output should be formatted as a JSON instance that conforms to the schema below.");
            sb.AppendLine("For example, if the schema is {\"properties\": {\"foo\": {\"title\": \"Foo\", \"description\": \"a list of strings\", \"type\": \"array\", \"items\": {\"type\": \"string\"}}}, \"required\": [\"foo\"]},");
            sb.AppendLine("then {\"foo\": [\"bar\", \"baz\"]} is valid, while {\"properties\": {\"foo\": [\"bar\", \"baz\"]}} is not.");
            sb.AppendLine();
            sb.AppendLine("Respond with JSON that matches the following schema:");
            sb.AppendLine("{");

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToArray();

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var info = DescribeProperty(property);
                var suffix = i < properties.Length - 1 ? "," : string.Empty;
                sb.AppendLine($"  \"{property.Name}\": {info}{suffix}");
            }

            sb.Append('}');
            return sb.ToString();
        }

        private static string DescribeProperty(PropertyInfo propertyInfo)
        {
            var type = propertyInfo.PropertyType;
            var required = propertyInfo.GetCustomAttribute<RequiredAttribute>() is not null;
            var descriptionAttribute = propertyInfo.GetCustomAttribute<DisplayAttribute>();
            var description = descriptionAttribute?.Description ?? descriptionAttribute?.Name;

            var builder = new StringBuilder();
            builder.Append(type.Name);
            builder.Append(required ? " (required)" : " (optional)");

            if (!string.IsNullOrWhiteSpace(description))
            {
                builder.Append($": {description}");
            }

            var rangeAttribute = propertyInfo.GetCustomAttribute<RangeAttribute>();
            if (rangeAttribute is not null)
            {
                builder.Append($" Range[{rangeAttribute.Minimum}, {rangeAttribute.Maximum}]");
            }

            var stringLengthAttribute = propertyInfo.GetCustomAttribute<StringLengthAttribute>();
            if (stringLengthAttribute is not null)
            {
                builder.Append($" Length[{stringLengthAttribute.MinimumLength}, {stringLengthAttribute.MaximumLength}]");
            }

            return builder.ToString();
        }

        private static string StripMarkdownCodeFence(string text)
        {
            var trimmed = text.Trim();

            if (!trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                return trimmed;
            }

            // Remove the opening fence (e.g., ```json or ```).
            var firstLineEnd = trimmed.IndexOfAny(new[] { '\r', '\n' });
            if (firstLineEnd == -1)
            {
                return trimmed;
            }

            var withoutFence = trimmed[(firstLineEnd + 1)..];

            // Remove the closing fence if present.
            var closingFenceIndex = withoutFence.LastIndexOf("```", StringComparison.Ordinal);
            if (closingFenceIndex >= 0)
            {
                withoutFence = withoutFence[..closingFenceIndex];
            }

            return withoutFence.Trim();
        }
    }
}
