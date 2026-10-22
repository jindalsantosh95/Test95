using Microsoft.Azure.Functions.Worker;
using System.Collections;

namespace VAA.EAPI.Core.Functions
{
    public sealed class TestInvocationFeatures : IInvocationFeatures
    {
        private readonly Dictionary<Type, object?> items = new();

        public T? Get<T>() => (T?)items[typeof(T)];

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() => items.GetEnumerator();

        public void Set<T>(T instance)
        {
            if (items.ContainsKey(typeof(T)))
                items[typeof(T)] = instance;
            else
                items.Add(typeof(T), instance);
        }

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }
}
