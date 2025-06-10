using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace easycodegenunity.Editor.Core
{
    /// <summary>
    /// Provides static methods for querying types and members in the application domain.
    /// </summary>
    public static class EasyQuery
    {
        /// <summary>
        /// Finds all types in the application domain that have a specific attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to search for.</typeparam>
        /// <returns>An EasyQueryCollection containing the results.</returns>
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

        /// <summary>
        /// Finds all types in the application domain that inherit from a specific type.
        /// </summary>
        /// <typeparam name="T">The type to search for derived types.</typeparam>
        /// <returns>An EasyQueryCollection containing the results.</returns>
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

        /// <summary>
        /// Finds all types in the application domain that implement a specific interface.
        /// </summary>
        /// <typeparam name="T">The interface type to search for implementing types.</typeparam>
        /// <returns>An EasyQueryCollection containing the results.</returns>
        /// <exception cref="ArgumentException">Thrown if T is not an interface.</exception>
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

        /// <summary>
        /// Finds all members of a type that have a specific attribute.
        /// </summary>
        /// <param name="resultClass">The EasyQueryResult representing the type to search.</param>
        /// <typeparam name="T">The type of the attribute to search for.</typeparam>
        /// <returns>A list of EasyQueryResult objects representing the members found.</returns>
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

        /// <summary>
        /// Creates an EasyQueryCollection from a list of EasyQueryResult objects.
        /// </summary>
        /// <param name="results">The list of EasyQueryResult objects.</param>
        /// <returns>An EasyQueryCollection containing the results.</returns>
        public static EasyQueryCollection FromResults(IEnumerable<EasyQueryResult> results)
        {
            return new EasyQueryCollection(results.ToList());
        }

        /// <summary>
        /// Filters a sequence of EasyQueryResult objects based on a predicate.
        /// </summary>
        /// <param name="results">The sequence of EasyQueryResult objects.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An EasyQueryCollection containing the results that satisfy the predicate.</returns>
        public static EasyQueryCollection Where(this IEnumerable<EasyQueryResult> results,
            Func<EasyQueryResult, bool> predicate)
        {
            return new EasyQueryCollection(results.Where(predicate).ToList());
        }

        /// <summary>
        /// Sorts the elements of a sequence of EasyQueryResult objects in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <param name="results">A sequence of values to order.</param>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>An EasyQueryCollection whose elements are sorted according to the key.</returns>
        public static EasyQueryCollection OrderBy<TKey>(this IEnumerable<EasyQueryResult> results,
            Func<EasyQueryResult, TKey> keySelector)
        {
            return new EasyQueryCollection(results.OrderBy(keySelector).ToList());
        }
    }
}