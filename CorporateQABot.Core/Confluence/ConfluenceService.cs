using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CorporateQABot.Core.Confluence
{
    public class ConfluenceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _token;

        public ConfluenceService(string baseUrl, string token)
        {
            //_baseUrl = baseUrl.TrimEnd('/');
            //_token = token;
            //_httpClient = new HttpClient
            //{
            //    BaseAddress = new Uri(_baseUrl)
            //};
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> LoadPageContextAsync(string wikiUrl)
        {
            // Parse the URL to extract page ID and space key
            // var (pageId, spaceKey) = ParseConfluenceUrl(wikiUrl);

            // Step 1: Get the page HTML
            // var rawHtml = await GetPageHtmlAsync(pageId);

            // Step 2: Get the requirements from the magic URL
            // var requirements = await GetRequirementsFromMagicUrlAsync(spaceKey, pageId);

            #region Read From Files
            // Get the directory where this source file is located
            var serviceDirectory = GetSourceDirectory();
            var resourcesDirectory = Path.Combine(serviceDirectory, "Resources");

            // Read HTML from local file instead
            var htmlFilePath = Path.Combine(resourcesDirectory, "HtmlPage.html");
            var rawHtml = File.Exists(htmlFilePath) ? await File.ReadAllTextAsync(htmlFilePath) : string.Empty;

            // Read requirements from local JSON file instead
            Dictionary<string, RequirementInfo> requirements = await GetRequirementsFromFileAsync(resourcesDirectory); 
            #endregion

            // Step 3: Inject definitions into HTML
            var enrichedHtml = InjectDefinitionsIntoHtml(rawHtml, requirements);

            // Step 4: Convert to plain text for LLM
            var plainText = ConvertToPlainText(enrichedHtml, requirements);

            return plainText;
        }

        public async Task<string> GetPageHtmlAsync(string pageId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/rest/api/content/{pageId}?expand=body.storage");
                if (!response.IsSuccessStatusCode) return string.Empty;

                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                return json["body"]?["storage"]?["value"]?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting page HTML: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<Dictionary<string, RequirementInfo>> GetRequirementsFromMagicUrlAsync(string spaceKey, string pageId)
        {
            var result = new Dictionary<string, RequirementInfo>();
            var url = $"/rest/reqs/1/page/{spaceKey}/{pageId}?numberOfRequirements=2000";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                    var reqs = json["requirements"];

                    if (reqs != null)
                    {
                        foreach (var req in reqs)
                        {
                            var key = req["key"]?.ToString();
                            var excerpt = req["htmlExcerpt"]?.ToString();
                            var origin = req["origin"]?["title"]?.ToString();
                            var destinationUrl = req["destinationUrl"]?.ToString();
                            var status = req["status"]?.ToString();
                            var reqSpaceKey = req["spaceKey"]?.ToString();

                            if (!string.IsNullOrEmpty(key) && !result.ContainsKey(key))
                            {
                                var reqInfo = new RequirementInfo
                                {
                                    Key = key,
                                    Excerpt = excerpt ?? string.Empty,
                                    OriginTitle = origin ?? string.Empty,
                                    DestinationUrl = destinationUrl ?? string.Empty,
                                    Status = status ?? "ACTIVE",
                                    SpaceKey = reqSpaceKey ?? spaceKey
                                };

                                // Extract properties (e.g., @ActorNameAr, @ActorNameEn, @Description)
                                var properties = req["properties"];
                                if (properties != null)
                                {
                                    foreach (var prop in properties)
                                    {
                                        var propKey = prop["key"]?.ToString();
                                        var propValue = prop["value"]?.ToString();
                                        if (!string.IsNullOrEmpty(propKey) && !string.IsNullOrEmpty(propValue))
                                        {
                                            // Clean HTML from property value
                                            var cleanValue = Regex.Replace(propValue, "<[^>]+>", " ");
                                            cleanValue = Regex.Replace(cleanValue, @"\s+", " ").Trim();
                                            reqInfo.Properties[propKey] = cleanValue;
                                        }
                                    }
                                }

                                result[key] = reqInfo;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching requirements: {ex.Message}");
            }

            return result;
        }

        private (string pageId, string spaceKey) ParseConfluenceUrl(string url)
        {
            // Format 1: https://wiki.elm.sa/spaces/BJS/pages/248936913/UC-Cancel+Expert+Support+Request+...
            // This is the most common format users will provide
            var spacesMatch = Regex.Match(url, @"/spaces/([^/]+)/pages/(\d+)");
            if (spacesMatch.Success)
            {
                var spaceKey = spacesMatch.Groups[1].Value;
                var pageId = spacesMatch.Groups[2].Value;
                return (pageId, spaceKey);
            }

            // Format 2: https://wiki.elm.sa/pages/viewpage.action?pageId=248936913
            var pageIdMatch = Regex.Match(url, @"pageId=(\d+)");
            if (pageIdMatch.Success)
            {
                var pageId = pageIdMatch.Groups[1].Value;
                // Try to extract space key from URL or default
                var spaceMatch = Regex.Match(url, @"/display/([^/]+)/");
                var spaceKey = spaceMatch.Success ? spaceMatch.Groups[1].Value : "BJS";
                return (pageId, spaceKey);
            }

            // Format 3: https://wiki.elm.sa/display/BJS/Page+Title
            var displayMatch = Regex.Match(url, @"/display/([^/]+)/");
            if (displayMatch.Success)
            {
                // This format doesn't have pageId in URL, would need API lookup
                return ("", displayMatch.Groups[1].Value);
            }

            throw new ArgumentException($"Could not parse Confluence URL: {url}. Expected formats: /spaces/SPACE/pages/PAGEID/... or ?pageId=...");
        }

        private string InjectDefinitionsIntoHtml(string html, Dictionary<string, RequirementInfo> definitions)
        {
            try
            {
                Console.WriteLine($"Raw HTML length: {html?.Length ?? 0}");
                
                if (string.IsNullOrWhiteSpace(html))
                {
                    Console.WriteLine("HTML is empty or null");
                    return html ?? string.Empty;
                }

                // Load HTML using HtmlAgilityPack (handles malformed HTML gracefully)
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Find all ac:structured-macro elements with ac:name="requirement"
                // We need to look for nodes where the name contains "structured-macro"
                var allNodes = htmlDoc.DocumentNode.Descendants().ToList();
                Console.WriteLine($"Total nodes in document: {allNodes.Count}");

                var requirementMacros = allNodes
                    .Where(n => n.Name.Contains("ac:structured-macro") &&
                                n.Attributes.Any(a => a.Name == "ac:name" && a.Value == "\\\"requirement\\\""))
                    .ToList();

                // If still not found, try a more generic approach
                if (!requirementMacros.Any())
                {
                    Console.WriteLine("No macros found with first approach, trying alternative...");
                    requirementMacros = allNodes
                        .Where(n => n.OuterHtml.Contains("ac:name=\"requirement\"") ||
                                   n.OuterHtml.Contains("ac:name=\\\"requirement\\\""))
                        .ToList();
                }

                if (requirementMacros != null && requirementMacros.Any())
                {
                    Console.WriteLine($"Found {requirementMacros.Count} requirement macros");

                    foreach (var macro in requirementMacros)
                    {
                        Console.WriteLine($"Macro HTML: {macro.OuterHtml.Substring(0, Math.Min(200, macro.OuterHtml.Length))}...");
                        
                        // Find the key parameter - look for any node containing "key"
                        var allDescendants = macro.Descendants().ToList();
                        var keyParam = allDescendants
                            .FirstOrDefault(n => n.Name.Contains("ac:parameter") && 
                                                 n.Attributes.Any(a => a.Name == "ac:name" && a.Value == "\\\"key\\\""));
                        
                        if (keyParam != null)
                        {
                            var key = keyParam.InnerText?.Trim();
                            
                            if (!string.IsNullOrEmpty(key))
                            {
                                Console.WriteLine($"Processing requirement: {key}");
                                
                                // Look up the definition
                                if (definitions.TryGetValue(key, out var info))
                                {
                                    // Create a simple text definition to insert AFTER the macro
                                    var definitionHtml = new StringBuilder();
                                    definitionHtml.Append($"<span class='requirement-definition' style='color: #0066cc; font-style: italic;'>");
                                    definitionHtml.Append($" ({info.OriginTitle}");
                                    
                                    if (!string.IsNullOrEmpty(info.Excerpt))
                                    {
                                        // Clean the excerpt from HTML tags for simple display
                                        var cleanExcerpt = Regex.Replace(info.Excerpt, "<[^>]+>", " ");
                                        cleanExcerpt = Regex.Replace(cleanExcerpt, @"\s+", " ").Trim();
                                        
                                        // Limit length for readability
                                        //if (cleanExcerpt.Length > 150)
                                        //{
                                        //    cleanExcerpt = cleanExcerpt.Substring(0, 150) + "...";
                                        //}
                                        
                                        definitionHtml.Append($": {cleanExcerpt}");
                                    }
                                    
                                    definitionHtml.Append($")</span>");
                                    
                                    // Create a new node with the definition
                                    var definitionNode = HtmlNode.CreateNode(definitionHtml.ToString());
                                    
                                    // Insert the definition AFTER the macro (keeping the macro)
                                    macro.ParentNode.InsertAfter(definitionNode, macro);
                                    
                                    Console.WriteLine($"Added definition for requirement: {key}");
                                }
                                else
                                {
                                    Console.WriteLine($"Warning: No definition found for requirement {key}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No key parameter found in macro");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No requirement macros found in HTML");
                    
                    // Debug: Print first 500 chars to see the structure
                    Console.WriteLine($"HTML Preview: {html.Substring(0, Math.Min(500, html.Length))}");
                }

                // Get the enriched HTML
                var enrichedHtml = htmlDoc.DocumentNode.OuterHtml;

                // Save the enriched HTML to a file
                try
                {
                    var serviceDirectory = GetSourceDirectory();
                    var outputDirectory = Path.Combine(serviceDirectory, "ConfluenceOutput");
                    Directory.CreateDirectory(outputDirectory);
                    var htmlFilePath = Path.Combine(outputDirectory, $"enriched_page_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                    File.WriteAllText(htmlFilePath, enrichedHtml);
                    Console.WriteLine($"Enriched HTML saved to: {htmlFilePath}");
                }
                catch (Exception fileEx)
                {
                    Console.WriteLine($"Error saving HTML file: {fileEx.Message}");
                }

                return enrichedHtml;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InjectDefinitionsIntoHtml: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return html ?? string.Empty;
            }
        }

        private string ConvertToPlainText(string html, Dictionary<string, RequirementInfo> requirements)
        {
            // Decode HTML entities first
            var decodedHtml = System.Net.WebUtility.HtmlDecode(html);
            
            // Remove script and style tags completely
            decodedHtml = Regex.Replace(decodedHtml, @"<script[^>]*>[\s\S]*?</script>", " ", RegexOptions.IgnoreCase);
            decodedHtml = Regex.Replace(decodedHtml, @"<style[^>]*>[\s\S]*?</style>", " ", RegexOptions.IgnoreCase);
            
            // Replace structural HTML tags with appropriate spacing/newlines
            decodedHtml = Regex.Replace(decodedHtml, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
            decodedHtml = Regex.Replace(decodedHtml, @"</p>", "\n\n", RegexOptions.IgnoreCase);
            decodedHtml = Regex.Replace(decodedHtml, @"</h[1-6]>", "\n\n", RegexOptions.IgnoreCase);
            decodedHtml = Regex.Replace(decodedHtml, @"</li>", "\n", RegexOptions.IgnoreCase);
            decodedHtml = Regex.Replace(decodedHtml, @"</tr>", "\n", RegexOptions.IgnoreCase);
            decodedHtml = Regex.Replace(decodedHtml, @"</div>", "\n", RegexOptions.IgnoreCase);
            decodedHtml = Regex.Replace(decodedHtml, @"</td>", " | ", RegexOptions.IgnoreCase);
            decodedHtml = Regex.Replace(decodedHtml, @"</th>", " | ", RegexOptions.IgnoreCase);
            
            // Add extra spacing for requirement definitions
            decodedHtml = Regex.Replace(decodedHtml, @"<div class='requirement-definition'", "\n\n--- REQUIREMENT DEFINITION ---\n<div class='requirement-definition'", RegexOptions.IgnoreCase);
            
            // Remove all remaining HTML tags
            var text = Regex.Replace(decodedHtml, "<[^>]+>", " ");
            
            // Clean up whitespace while preserving intentional line breaks
            text = Regex.Replace(text, @"[ \t]+", " "); // Multiple spaces/tabs to single space
            text = Regex.Replace(text, @" *\n *", "\n"); // Trim spaces around newlines
            text = Regex.Replace(text, @"\n{3,}", "\n\n"); // Max 2 consecutive newlines
            text = text.Trim();

            // Save the plain text to a file
            try
            {
                var serviceDirectory = GetSourceDirectory();
                var outputDirectory = Path.Combine(serviceDirectory, "ConfluenceOutput");
                Directory.CreateDirectory(outputDirectory);
                var textFilePath = Path.Combine(outputDirectory, $"plain_text_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(textFilePath, text);
                Console.WriteLine($"Plain text saved to: {textFilePath}");
            }
            catch (Exception fileEx)
            {
                Console.WriteLine($"Error saving text file: {fileEx.Message}");
            }

            return text;
        }

        private static async Task<Dictionary<string, RequirementInfo>> GetRequirementsFromFileAsync(string resourcesDirectory)
        {
            var requirementsFilePath = Path.Combine(resourcesDirectory, "requirements.json");
            var requirements = new Dictionary<string, RequirementInfo>();

            if (File.Exists(requirementsFilePath))
            {
                var jsonContent = await File.ReadAllTextAsync(requirementsFilePath);
                var json = JObject.Parse(jsonContent);
                var reqs = json["requirements"];

                if (reqs != null)
                {
                    foreach (var req in reqs)
                    {
                        var key = req["key"]?.ToString();
                        var origin = req["origin"]?["title"]?.ToString();
                        var destinationUrl = req["destinationUrl"]?.ToString();
                        var status = req["status"]?.ToString();
                        var reqSpaceKey = req["spaceKey"]?.ToString();

                        if (!string.IsNullOrEmpty(key) && !requirements.ContainsKey(key))
                        {
                            var reqInfo = new RequirementInfo
                            {
                                Key = key,
                                OriginTitle = origin ?? string.Empty,
                                DestinationUrl = destinationUrl ?? string.Empty,
                                Status = status ?? "ACTIVE",
                                SpaceKey = reqSpaceKey ?? string.Empty
                            };

                            // Build a rich excerpt from indexation data
                            var excerptBuilder = new StringBuilder();

                            // Extract properties and build excerpt from indexation
                            var properties = req["properties"];
                            if (properties != null)
                            {
                                foreach (var prop in properties)
                                {
                                    var propKey = prop["key"]?.ToString();
                                    var propDataType = prop["dataType"]?.ToString();
                                    var propValue = prop["value"]?.ToString();
                                    var indexation = prop["indexation"];

                                    // Store the property value in Properties dictionary
                                    if (!string.IsNullOrEmpty(propKey) && !string.IsNullOrEmpty(propValue))
                                    {
                                        // Clean HTML from property value
                                        var cleanValue = Regex.Replace(propValue, "<[^>]+>", " ");
                                        cleanValue = Regex.Replace(cleanValue, @"\s+", " ").Trim();
                                        reqInfo.Properties[propKey] = cleanValue;
                                    }

                                    // Build excerpt from indexation data
                                    if (indexation != null)
                                    {
                                        // Handle multivalues (like Description with list items)
                                        var multivalues = indexation["multivalues"];
                                        if (multivalues != null && multivalues.HasValues)
                                        {
                                            excerptBuilder.AppendLine($"{propKey}:");
                                            foreach (var value in multivalues)
                                            {
                                                var valueText = value?.ToString()?.Trim();
                                                if (!string.IsNullOrEmpty(valueText))
                                                {
                                                    excerptBuilder.AppendLine($"  • {valueText}");
                                                }
                                            }
                                        }
                                        
                                        // Handle simple text (like ActorNameEn, ActorNameAr)
                                        var textValue = indexation["text"]?.ToString()?.Trim();
                                        if (!string.IsNullOrEmpty(textValue))
                                        {
                                            excerptBuilder.AppendLine($"{propKey}: {textValue}");
                                        }
                                    }
                                }
                            }

                            // Set the excerpt from built data or fallback to htmlExcerpt
                            var builtExcerpt = excerptBuilder.ToString().Trim();
                            if (!string.IsNullOrEmpty(builtExcerpt))
                            {
                                reqInfo.Excerpt = builtExcerpt;
                            }
                            else
                            {
                                // Fallback to htmlExcerpt if no indexation data found
                                var htmlExcerpt = req["htmlExcerpt"]?.ToString();
                                if (!string.IsNullOrEmpty(htmlExcerpt))
                                {
                                    var cleanExcerpt = Regex.Replace(htmlExcerpt, "<[^>]+>", " ");
                                    cleanExcerpt = Regex.Replace(cleanExcerpt, @"\s+", " ").Trim();
                                    reqInfo.Excerpt = cleanExcerpt;
                                }
                                else
                                {
                                    reqInfo.Excerpt = string.Empty;
                                }
                            }

                            requirements[key] = reqInfo;
                        }
                    }
                }
            }

            return requirements;
        }

        private static string GetSourceDirectory([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            return Path.GetDirectoryName(sourceFilePath) ?? Directory.GetCurrentDirectory();
        }
    }
}
