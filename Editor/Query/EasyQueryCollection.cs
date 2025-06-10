using System;
using System.Collections.Generic;
using System.Linq;

namespace easycodegenunity.Editor.Core
{
    /// <summary>
    /// Represents a collection of EasyQueryResult objects.
    /// </summary>
    public class EasyQueryCollection : List<EasyQueryResult>
    {
        private readonly List<EasyQueryResult> _results;
        private IEnumerator<EasyQueryResult> _enumerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyQueryCollection"/> class.
        /// </summary>
        /// <param name="results">The list of EasyQueryResult objects.</param>
        public EasyQueryCollection(List<EasyQueryResult> results)
        {
            _results = results ?? new List<EasyQueryResult>();
        }

        /// <summary>
        /// Filters the collection to include only results with a specific attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to search for.</typeparam>
        /// <returns>A new EasyQueryCollection containing the filtered results.</returns>
        public EasyQueryCollection WithAttribute<T>()
        {
            var results = _results.Where(result =>
                result.Type.GetCustomAttributes(typeof(T), false).Any()).ToList();
            return new EasyQueryCollection(results);
        }

        /// <summary>
        /// Filters the collection to include only results that inherit from a specific type.
        /// </summary>
        /// <typeparam name="T">The type to search for derived types.</typeparam>
        /// <returns>A new EasyQueryCollection containing the filtered results.</returns>
        public EasyQueryCollection WithInheritingFrom<T>()
        {
            var type = typeof(T);
            var results = _results.Where(result =>
                type.IsAssignableFrom(result.Type) && result.Type != type).ToList();
            return new EasyQueryCollection(results);
        }

        /// <summary>
        /// Filters the collection to include only results that implement a specific interface.
        /// </summary>
        /// <typeparam name="T">The interface type to search for implementing types.</typeparam>
        /// <returns>A new EasyQueryCollection containing the filtered results.</returns>
        /// <exception cref="ArgumentException">Thrown if T is not an interface.</exception>
        public EasyQueryCollection WithImplementing<T>()
        {
            var interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"{interfaceType.Name} is not an interface");
            }

            var results = _results.Where(result =>
                interfaceType.IsAssignableFrom(result.Type) && result.Type != interfaceType).ToList();
            return new EasyQueryCollection(results);
        }

        /// <summary>
        /// Filters the collection based on a predicate.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A new EasyQueryCollection containing the filtered results.</returns>
        public EasyQueryCollection Where(Func<EasyQueryResult, bool> predicate)
        {
            return new EasyQueryCollection(_results.Where(predicate).ToList());
        }

        /// <summary>
        /// Sorts the elements of the collection in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by keySelector.</typeparam>
        /// <param name="keySelector">A function to extract a key from an element.</param>
        /// <returns>A new EasyQueryCollection whose elements are sorted according to the key.</returns>
        public EasyQueryCollection OrderBy<TKey>(Func<EasyQueryResult, TKey> keySelector)
        {
            return new EasyQueryCollection(_results.OrderBy(keySelector).ToList());
        }

        /// <summary>
        /// Projects each element of the collection into a new form.
        /// </summary>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>A new EasyQueryCollection whose elements are the result of invoking the transform function on each element of the source.</returns>
        public EasyQueryCollection Select(Func<EasyQueryResult, EasyQueryResult> selector)
        {
            return new EasyQueryCollection(_results.Select(selector).ToList());
        }

        /// <summary>
        /// Converts the EasyQueryCollection to a List of EasyQueryResult objects.
        /// </summary>
        /// <returns>A new List containing the elements of the EasyQueryCollection.</returns>
        public List<EasyQueryResult> ToList()
        {
            return new List<EasyQueryResult>(_results);
        }

        /// <summary>
        /// Copies the elements of the EasyQueryCollection to a new array.
        /// </summary>
        /// <returns>An array containing copies of the elements of the EasyQueryCollection.</returns>
        public new EasyQueryResult[] ToArray()
        {
            return _results.ToArray();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the EasyQueryCollection.
        /// </summary>
        /// <returns>An IEnumerator for the EasyQueryCollection.</returns>
        public new IEnumerator<EasyQueryResult> GetEnumerator()
        {
            return _results.GetEnumerator();
        }

        /// <summary>
        /// Gets the number of elements in the EasyQueryCollection.
        /// </summary>
        public new int Count => _results.Count;

        /// <summary>
        /// Gets the element at the specified index in the EasyQueryCollection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        public new EasyQueryResult this[int index] => _results[index];
    }
}