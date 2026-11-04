using System;

namespace RescueGrid.Core.DsaTriageAndHash
{
    /// <summary>
    /// Custom max-heap priority queue for emergency triage.
    /// key = base severity - 2.5 x arrival time (minutes).
    /// The key is computed once at insert; older tickets have a smaller
    /// arrival time so they automatically rank higher than newer tickets
    /// of the same severity, with no re-sorting needed.
    /// </summary>
    public class MaxHeapAging
    {
        private const double AgingFactor = 2.5;
        private static readonly DateTime Reference = DateTime.UtcNow;

        private TriageNode[] heap;
        private int size;

        // ticket id -> position in the heap array (enables O(log n) Remove)
        private readonly HashMap<string, int> index = new HashMap<string, int>();

        public MaxHeapAging(int capacity = 16)
        {
            heap = new TriageNode[Math.Max(capacity, 2)];
        }

        public int Count => size;
        public bool IsEmpty => size == 0;

        private static double ComputeKey(int baseSeverity, DateTime arrivalTime)
        {
            double minutes = (arrivalTime - Reference).TotalMinutes;
            return baseSeverity - AgingFactor * minutes;
        }

        public void Insert(string incidentId, int baseSeverity)
        {
            Insert(incidentId, baseSeverity, DateTime.UtcNow);
        }

        public void Insert(string incidentId, int baseSeverity, DateTime arrivalTime)
        {
            if (index.ContainsKey(incidentId))
                throw new ArgumentException("Ticket already in queue: " + incidentId);
            if (size == heap.Length) Array.Resize(ref heap, heap.Length * 2);

            double key = ComputeKey(baseSeverity, arrivalTime);
            heap[size] = new TriageNode(incidentId, baseSeverity, arrivalTime, key);
            index.Put(incidentId, size);
            size++;
            SiftUp(size - 1);
        }

        /// <summary>Look at the highest-priority ticket without removing it.</summary>
        public TriageNode Peek()
        {
            if (size == 0) throw new InvalidOperationException("Heap is empty.");
            return heap[0];
        }

        /// <summary>Remove and return the highest-priority ticket.</summary>
        public TriageNode ExtractMax()
        {
            if (size == 0) throw new InvalidOperationException("Heap is empty.");
            return RemoveAt(0);
        }

        public bool Contains(string incidentId)
        {
            return index.ContainsKey(incidentId);
        }

        /// <summary>Cancel a ticket by id (e.g. caller hung up). O(log n).</summary>
        public bool Remove(string incidentId)
        {
            if (!index.TryGet(incidentId, out int i)) return false;
            RemoveAt(i);
            return true;
        }

        private TriageNode RemoveAt(int i)
        {
            int last = size - 1;
            if (i != last) Swap(i, last);

            TriageNode removed = heap[last];
            index.Remove(removed.IncidentId);
            heap[last] = null;
            size--;

            if (i < size)
            {
                SiftUp(i);
                SiftDown(i);
            }
            return removed;
        }

        private void SiftUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (heap[i].Key <= heap[parent].Key) break;
                Swap(i, parent);
                i = parent;
            }
        }

        private void SiftDown(int i)
        {
            while (true)
            {
                int left = 2 * i + 1;
                int right = left + 1;
                int largest = i;

                if (left < size && heap[left].Key > heap[largest].Key) largest = left;
                if (right < size && heap[right].Key > heap[largest].Key) largest = right;
                if (largest == i) break;

                Swap(i, largest);
                i = largest;
            }
        }

        private void Swap(int a, int b)
        {
            TriageNode t = heap[a];
            heap[a] = heap[b];
            heap[b] = t;
            index.Put(heap[a].IncidentId, a);
            index.Put(heap[b].IncidentId, b);
        }
    }
}
