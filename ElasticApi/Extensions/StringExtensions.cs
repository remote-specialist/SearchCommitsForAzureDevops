namespace ElasticApi.Extensions
{
    public static class StringExtensions
    {
        public static string ToCamelCase(this string s)
        {
            return char.ToLowerInvariant(s[0]) + s[1..];
        }
    }
}
