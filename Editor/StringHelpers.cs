namespace easycodegenunity.Editor.Core
{
    /// <summary>
    /// Some string helper methods to modify strings in a more convenient way.
    /// </summary>
    public static class StringHelpers
    {
        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length < 2)
                return str;

            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        public static string ToPascalCase(this string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length < 2)
                return str;

            return char.ToUpperInvariant(str[0]) + str.Substring(1);
        }
    }
}