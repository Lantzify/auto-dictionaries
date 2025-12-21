using System.Text.RegularExpressions;

namespace AutoDictionaries.Helpers
{
	public class FindStaticContentHelper
	{
		public static List<string> ExtractStaticContent(string viewContent)
		{
			var results = new List<string>();
			var cleanedContent = RemoveRazorCode(viewContent);

			ExtractTextBetweenTags(cleanedContent, results);

			ExtractTranslatableAttributes(cleanedContent, results);

			return results;
		}

		public static void ExtractTextBetweenTags(string content, List<string> results)
		{
			var matches = Regex.Matches(content, @">([^<]+)<");

			foreach (Match match in matches)
				ProcessAndAddText(match.Groups[1].Value, results);
		}

		private static void ExtractTranslatableAttributes(string content, List<string> results)
		{
			var matches = Regex.Matches(content, @"\b(alt|title|placeholder|value|aria-label)\s*=\s*(['""])(.*?)\2", RegexOptions.IgnoreCase);
			foreach (Match match in matches)
			{
				var text = match.Groups[3].Value;
				if (!ContainsRazorOrCode(text))
					ProcessAndAddText(text, results);
			}
		}

		private static bool ContainsRazorOrCode(string text)
		{
			if (text.Contains('@'))
				return true;

			// Check for common code patterns
			if (Regex.IsMatch(text, @"\b(var|const|let|function|return|null|true|false|undefined)\b"))
				return true;

			if (Regex.IsMatch(text, @"^https?://|^/|^#|^\.\./"))
				return true;

			if (Regex.IsMatch(text, @"\.(cshtml|css|js|json|xml|html|png|jpg|gif|svg|ico)$", RegexOptions.IgnoreCase))
				return true;

			return false;
		}

		private static void ProcessAndAddText(string text, List<string> results)
		{
			// Decode HTML entities
			text = System.Net.WebUtility.HtmlDecode(text).Trim();

			// Skip if empty or whitespace only
			if (string.IsNullOrWhiteSpace(text))
				return;

			text = Regex.Replace(text, @"^[\s,;:!?\-–—]+", "").Trim();

			if (string.IsNullOrWhiteSpace(text))
				return;

			// Skip if too short (single character or less than 2 meaningful characters)
			if (text.Length < 2 || Regex.Replace(text, @"\W", "").Length < 2)
				return;

			// Skip if starts with non-word character (likely code or special content), but allow & for HTML entities
			var firstChar = text.TrimStart()[0];
			if (Regex.IsMatch(firstChar.ToString(), @"[\d]"))
				return;

			// Skip HTML entities that weren't decoded properly
			if (Regex.IsMatch(text, @"^&\w+;$"))
				return;

			// Skip C# code patterns - starts/ends with code characters
			if (Regex.IsMatch(text, @"^[@({>]|[;{}]$"))
				return;

			// Skip domain names
			if (Regex.IsMatch(text, @"^[a-zA-Z0-9][a-zA-Z0-9-]*(\.[a-zA-Z0-9][a-zA-Z0-9-]*)+$"))
				return;

			results.Add(text);
		}
		
		private static string RemoveRazorCode(string content)
		{
			content = Regex.Replace(content, @"(?:@)?(?:if|for|foreach|while|switch)\s*\((?>[^()]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\)", "");

			content = Regex.Replace(content, @"@(?:inherits|using|inject|model|addTagHelper|removeTagHelper|namespace|page)\s+[^\r\n]*", "");
			content = Regex.Replace(content, @"@await\s+[^<\r\n]+", "");
			content = Regex.Replace(content, @"@\([^)]+\).*\)", "");
			content = Regex.Replace(content, @"@Html\.[^\s<]+", "");
			content = Regex.Replace(content, @"@Umbraco\.GetDictionaryValue\([^)]+\)", "");

			content = Regex.Replace(content, @"@[a-zA-Z_][a-zA-Z0-9_]*(?:\.[a-zA-Z_][a-zA-Z0-9_]*(?:\([^)]*\))?)*", "");
			content = Regex.Replace(content, @"@:", "");

			content = Regex.Replace(content, @"<script[^>]*>.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
			content = Regex.Replace(content, @"<style[^>]*>.*?</style>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
			content = Regex.Replace(content, @"<!--.*?-->", "", RegexOptions.Singleline);

			return content;
		}
	}
}
