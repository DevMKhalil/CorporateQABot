using LangChain.Prompts;
using LangChain.Prompts.Base;
using LangChain.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateQABot.Core
{
    internal class Helper
    {
        /// <summary>
        /// Generates a composite prompt using a "few-shot" learning approach by formatting a base prompt template with multiple examples,
        /// followed by a suffix template formatted with specific input variables.
        /// </summary>
        /// <param name="template">The base prompt template string used for formatting each example.</param>
        /// <param name="suffixTemplate">The suffix template string appended at the end of the prompt, typically containing the final question or task.</param>
        /// <param name="suffixInputVariable">A dictionary containing the input variables to format the <paramref name="suffixTemplate"/>.</param>
        /// <param name="templateExamples">A collection of dictionaries, where each dictionary represents a single example's variables to format the base <paramref name="template"/>.</param>
        /// <returns>
        /// A single formatted prompt string that combines all formatted examples followed by the formatted suffix.
        /// </returns>
        /// <remarks>
        /// This method is commonly used in few-shot prompt engineering scenarios, where multiple examples are shown to guide the language model's output.
        /// </remarks>
        public static async Task<string> FewShotPrompt(
            string template,
            string suffixTemplate,
            Dictionary<string, object> suffixInputVariable,
            IEnumerable<Dictionary<string, object>> templateExamples)
        {
            var prompt = new StringBuilder();

            var baseTemplate = PromptTemplate.FromTemplate(template);

            foreach (var exampleSet in templateExamples)
            {
                var example = await baseTemplate.FormatAsync(new InputValues(exampleSet));
                prompt.AppendLine(example);
            }

            var sufTemplate = PromptTemplate.FromTemplate(suffixTemplate);

            var suffix = await sufTemplate.FormatAsync(new InputValues(suffixInputVariable));
            prompt.AppendLine(suffix);

            return prompt.ToString();
        }

        public static string ProjectRoot =>
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

        public static string GetDataChatsFilePath(string relativePath) =>
            Path.Combine(ProjectRoot, "Data", "Chats", relativePath);
    }
}
