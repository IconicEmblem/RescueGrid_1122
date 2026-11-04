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
            if (size == heap.Length) Array.Resize(ref heap, heap.Length * 2);

            double key = ComputeKey(baseSeverity, arrivalTime);
            heap[size] = new TriageNode(incidentId, baseSeverity, arrivalTime, key);
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

            TriageNode top = heap[0];
            size--;
            heap[0] = heap[size];
            heap[size] = null;
            if (size > 0) SiftDown(0);
            return top;
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
        }
    }
}
