using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CorporateQABot.Core.Confluence
{
    public class ConfluenceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _token;

        public ConfluenceService(string baseUrl, string token)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _token = token;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task LoadPageContextAsync(string wikiUrl)
        {
            // Parse the URL to extract page ID and space key
            var (pageId, spaceKey) = ParseConfluenceUrl(wikiUrl);

            // Step 1: Get the page HTML
            var rawHtml = await GetPageHtmlAsync(pageId);

            // Step 2: Get the requirements from the magic URL
            var requirements = await GetRequirementsFromMagicUrlAsync(spaceKey, pageId);

            // Step 3: Inject definitions into HTML
            var enrichedHtml = InjectDefinitionsIntoHtml(rawHtml, requirements);

            // Step 4: Convert to plain text for LLM
            var plainText = ConvertToPlainText(enrichedHtml, requirements);
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
                var wrappedHtml = $"<root xmlns:ac='http://www.atlassian.com/schema/confluence/4/ac/' xmlns:ri='http://www.atlassian.com/schema/confluence/4/ri/'>{html}</root>";
                var root = XElement.Parse(wrappedHtml);

                var macros = root.Descendants()
                    .Where(n => n.Name.LocalName == "structured-macro" &&
                            n.Attribute(XName.Get("name", "http://www.atlassian.com/schema/confluence/4/ac/"))?.Value == "requirement")
                    .ToList();

                foreach (var macro in macros)
                {
                    var keyParam = macro.Descendants()
                        .FirstOrDefault(p => p.Name.LocalName == "parameter" &&
                                            p.Attribute(XName.Get("name", "http://www.atlassian.com/schema/confluence/4/ac/"))?.Value == "key");

                    if (keyParam != null)
                    {
                        var key = keyParam.Value.Trim();
                        var keyBadge = new XElement("span",
                            new XAttribute("class", "key-badge"),
                            key
                        );

                        if (definitions.TryGetValue(key, out var info))
                        {
                            var infoBox = new XElement("div",
                                new XAttribute("class", "ai-context-box"),
                                new XElement("strong", $"{info.OriginTitle}: "),
                                new XElement("span", info.Excerpt)
                            );
                            macro.ReplaceWith(keyBadge, infoBox);
                        }
                        else
                        {
                            macro.ReplaceWith(keyBadge);
                        }
                    }
                }

                var reader = root.CreateReader();
                reader.MoveToContent();
                var result = reader.ReadInnerXml();

                // Save the enriched HTML to a file
                try
                {
                    var serviceDirectory = Path.GetDirectoryName(typeof(ConfluenceService).Assembly.Location) ?? Directory.GetCurrentDirectory();
                    var outputDirectory = Path.Combine(serviceDirectory, "ConfluenceOutput");
                    Directory.CreateDirectory(outputDirectory);
                    var htmlFilePath = Path.Combine(outputDirectory, $"enriched_page_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                    File.WriteAllText(htmlFilePath, result);
                    Console.WriteLine($"Enriched HTML saved to: {htmlFilePath}");
                }
                catch (Exception fileEx)
                {
                    Console.WriteLine($"Error saving HTML file: {fileEx.Message}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Injection Error: {ex.Message}");
                return html;
            }
        }

        private string ConvertToPlainText(string html, Dictionary<string, RequirementInfo> requirements)
        {
            // Simple HTML to text conversion for LLM consumption
            var text = Regex.Replace(html, "<[^>]+>", " ");
            text = Regex.Replace(text, @"\s+", " ").Trim();

            // Append requirements summary
            if (requirements.Any())
            {
                text += "\n\n=== REQUIREMENTS DEFINITIONS ===\n";
                foreach (var req in requirements)
                {
                    var cleanExcerpt = Regex.Replace(req.Value.Excerpt, "<[^>]+>", " ");
                    cleanExcerpt = Regex.Replace(cleanExcerpt, @"\s+", " ").Trim();
                    text += $"\n[{req.Key}] ({req.Value.OriginTitle}): {cleanExcerpt}";
                }
            }

            // Save the plain text to a file
            try
            {
                var serviceDirectory = Path.GetDirectoryName(typeof(ConfluenceService).Assembly.Location) ?? Directory.GetCurrentDirectory();
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
    }
}
