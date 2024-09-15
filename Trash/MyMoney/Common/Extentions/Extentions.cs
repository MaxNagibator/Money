using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Extentions
{
    public static class CodeHelper
    {
        private static Random _random;

        public static T Random<T>(this IList<T> data)
        {
            if (data.Any())
            {
                if (_random == null)
                {
                    _random = new Random();
                }
                var randomPos = _random.Next(0, data.Count());
                return data[randomPos];
            }
            return data.FirstOrDefault();
        }

        public static string Aggregate<T>(this List<T> source, string delimeter = " / ")
        {
            if (source == null)
            {
                return "";
            }
            var s = source.Aggregate("", (current, i) => current + (delimeter + i));
            if (s.Length > delimeter.Length)
            {
                s = s.Substring(delimeter.Length);
            }
            return s;
        }

        public static Guid ToGuid(this string source)
        {
            return Guid.Parse(source);
        }

        public static Guid ToGuid(this string source, Guid defaultValue)
        {
            Guid i;
            if (Guid.TryParse(source, out i))
            {
                return i;
            }
            return defaultValue;
        }

        public static Guid? ToGuid(this string source, Guid? defaultValue)
        {
            Guid i;
            if (Guid.TryParse(source, out i))
            {
                return i;
            }
            return defaultValue;
        }

        public static Guid? ToNullableGuid(this string source)
        {
            if (String.IsNullOrEmpty(source))
            {
                return null;
            }
            return Guid.Parse(source);
        }

        public static bool ToBool(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            return Convert.ToBoolean(source);
        }

        public static bool ToBool(this bool? source)
        {
            if (source == null)
            {
                return false;
            }
            return (bool)source;
        }

        public static bool ToBool(this string source, bool defaultValue)
        {
            bool i;
            if (Boolean.TryParse(source, out i))
            {
                return i;
            }
            return defaultValue;
        }

        public static bool? ToBool(this string source, bool? defaultValue)
        {
            if (source == null)
            {
                return null;
            }
            bool i;
            if (Boolean.TryParse(source, out i))
            {
                return i;
            }
            return defaultValue;
        }

        public static int ToInt(this string source)
        {
            return Convert.ToInt32(source);
        }

        public static decimal ToDecimal(this string source)
        {
            return Convert.ToDecimal(source);
        }

        public static int ToInt(this string source, int defaultValue)
        {
            int i;
            if (Int32.TryParse(source, out i))
            {
                return i;
            }
            return defaultValue;
        }

        public static int? ToNullableInt(this string s)
        {
            if (s == null)
            {
                return null;
            }
            int i;
            if (Int32.TryParse(s, out i)) return i;
            return null;
        }

        public static int? ToInt(this string source, int? defaultValue)
        {
            int i;
            if (Int32.TryParse(source, out i))
            {
                return i;
            }
            return defaultValue;
        }

        public static decimal? ToNullableDecimal(this string s)
        {
            if (s == null)
            {
                return null;
            }
            decimal i;
            if (Decimal.TryParse(s, out i))
            {
                return i;
            }
            return null;
        }

        public static DateTime ToDateTime(this string source)
        {
            return DateTime.Parse(source);
        }

        public static DateTime? ToNullableDateTime(this string source, DateTime? defaultValue)
        {
            DateTime i;
            if (DateTime.TryParse(source, out i))
            {
                return i;
            }
            return defaultValue;
        }
        public static DateTime ToDateTime(this string source, DateTime defaultValue)
        {
            DateTime i;
            if (DateTime.TryParse(source, out i))
            {
                return i;
            }
            return defaultValue;
        }

        public static double? ToDouble(this string source, double? defaultValue)
        {
            double i;
            var parsingSource = source;
            if (!String.IsNullOrEmpty(parsingSource))
            {
                parsingSource = parsingSource.Replace(".", ",");
            }
            if (double.TryParse(parsingSource, out i))
            {
                return i;
            }
            return defaultValue;
        }

        public static string ToShortDateString(this DateTime? source)
        {
            if (source == null)
            {
                return null;
            }
            return ((DateTime)source).ToShortDateString();
        }

        public static string ToUnitTestValue(this DateTime date)
        {
            return date.ToString("yyyyMMddHHmmss");
        }

        public static string ToUnitTestValueWithRandom(this DateTime date, int randNumbers = 3)
        {
            if (_random == null)
            {
                _random = new Random();
            }
            var maxValue = (int)Math.Pow(10, randNumbers);
            var randomPos = _random.Next(0, maxValue).ToString();
            while (randomPos.Length < randNumbers)
            {
                randomPos = "0" + randomPos;
            }
            return date.ToString("yyyyMMddHHmmss") + randomPos;
        }

        public static string SurroundByBrackets(this string source)
        {
            return String.Format("[{0}]", source);
        }

        public static string ToLowerOrNull(this string source)
        {
            var x = String.IsNullOrEmpty(source) ? null : source.ToLower();
            return x;
        }

        public static string FirstUpper(this string source)
        {
            var x = String.IsNullOrEmpty(source) ? null : Char.ToUpper(source[0]) + source.Substring(1, source.Length - 1).ToLower();
            return x;
        }

        public static bool Between(this DateTime date, DateTime start, DateTime end)
        {
            return start.CompareTo(date) <= 0 && end.CompareTo(date) >= 0;
        }

        public static string ToUpper(this string value, int index = 0, int? length = null)
        {
            if (length == null || length > value.Length)
            {
                length = value.Length;
            }
            value = value.Substring(index, (int)length).ToUpper() + value.Substring((int)length);
            return value;
        }

        public static string TrimValue(this string value, char c = ' ')
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Trim(c);
        }

        public static string TrimStart(this string value, string c)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }
            if (value.StartsWith(c))
            {
                value = value.Substring(c.Length);
            }
            return value;
        }

        public static string TrimEnd(this string value, string c)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }
            if (value.EndsWith(c))
            {
                value = value.Substring(0, value.Length - c.Length);
            }
            return value;
        }

        public static string ToValueIfNullOrEmpty(this string value, string defaultValue = null)
        {
            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            return value;
        }

        public static string NameOf<T, TT>(this T obj, Expression<Func<T, TT>> propertyAccessor)
        {
            var expr = GetExpression(propertyAccessor.Body);
            return expr;
        }

        private static string GetExpression(Expression memberExpression)
        {
            if (memberExpression.NodeType == ExpressionType.MemberAccess)
            {
                var member = memberExpression as MemberExpression;
                if (member == null)
                {
                    return null;
                }
                var exp = "";
                if (member.Expression != null)
                {
                    exp = GetExpression(member.Expression);
                    if (exp == null)
                    {
                        exp = "";
                    }
                    else
                    {
                        exp += ".";
                    }
                }
                return exp + member.Member.Name;
            }
            return null;
        }

        public static DateTime GetNextWeekday(this DateTime start, DayOfWeek day)
        {
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }

    public static class UnixDateTimeConverter
    {
        private static readonly DateTime StartPoint = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public static readonly double SecondsPerDay = 86400;

        public static double GetUnixDate(DateTime dateTime)
        {
            return (dateTime - StartPoint).TotalSeconds;
        }

        public static double ToUnixDate(this DateTime dateTime)
        {
            return (dateTime - StartPoint).TotalSeconds;
        }

        public static double? GetUnixDate(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }
            return GetUnixDate((DateTime)dateTime);
        }

        public static double? ToUnixDate(this DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }
            return GetUnixDate((DateTime)dateTime);
        }

        public static DateTime GetDateTime(double dateTime)
        {

            return StartPoint.AddSeconds(dateTime);
        }

        public static DateTime ToDateTime(this double dateTime)
        {

            return StartPoint.AddSeconds(dateTime);
        }

        public static DateTime? GetDateTime(double? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }
            return GetDateTime((double)dateTime);
        }

        public static DateTime? ToDateTime(this double? dateTime)
        {
            if (dateTime == null)
            {
                return null;
            }
            return GetDateTime((double)dateTime);
        }
    }

    public static class DateTimeHelper
    {
        public static DateTime ChangeTimeZoneFromLocalTo(this DateTime value, double newTimeZoneToUtcShiftInMinutes)
        {
            var valueInUtc = TimeZoneInfo.ConvertTimeToUtc(value, TimeZoneInfo.Local);
            return valueInUtc.AddMinutes(-1 * newTimeZoneToUtcShiftInMinutes);
        }
    }
}