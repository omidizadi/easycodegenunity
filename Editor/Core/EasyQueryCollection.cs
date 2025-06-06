using System;
using System.Collections.Generic;
using System.Linq;

namespace easycodegenunity.Editor.Core
{
    public class EasyQueryCollection : List<EasyQueryResult>
    {
        private readonly List<EasyQueryResult> _results;
        private IEnumerator<EasyQueryResult> _enumerator;

        public EasyQueryCollection(List<EasyQueryResult> results)
        {
            _results = results ?? new List<EasyQueryResult>();
        }

        public EasyQueryCollection WithAttribute<T>()
        {
            var results = _results.Where(result =>
                result.Type.GetCustomAttributes(typeof(T), false).Any()).ToList();
            return new EasyQueryCollection(results);
        }

        public EasyQueryCollection WithInheritingFrom<T>()
        {
            var type = typeof(T);
            var results = _results.Where(result =>
                type.IsAssignableFrom(result.Type) && result.Type != type).ToList();
            return new EasyQueryCollection(results);
        }

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

        public EasyQueryCollection Where(Func<EasyQueryResult, bool> predicate)
        {
            return new EasyQueryCollection(_results.Where(predicate).ToList());
        }

        public EasyQueryCollection OrderBy<TKey>(Func<EasyQueryResult, TKey> keySelector)
        {
            return new EasyQueryCollection(_results.OrderBy(keySelector).ToList());
        }

        public EasyQueryCollection Select(Func<EasyQueryResult, EasyQueryResult> selector)
        {
            return new EasyQueryCollection(_results.Select(selector).ToList());
        }

        public List<EasyQueryResult> ToList()
        {
            return new List<EasyQueryResult>(_results);
        }

        public new EasyQueryResult[] ToArray()
        {
            return _results.ToArray();
        }

        public new IEnumerator<EasyQueryResult> GetEnumerator()
        {
            return _results.GetEnumerator();
        }

        public new int Count => _results.Count;

        public new EasyQueryResult this[int index] => _results[index];
    }
}