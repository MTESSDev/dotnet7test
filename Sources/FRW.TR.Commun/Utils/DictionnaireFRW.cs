using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace FRW.TR.Commun.Utils
{
    /// <summary>
    /// Dictionnaire maison pour charger le JSON de FRW (permet de remplacer les clées qui contiennent des [0] et autres)
    /// </summary>
    public class DictionnaireFRW<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionnaire;

        public DictionnaireFRW()
        {
            _dictionnaire = new Dictionary<TKey, TValue>();
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return _dictionnaire[key];
            }
            set
            {
                var oKey = Regex.Replace(key.ToString() ?? string.Empty, @"\[.*?\]", "");
                var vKey = (TKey)Convert.ChangeType(oKey, typeof(TKey));

                _dictionnaire[vKey] = value;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _dictionnaire.Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => _dictionnaire.Values;

        int ICollection<KeyValuePair<TKey, TValue>>.Count => _dictionnaire.Count;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            _dictionnaire.Add(key, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionnaire.Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            _dictionnaire.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionnaire.Contains(item);
        }

        bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return _dictionnaire.ContainsKey(key);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _dictionnaire.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionnaire.GetEnumerator();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return _dictionnaire.Remove(key);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return _dictionnaire.Remove(item.Key);
        }

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            return _dictionnaire.TryGetValue(key, out value!);
        }
    }
}