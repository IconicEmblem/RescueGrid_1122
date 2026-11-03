namespace RescueGrid.Core.DsaGraphAndRouting
{
    /// <summary>
    /// Generic binary Min-Heap (custom implementation — no built-in PriorityQueue).
    /// Used internally by Dijkstra as its priority queue.
    ///
    /// Stores (Priority, Value) pairs where the entry with the LOWEST
    /// Priority value is always at the top.
    ///
    /// Complexity:
    ///   Insert  — O(log N)
    ///   ExtractMin — O(log N)
    ///   Peek    — O(1)
    /// </summary>
    /// <typeparam name="TValue">Type of the payload (e.g., int for node ids).</typeparam>
    public class MinHeap<TValue>
    {
        // ── Internal heap node ────────────────────────────────────────────────
        private struct HeapNode
        {
            public double Priority;
            public TValue Value;
        }

        // ── Storage ───────────────────────────────────────────────────────────
        private HeapNode[] _data;
        private int        _size;

        // ── Constructor ───────────────────────────────────────────────────────
        /// <summary>
        /// Creates a min-heap with the given initial capacity.
        /// The array doubles automatically when full.
        /// </summary>
        public MinHeap(int initialCapacity = 64)
        {
            _data = new HeapNode[initialCapacity > 0 ? initialCapacity : 64];
            _size = 0;
        }

        // ── Properties ────────────────────────────────────────────────────────
        public int  Count   => _size;
        public bool IsEmpty => _size == 0;

        // ── Public API ────────────────────────────────────────────────────────
        /// <summary>Inserts a new (priority, value) pair. O(log N).</summary>
        public void Insert(double priority, TValue value)
        {
            if (_size == _data.Length)
                Resize();

            _data[_size] = new HeapNode { Priority = priority, Value = value };
            BubbleUp(_size);
            _size++;
        }

        /// <summary>
        /// Returns the value with the lowest priority without removing it. O(1).
        /// </summary>
        public TValue Peek()
        {
            if (_size == 0)
                throw new InvalidOperationException("MinHeap is empty.");
            return _data[0].Value;
        }

        /// <summary>
        /// Removes and returns the value with the lowest priority. O(log N).
        /// </summary>
        public TValue ExtractMin()
        {
            if (_size == 0)
                throw new InvalidOperationException("MinHeap is empty.");

            TValue minValue = _data[0].Value;

            // Move last element to root and restore heap property
            _size--;
            if (_size > 0)
            {
                _data[0] = _data[_size];
                SiftDown(0);
            }

            return minValue;
        }

        /// <summary>
        /// Removes and returns both the priority and value of the minimum entry. O(log N).
        /// </summary>
        public (double Priority, TValue Value) ExtractMinWithPriority()
        {
            if (_size == 0)
                throw new InvalidOperationException("MinHeap is empty.");

            var min = (_data[0].Priority, _data[0].Value);

            _size--;
            if (_size > 0)
            {
                _data[0] = _data[_size];
                SiftDown(0);
            }

            return min;
        }

        /// <summary>Removes all elements from the heap.</summary>
        public void Clear() => _size = 0;

        // ── Internal helpers ──────────────────────────────────────────────────
        /// <summary>
        /// Bubbles element at index <paramref name="i"/> up until the heap property
        /// is satisfied (parent.Priority ≤ child.Priority).
        /// </summary>
        private void BubbleUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_data[parent].Priority <= _data[i].Priority)
                    break;

                Swap(i, parent);
                i = parent;
            }
        }

        /// <summary>
        /// Sifts element at index <paramref name="i"/> down until the heap property
        /// is satisfied (parent.Priority ≤ both children's priorities).
        /// </summary>
        private void SiftDown(int i)
        {
            while (true)
            {
                int left  = 2 * i + 1;
                int right = 2 * i + 2;
                int smallest = i;

                if (left  < _size && _data[left].Priority  < _data[smallest].Priority)
                    smallest = left;
                if (right < _size && _data[right].Priority < _data[smallest].Priority)
                    smallest = right;

                if (smallest == i) break;

                Swap(i, smallest);
                i = smallest;
            }
        }

        private void Swap(int a, int b)
        {
            HeapNode tmp = _data[a];
            _data[a]     = _data[b];
            _data[b]     = tmp;
        }

        /// <summary>Doubles the internal array when capacity is reached.</summary>
        private void Resize()
        {
            var bigger = new HeapNode[_data.Length * 2];
            Array.Copy(_data, bigger, _data.Length);
            _data = bigger;
        }
    }
}
