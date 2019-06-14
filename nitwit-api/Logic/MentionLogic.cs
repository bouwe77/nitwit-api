using System;
using System.Collections.Generic;
using System.Linq;

namespace nitwitapi.Logic
{
    public class MentionLogic
    {
        public IEnumerable<string> GetMentionedUsernames(string content)
        {
            if (!content.Contains("@"))
                return new List<string>();

            var words = content.Split(" ");

            // Find the first "normal" word in the content. All @users after this word are mentions.
            var indexOfNormalWord = Array.FindIndex(words, word => !word.StartsWith("@"));

            if (indexOfNormalWord == -1)
                return new List<string>();

            var words2 = words.Skip(indexOfNormalWord);

            var mentionedUsernames = words2.Where(word => word.StartsWith("@"));

            return mentionedUsernames.Select(username => username.Replace("@", string.Empty));
        }
    }
}
