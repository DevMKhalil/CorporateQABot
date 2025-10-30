using LangChain.Prompts;
using LangChain.Prompts.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateQABot.Core
{
    internal class Helper
    {

    }

    public class BasePromptTemplateInput(
    IReadOnlyList<string> inputVariables,
    Dictionary<string, object>? partialVariables = null)
    : IBasePromptTemplateInput
    {

        public IReadOnlyList<string> InputVariables { get; private set; } = inputVariables;

        /// <inheritdoc/>
        public Dictionary<string, object> PartialVariables { get; private set; } = partialVariables ?? new();
    }
}
