using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace easycodegenunity.Editor.Core
{
    public static class EasyQuery
    {
        public static List<EasyQueryResult> WithAttribute<T>()
        {
            return (from type in AppDomain.CurrentDomain.GetAssemblies()
                    from t in type.GetTypes()
                    where Attribute.IsDefined(t, typeof(T))
                    select new EasyQueryResult
                    {
                        Name = t.Name,
                        FullName = t.FullName,
                        Type = t,
                        Namespace = t.Namespace
                    })
                .ToList();
        }

        public static List<EasyQueryResult> WithInheritingFrom<T>()
        {
            throw new NotImplementedException();
        }

        public static List<EasyQueryResult> WithImplementing<T>()
        {
            throw new NotImplementedException();
        }

        public static List<EasyQueryResult> WithAttribute<T>(this EasyQueryResult resultClass)
        {
            return (from member in resultClass.Type.GetMembers(BindingFlags.Instance | BindingFlags.Public |
                                                               BindingFlags.NonPublic)
                where Attribute.IsDefined(member, typeof(T))
                select new EasyQueryResult
                {
                    Name = member.Name,
                    FullName = member.MemberType + "." + member.Name,
                    Type = member switch
                    {
                        PropertyInfo prop => prop.PropertyType,
                        FieldInfo field => field.FieldType,
                        MethodInfo method => method.ReturnType,
                        EventInfo eventInfo => eventInfo.EventHandlerType,
                        _ => null
                    },
                    Namespace = member.DeclaringType?.Namespace,
                    MemberInfo = member
                }).ToList();
        }

        public static List<EasyQueryResult> WithMembers(this EasyQueryResult resultClass)
        {
            throw new NotImplementedException();
        }

        public static List<EasyQueryResult> FromJson<T>(string path)
        {
            throw new NotImplementedException();
        }

        public static List<EasyQueryResult> FromScriptableObject<T>(string path)
        {
            throw new NotImplementedException();
        }

        public static List<EasyQueryResult> Where(Func<EasyQueryResult, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public static List<EasyQueryResult> OrderBy<TKey>(Func<EasyQueryResult, TKey> keySelector)
        {
            throw new NotImplementedException();
        }

        public static List<EasyQueryResult> Select(Func<EasyQueryResult, object> selector)
        {
            throw new NotImplementedException();
        }

        internal static string GetFriendlyTypeName(Type type)
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