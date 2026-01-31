using System.Text.RegularExpressions;

namespace AutoDictionaries.Helpers
{
	public class FindStaticContentHelper
	{
		// Pre-compiled static regexes (or use [GeneratedRegex] in .NET 7+)
		private static readonly Regex ControlStructuresRegex = new(
			@"(?:@)?(?:if|for|foreach|while|switch)\s*\((?>[^()]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\)",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex DirectivesRegex = new(
			@"@(?:inherits|using|inject|model|addTagHelper|removeTagHelper|namespace|page)\s+[^\r\n]*",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex AwaitRegex = new(
			@"@await\s+[^<\r\n]+",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex RazorInlineExpressionsRegex = new(
			@"@\((?>[^()]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\)",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex ObjectInitializersRegex = new(
			@"\bnew\s+\w+(?:\.\w+)*\s*\{(?>[^{}]+|\{(?<Depth>)|\}(?<-Depth>))*(?(Depth)(?!))\}",
			RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex HtmlHelpersRegex = new(
			@"@Html\.[a-zA-Z_][a-zA-Z0-9_]*\s*\((?>[^()]+|\((?<Depth>)|\)(?<-Depth>))*(?(Depth)(?!))\)",
			RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

		private static readonly Regex RazorPropertiesRegex = new(
			@"@[a-zA-Z_][a-zA-Z0-9_]*(?:\.[a-zA-Z_][a-zA-Z0-9_]*(?:\([^)]*\))?)*",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex ExplicitExpressionRegex = new(
			@"@:",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex ScriptTagRegex = new(
			@"<script[^>]*>.*?</script>",
			RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex StyleTagRegex = new(
			@"<style[^>]*>.*?</style>",
			RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex HtmlCommentRegex = new(
			@"<!--.*?-->",
			RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex TextBetweenTagsRegex = new(
			@">([^<]+)<",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex TranslatableAttributesRegex = new(
			@"\b(alt|title|placeholder|value|aria-label)\s*=\s*(['""])(.*?)\2",
			RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// ProcessAndAddText regexes
		private static readonly Regex TrimStartEndRegex = new(
			@"^(?:\s*\d+(?:[.,]\d+)?%?\s+|\s*(?:\?)?\.[a-zA-Z_][a-zA-Z0-9_]*\s*|[\s,;:!?\-()'""]+)|[\s()'.:-]+$",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex NonWordCharsRegex = new(
			@"\W",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex NumericOrDateTimeOnlyRegex = new(
			@"^\s*(?:[+-]?\d+(?:[.,]\d+)?|[+-]?\d{1,2}:\d{2}(?::\d{2})?(?:\s?(?:AM|PM|am|pm))?|\d{4}-\d{2}-\d{2})\s*$",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex HtmlEntityRegex = new(
			@"^&\w+;$",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex CSharpOperatorsRegex = new(
			@"(&&|\|\||=>|==|!=|<=|>=|\.HasValue\s*\(|\.Value(?:\s*(?:<[^>]+>)?\s*\()?)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex CodePatternsRegex = new(
			@"^[@({}>]|(new\s{|case\s\""|default:|break;)|[;{}]$",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex DomainNameRegex = new(
			@"^[a-zA-Z0-9][a-zA-Z0-9-]*(\.[a-zA-Z0-9][a-zA-Z0-9-]*)+$",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex CodeKeywordsRegex = new(
			@"\b(var|const|let|function|return|null|true|false|undefined)\b",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex UrlPatternRegex = new(
			@"^https?://|^/|^#|^\.\./",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private static readonly Regex FileExtensionRegex = new(
			@"\.(cshtml|css|js|json|xml|html|png|jpg|gif|svg|ico)$",
			RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
			var matches = TextBetweenTagsRegex.Matches(content);

			foreach (Match match in matches)
				ProcessAndAddText(match.Groups[1].Value, results);
		}

		private static void ExtractTranslatableAttributes(string content, List<string> results)
		{
			var matches = TranslatableAttributesRegex.Matches(content);
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
			if (CodeKeywordsRegex.IsMatch(text))
				return true;

			if (UrlPatternRegex.IsMatch(text))
				return true;

			if (FileExtensionRegex.IsMatch(text))
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

			text = TrimStartEndRegex.Replace(text, "").Trim();

			if (string.IsNullOrWhiteSpace(text))
				return;

			// Skip if too short (single character or less than 2 meaningful characters)
			if (text.Length < 2 || NonWordCharsRegex.Replace(text, "").Length < 2)
				return;

			// Skip if the content is only a number, date, or time.
			if (NumericOrDateTimeOnlyRegex.IsMatch(text))
				return;

			// Skip HTML entities that weren't decoded properly
			if (HtmlEntityRegex.IsMatch(text))
				return;

			// C# operators / lambdas / generics / method calls
			if (CSharpOperatorsRegex.IsMatch(text))
				return;

			// Skip C# code patterns - starts/ends with code characters
			if (CodePatternsRegex.IsMatch(text))
				return;

			// Skip domain names
			if (DomainNameRegex.IsMatch(text))
				return;

			results.Add(text);
		}
		
		private static string RemoveRazorCode(string content)
		{
			content = ControlStructuresRegex.Replace(content, "");

			content = DirectivesRegex.Replace(content, "");
			content = AwaitRegex.Replace(content, "");
			content = RazorInlineExpressionsRegex.Replace(content, "");
			content = ObjectInitializersRegex.Replace(content, "");
			content = HtmlHelpersRegex.Replace(content, "");
			content = RazorPropertiesRegex.Replace(content, "");
			content = ExplicitExpressionRegex.Replace(content, "");

			content = ScriptTagRegex.Replace(content, "");
			content = StyleTagRegex.Replace(content, "");
			content = HtmlCommentRegex.Replace(content, "");

			return content;
		}
	}
}
