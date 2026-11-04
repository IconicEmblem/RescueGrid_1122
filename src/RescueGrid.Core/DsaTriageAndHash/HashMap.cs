using System;
using System.Collections.Generic;

namespace RescueGrid.Core.DsaTriageAndHash
{
    /// <summary>
    /// Custom hash table built from scratch (no Dictionary).
    /// Used for the active unit registry and ticket-id -> heap-index mapping.
    /// </summary>
    public class HashMap<K, V>
    {
        private const int InitialCapacity = 16;
        private const double LoadFactorThreshold = 0.75;

        private HashEntry<K, V>[] buckets;
        private int count;

        public HashMap()
        {
            buckets = new HashEntry<K, V>[InitialCapacity];
        }

        public int Count => count;

        private static int IndexFor(K key, int length)
        {
            // mask the sign bit so the index is never negative
            return (key.GetHashCode() & 0x7FFFFFFF) % length;
        }

        /// <summary>Insert a key/value pair, or overwrite the value if the key exists.</summary>
        public void Put(K key, V value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            int i = IndexFor(key, buckets.Length);
            for (var e = buckets[i]; e != null; e = e.Next)
            {
                if (e.Key.Equals(key))
                {
                    e.Value = value;
                    return;
                }
            }
            buckets[i] = new HashEntry<K, V>(key, value, buckets[i]);
            count++;

            if (count > buckets.Length * LoadFactorThreshold)
                Resize();
        }

        /// <summary>Get the value for a key. Throws if the key is missing.</summary>
        public V Get(K key)
        {
            if (TryGet(key, out V value)) return value;
            throw new KeyNotFoundException("Key not found: " + key);
        }

        public bool TryGet(K key, out V value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            int i = IndexFor(key, buckets.Length);
            for (var e = buckets[i]; e != null; e = e.Next)
            {
                if (e.Key.Equals(key))
                {
                    value = e.Value;
                    return true;
                }
            }
            value = default(V);
            return false;
        }

        public bool ContainsKey(K key)
        {
            return TryGet(key, out _);
        }

        /// <summary>Delete a key. Returns true if it existed.</summary>
        public bool Remove(K key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            int i = IndexFor(key, buckets.Length);
            HashEntry<K, V> prev = null;
            for (var e = buckets[i]; e != null; prev = e, e = e.Next)
            {
                if (e.Key.Equals(key))
                {
                    if (prev == null) buckets[i] = e.Next;
                    else prev.Next = e.Next;
                    count--;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Double the table size and re-hash every entry.</summary>
        private void Resize()
        {
            var old = buckets;
            buckets = new HashEntry<K, V>[old.Length * 2];

            foreach (var head in old)
            {
                var e = head;
                while (e != null)
                {
                    var next = e.Next;
                    int i = IndexFor(e.Key, buckets.Length);
                    e.Next = buckets[i];
                    buckets[i] = e;
                    e = next;
                }
            }
        }
    }
}
