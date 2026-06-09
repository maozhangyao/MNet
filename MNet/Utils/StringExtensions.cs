using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MNet.Utils
{
    public static class StringExtensions
    {
        public static string Left(this string str, int len)
        {
            return str?.Substring(0, Math.Min(str.Length, len));
        }
        public static string Right(this string str, int len)
        {
            return str?.Substring(Math.Max(0, str.Length - len));
        }
    }
}
