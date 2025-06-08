using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json;

namespace easycodegenunity.Editor.Core
{
    public static class EasyQuery
    {
        public static EasyQueryCollection WithAttribute<T>()
        {
            var results = (from type in AppDomain.CurrentDomain.GetAssemblies()
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

            return new EasyQueryCollection(results);
        }

        public static EasyQueryCollection WithInheritingFrom<T>()
        {
            var type = typeof(T);
            var results = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from t in assembly.GetTypes()
                    where type.IsAssignableFrom(t) && t != type
                    select new EasyQueryResult
                    {
                        Name = t.Name,
                        FullName = t.FullName,
                        Type = t,
                        Namespace = t.Namespace
                    })
                .ToList();

            return new EasyQueryCollection(results);
        }

        public static EasyQueryCollection WithImplementing<T>()
        {
            var interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"{interfaceType.Name} is not an interface");
            }

            var results = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from t in assembly.GetTypes()
                    where interfaceType.IsAssignableFrom(t) && t != interfaceType
                    select new EasyQueryResult
                    {
                        Name = t.Name,
                        FullName = t.FullName,
                        Type = t,
                        Namespace = t.Namespace
                    })
                .ToList();

            return new EasyQueryCollection(results);
        }

        public static List<EasyQueryResult> WithMembers<T>(this EasyQueryResult resultClass)
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
                }).ToList();
        }

        public static EasyQueryCollection FromResults(IEnumerable<EasyQueryResult> results)
        {
            return new EasyQueryCollection(results.ToList());
        }

        public static EasyQueryCollection Where(this IEnumerable<EasyQueryResult> results,
            Func<EasyQueryResult, bool> predicate)
        {
            return new EasyQueryCollection(results.Where(predicate).ToList());
        }

        public static EasyQueryCollection OrderBy<TKey>(this IEnumerable<EasyQueryResult> results,
            Func<EasyQueryResult, TKey> keySelector)
        {
            return new EasyQueryCollection(results.OrderBy(keySelector).ToList());
        }
    }
}