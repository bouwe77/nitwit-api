using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace nitwitapi.Logic
{
    public static class Utils
    {
        private static Regex _usernameRegex = new Regex(Constants.ValidUsernameRegexForCleaning, RegexOptions.Compiled);

        public static IEnumerable<string> KeepAlphanumericCharacters(this IEnumerable<string> stuff)
        {
            return stuff.Select(username => _usernameRegex.Replace(username, string.Empty));
        }
    }
}
