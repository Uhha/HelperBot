using System.Text;

namespace BotApi.Extensions
{
    /// <summary>
    /// Extension methods for string operations, including message splitting for Telegram.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Telegram's maximum message length is 4096 characters.
        /// </summary>
        private const int TelegramMaxMessageLength = 4096;

        /// <summary>
        /// Splits a long text into chunks that fit within Telegram's message limit.
        /// Each chunk after the first will be prefixed with "..." to indicate continuation.
        /// </summary>
        /// <param name="text">The text to split.</param>
        /// <param name="maxLength">Maximum length per message (default: 4096).</param>
        /// <returns>A list of strings, each within the maxLength limit.</returns>
        public static List<string> SplitIntoTelegramMessages(this string text, int maxLength = TelegramMaxMessageLength)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            if (text.Length <= maxLength)
                return new List<string> { text };

            var messages = new List<string>();
            var remainingText = text;
            var firstChunk = true;

            while (remainingText.Length > 0)
            {
                if (firstChunk)
                {
                    // First chunk: take up to maxLength characters
                    var endPos = Math.Min(maxLength, remainingText.Length);
                    var chunk = remainingText.Substring(0, endPos);
                    messages.Add(chunk);
                    remainingText = remainingText.Substring(endPos);
                    firstChunk = false;
                }
                else
                {
                    // Subsequent chunks: try to find a good break point (preferably at whitespace or punctuation)
                    var chunk = SplitAtBestBreakPoint(remainingText, maxLength);

                    // If we couldn't find a good break point, just take the first maxLength characters
                    if (chunk == null || chunk.Length >= remainingText.Length)
                    {
                        chunk = remainingText.Substring(0, Math.Min(maxLength, remainingText.Length));
                    }

                    messages.Add(chunk);
                    remainingText = remainingText.Substring(chunk.Length);
                }
            }

            return messages;
        }

        /// <summary>
        /// Finds the best break point within a text segment to split at.
        /// Prefers breaking at whitespace, punctuation, or sentence boundaries.
        /// </summary>
        private static string SplitAtBestBreakPoint(string text, int maxLength)
        {
            if (text.Length <= maxLength)
                return text;

            // Find the last good break point before maxLength
            var breakPoints = new List<int>();

            for (int i = maxLength - 1; i >= Math.Max(0, maxLength - 50); i--)
            {
                char c = text[i];
                if (IsGoodBreakPoint(c))
                {
                    breakPoints.Add(i);
                }
            }

            if (breakPoints.Count > 0)
            {
                // Take the last good break point
                var breakIndex = breakPoints[breakPoints.Count - 1];
                return text.Substring(0, breakIndex + 1);
            }

            // If no good break point found, return the full text (will be handled by caller)
            return null;
        }

        /// <summary>
        /// Determines if a character is a good place to split text.
        /// </summary>
        private static bool IsGoodBreakPoint(char c)
        {
            // Good break points: whitespace, punctuation, or end of sentence
            return char.IsWhiteSpace(c) || 
                   c == '.' || c == '!' || c == '?' || 
                   c == ';' || c == ':' || c == ',' ||
                   c == '-' || c == '(' || c == ')' ||
                   c == '[' || c == ']' || c == '{' || c == '}';
        }

        /// <summary>
        /// Joins multiple messages back into a single string.
        /// </summary>
        public static string JoinMessages(this List<string> messages, string separator = "\n\n")
        {
            if (messages == null || messages.Count == 0)
                return string.Empty;

            var result = new StringBuilder();
            for (int i = 0; i < messages.Count; i++)
            {
                result.Append(messages[i]);
                if (i < messages.Count - 1)
                    result.Append(separator);
            }

            return result.ToString();
        }
    }
}