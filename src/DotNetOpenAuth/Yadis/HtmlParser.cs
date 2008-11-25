//-----------------------------------------------------------------------
// <copyright file="HtmlParser.cs" company="Andrew Arnott, Scott Hanselman, Jason Alexander">
//     Copyright (c) Andrew Arnott, Scott Hanselman, Jason Alexander. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Yadis {
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Web.UI.HtmlControls;

	internal static class HtmlParser {
		private static readonly Regex attrRe = new Regex("\n# Must start with a sequence of word-characters, followed by an equals sign\n(?<attrname>(\\w|-)+)=\n\n# Then either a quoted or unquoted attribute\n(?:\n\n # Match everything that's between matching quote marks\n (?<qopen>[\"\\'])(?<attrval>.*?)\\k<qopen>\n|\n\n # If the value is not quoted, match up to whitespace\n (?<attrval>(?:[^\\s<>/]|/(?!>))+)\n)\n\n|\n\n(?<endtag>[<>])\n    ", Flags);
		private const RegexOptions Flags = (RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private const string TagExpr = "\n# Starts with the tag name at a word boundary, where the tag name is\n# not a namespace\n<{0}\\b(?!:)\n    \n# All of the stuff up to a \">\", hopefully attributes.\n(?<attrs>[^>]*?)\n    \n(?: # Match a short tag\n    />\n    \n|   # Match a full tag\n    >\n    \n    (?<contents>.*?)\n    \n    # Closed by\n    (?: # One of the specified close tags\n        </?{1}\\s*>\n    \n    # End of the string\n    |   \\Z\n    \n    )\n    \n)\n    ";
		private const string StartTagExpr = "\n# Starts with the tag name at a word boundary, where the tag name is\n# not a namespace\n<{0}\\b(?!:)\n    \n# All of the stuff up to a \">\", hopefully attributes.\n(?<attrs>[^>]*?)\n    \n(?: # Match a short tag\n    />\n    \n|   # Match a full tag\n    >\n    )\n    ";

		private static readonly Regex headRe = TagMatcher("head", new[] { "body" });
		private static readonly Regex htmlRe = TagMatcher("html", new string[0]);
		private static readonly Regex removedRe = new Regex(@"<!--.*?-->|<!\[CDATA\[.*?\]\]>|<script\b[^>]*>.*?</script>", Flags);

		public static IEnumerable<T> HeadTags<T>(string html) where T : HtmlControl, new() {
			html = removedRe.Replace(html, string.Empty);
			Match match = htmlRe.Match(html);
			string tagName = (new T()).TagName;
			if (match.Success) {
				Match match2 = headRe.Match(html, match.Index, match.Length);
				if (match2.Success) {
					string text = null;
					string text2 = null;
					Regex regex = StartTagMatcher(tagName);
					for (Match match3 = regex.Match(html, match2.Index, match2.Length); match3.Success; match3 = match3.NextMatch()) {
						int beginning = (match3.Index + tagName.Length) + 1;
						int length = (match3.Index + match3.Length) - beginning;
						Match match4 = attrRe.Match(html, beginning, length);
						var headTag = new T();
						while (match4.Success) {
							if (match4.Groups["endtag"].Success) {
								break;
							}
							text = match4.Groups["attrname"].Value;
							text2 = HttpUtility.HtmlDecode(match4.Groups["attrval"].Value);
							headTag.Attributes.Add(text, text2);
							match4 = match4.NextMatch();
						}
						yield return headTag;
					}
				}
			}
		}

		private static Regex TagMatcher(string tagName, params string[] closeTags) {
			string text2;
			if (closeTags.Length > 0) {
				StringBuilder builder = new StringBuilder();
				builder.AppendFormat("(?:{0}", tagName);
				int index = 0;
				string[] textArray = closeTags;
				int length = textArray.Length;
				while (index < length) {
					string text = textArray[index];
					index++;
					builder.AppendFormat("|{0}", text);
				}
				builder.Append(")");
				text2 = builder.ToString();
			} else {
				text2 = tagName;
			}
			return new Regex(string.Format(CultureInfo.InvariantCulture, TagExpr, tagName, text2), Flags);
		}

		private static Regex StartTagMatcher(string tag_name) {
			return new Regex(string.Format(CultureInfo.InvariantCulture, StartTagExpr, tag_name), Flags);
		}
	}
}
