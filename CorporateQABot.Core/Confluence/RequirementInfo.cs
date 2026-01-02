using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorporateQABot.Core.Confluence
{
    public class RequirementInfo
    {
        /// <summary>
        /// The unique key/code of the requirement (e.g., "CASE-EXP-STS-001", "ACT-006", "MSG-665").
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The HTML excerpt/definition of the requirement from the Magic URL.
        /// </summary>
        public string Excerpt { get; set; } = string.Empty;

        /// <summary>
        /// The title of the origin page where this requirement was defined.
        /// </summary>
        public string OriginTitle { get; set; } = string.Empty;

        /// <summary>
        /// The destination URL linking to the requirement definition.
        /// </summary>
        public string DestinationUrl { get; set; } = string.Empty;

        /// <summary>
        /// The requirement status (e.g., "ACTIVE", "DEPRECATED").
        /// </summary>
        public string Status { get; set; } = "ACTIVE";

        /// <summary>
        /// Space key where the requirement belongs.
        /// </summary>
        public string SpaceKey { get; set; } = string.Empty;

        /// <summary>
        /// Properties associated with the requirement (key-value pairs).
        /// Examples: @ActorNameAr, @ActorNameEn, @Description
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
