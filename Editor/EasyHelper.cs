using System;
using Microsoft.CodeAnalysis.CSharp;

namespace easycodegenunity.Editor.Core
{
    public static class EasyHelper
    {
        internal static string GetFriendlyTypeName(this Type type)
        {
            if (type == null) return null;

            // Common type mappings
            if (type == typeof(void)) return "void";
            if (type == typeof(int)) return "int";
            if (type == typeof(long)) return "long";
            if (type == typeof(short)) return "short";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(double)) return "double";
            if (type == typeof(float)) return "float";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(char)) return "char";
            if (type == typeof(object)) return "object";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(sbyte)) return "sbyte";

            // For other types, use the standard name
            return type.Name;
        }
    }
}