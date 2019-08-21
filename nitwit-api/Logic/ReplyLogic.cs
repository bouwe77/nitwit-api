using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace nitwitapi.Logic
{
    public class ReplyLogic
    {
        private static Regex _usernameRegex = new Regex(Constants.ValidUsernameRegexForCleaning, RegexOptions.Compiled);

        public IEnumerable<string> GetRepliedUsernames(string content)
        {
            if (!content.StartsWith("@"))
                return new List<string>();

            var words = content.Split(" ");

            // Find the first "normal" word in the content. All @users before this word are replies.
            var indexOfNormalWord = Array.FindIndex(words, word => !word.StartsWith("@"));

            // If there are no normal words, all words are replied usernames.
            if (indexOfNormalWord == -1)
                return words.KeepAlphanumericCharacters();

            var repliedUsernames = words.Take(indexOfNormalWord);

            return repliedUsernames.KeepAlphanumericCharacters();
        }
    }
}
