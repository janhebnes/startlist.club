using System.Linq;
using System.Web.Mvc;

namespace FlightJournal.Web.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Get the first line (i.e., up to but not including the first newline
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FirstLine(this string text)
        {
            var eol = text.IndexOfAny(new[] {'\n', '\r'});
            if (eol == -1)
                return text;
            return text.Substring(0, eol);
        }


        public static string RemoveNonAlphaNumPrefix(this string text)
        {
            var firstAlphaNum = text.IndexOf(text.FirstOrDefault(c => char.IsLetterOrDigit(c)));

            if (firstAlphaNum == -1)
                return text;
            return text.Substring(firstAlphaNum);
        }
    }
}