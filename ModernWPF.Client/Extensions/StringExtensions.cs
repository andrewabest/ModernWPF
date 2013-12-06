using System;
using System.Globalization;

namespace ModernWPF.Client.Extensions
{
    public static class StringExtensions
    {
        public static string FormatWith(this string str, params object[] values)
        {
            return String.Format(CultureInfo.CurrentUICulture, str, values);
        }
    }
}