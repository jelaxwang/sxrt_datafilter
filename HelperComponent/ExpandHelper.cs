using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HelperComponent
{
    public static class ExpandHelper
    {
        public static bool IsIn(this string thisValue, params string[] collection)
        {
            return Array.IndexOf(collection, thisValue) >= 0;
        }

        public static bool IsInt32(this string thisValue)
        {
            int intValue = 0;
            return Int32.TryParse(thisValue, out intValue);
        }

        public static bool IsDecimal(this string thisValue)
        {
            decimal decimalValue = 0m;
            return decimal.TryParse(thisValue, out decimalValue);
        }

        public static string ReplaceString(this string thisValue, string oldStringPattern, string newString)
        {
            return Regex.Replace(thisValue, oldStringPattern, newString, RegexOptions.IgnoreCase);
        }

    }
}
