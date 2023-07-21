using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static SK.Connectors.Llama.TextCompletion.TextCompletionRequest;

namespace SK.Connectors.Llama.TextCompletion
{
    public class LlamaTextParser
    {
        public List<Chat> Parse(string input)
        {
            List<Chat> messages = new List<Chat>();

            // Define the regular expression pattern to match {{ role }} content
            string pattern = @"\{\{\s*(?<role>user|system)\s*\}\}\s*(?<content>.+?)(?=\{\{\s*(user|system)\s*\}\}|\z)";

            // Match the pattern in the input string
            MatchCollection matches = Regex.Matches(input, pattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                string role = match.Groups["role"].Value;
                string content = match.Groups["content"].Value.Trim();

                messages.Add(new Chat { Role = role, Content = content });
            }

            return messages;
        }
    }
}
