using System;
using System.Collections.Generic;
using System.Linq;

namespace nitwitapi.Logic
{
    public class ReplyLogic
    {
        public IEnumerable<string> GetRepliedUsernames(string content)
        {
            if (!content.StartsWith("@"))
                return new List<string>();

            var words = content.Split(" ");

            // Find the first "normal" word in the content. All @users before this word are replies.
            var indexOfNormalWord = Array.FindIndex(words, word => !word.StartsWith("@"));

            if (indexOfNormalWord == -1)
                return RemoveAtSign(words);

            var repliedUsernames = words.Take(indexOfNormalWord);

            return RemoveAtSign(repliedUsernames);
        }

        private IEnumerable<string> RemoveAtSign(IEnumerable<string> stuff)
        {
            return stuff.Select(username => username.Replace("@", string.Empty));
        }
    }
}
