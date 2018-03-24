using System.Collections.Generic;

namespace AmplitudeSharp.Utils
{
    static class CollectionExtensions
    {
        public static TValue TryGet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue @default = default(TValue))
        {
            TValue value;

            if (!dictionary.TryGetValue(key, out value))
            {
                value = @default;
            }

            return value;
        }
    }
}
