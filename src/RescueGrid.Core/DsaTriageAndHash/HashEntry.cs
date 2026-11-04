namespace RescueGrid.Core.DsaTriageAndHash
{
    /// <summary>One node in a HashMap bucket chain (separate chaining).</summary>
    public class HashEntry<K, V>
    {
        public K Key { get; }
        public V Value { get; set; }
        public HashEntry<K, V> Next { get; set; }

        public HashEntry(K key, V value, HashEntry<K, V> next = null)
        {
            Key = key;
            Value = value;
            Next = next;
        }
    }
}
