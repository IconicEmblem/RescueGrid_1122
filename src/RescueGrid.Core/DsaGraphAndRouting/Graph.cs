using System.Text;

namespace RescueGrid.Core.DsaGraphAndRouting
{
    /// <summary>
    /// Directed, weighted road graph for Lahore using a custom adjacency list.
    /// Nodes  = intersections / landmarks (integer ids).
    /// Edges  = one-way road segments with travel-time weights.
    ///
    /// No built-in Dictionary or generic collections from System.Collections.Generic
    /// are used for the core adjacency storage — only raw arrays + open-addressing
    /// to satisfy the DSA requirement of custom data structures.
    /// (A simple Node[] with next-edge linked lists is used internally.)
    /// </summary>
    public class Graph
    {
        // ── Internal adjacency list node ──────────────────────────────────────
        private class AdjNode
        {
            public Edge     EdgeData;
            public AdjNode? Next;

            public AdjNode(Edge edge) { EdgeData = edge; Next = null; }
        }

        // ── Storage ───────────────────────────────────────────────────────────
        private readonly int        _capacity;   // max node id + 1
        private readonly AdjNode?[] _heads;      // _heads[u] = head of u's edge list
        private int                 _edgeCount;
        private int                 _nodeCount;  // how many node ids have been seen

        // ── Constructor ───────────────────────────────────────────────────────
        /// <summary>
        /// Initialises the graph.
        /// </summary>
        /// <param name="maxNodeId">
        /// The highest node id that will ever be inserted (e.g. 49 for a 50-node graph).
        /// Determines the size of the internal array.
        /// </param>
        public Graph(int maxNodeId = 99)
        {
            _capacity  = maxNodeId + 1;
            _heads     = new AdjNode?[_capacity];
            _edgeCount = 0;
            _nodeCount  = 0;
        }

        // ── Properties ────────────────────────────────────────────────────────
        public int NodeCount => _nodeCount;
        public int EdgeCount => _edgeCount;

        // ── Mutation ──────────────────────────────────────────────────────────
        /// <summary>
        /// Adds a directed edge from <paramref name="from"/> to <paramref name="to"/>.
        /// Duplicate edges (same from/to) are allowed (e.g., different lanes).
        /// </summary>
        public void AddEdge(int from, int to, double distanceKm,
                            double baseSpeedKmh, double congestionFactor = 1.0)
        {
            ValidateId(from);
            ValidateId(to);

            var edge = new Edge(from, to, distanceKm, baseSpeedKmh, congestionFactor);
            var node = new AdjNode(edge);

            // Prepend to head (O(1) insertion)
            node.Next   = _heads[from];
            _heads[from] = node;

            _edgeCount++;

            // Track unique node ids (simple: just mark that these ids exist)
            // We don't need a separate "nodes" collection for Dijkstra
            if (_heads[from]?.Next == null) _nodeCount++; // from is new head starter
        }

        /// <summary>
        /// Adds a bidirectional road (calls AddEdge twice with swapped from/to).
        /// </summary>
        public void AddBidirectionalEdge(int a, int b, double distanceKm,
                                         double baseSpeedKmh, double congestionFactor = 1.0)
        {
            AddEdge(a, b, distanceKm, baseSpeedKmh, congestionFactor);
            AddEdge(b, a, distanceKm, baseSpeedKmh, congestionFactor);
        }

        // ── Choke-point simulation ────────────────────────────────────────────
        /// <summary>
        /// Updates the congestion factor on the first edge found from → to.
        /// Dijkstra will use the new weight on the next call automatically.
        /// </summary>
        /// <returns>True if an edge was found and updated; false otherwise.</returns>
        public bool UpdateCongestion(int from, int to, double newCongestionFactor)
        {
            ValidateId(from);
            var cur = _heads[from];
            while (cur != null)
            {
                if (cur.EdgeData.ToNode == to)
                {
                    cur.EdgeData.CongestionFactor = newCongestionFactor;
                    return true;
                }
                cur = cur.Next;
            }
            return false;
        }

        /// <summary>Closes (blocks) the first edge found from → to.</summary>
        public bool CloseEdge(int from, int to)
        {
            ValidateId(from);
            var cur = _heads[from];
            while (cur != null)
            {
                if (cur.EdgeData.ToNode == to) { cur.EdgeData.Close(); return true; }
                cur = cur.Next;
            }
            return false;
        }

        /// <summary>Re-opens (unblocks) the first edge found from → to.</summary>
        public bool OpenEdge(int from, int to)
        {
            ValidateId(from);
            var cur = _heads[from];
            while (cur != null)
            {
                if (cur.EdgeData.ToNode == to) { cur.EdgeData.Open(); return true; }
                cur = cur.Next;
            }
            return false;
        }

        // ── Read ──────────────────────────────────────────────────────────────
        /// <summary>
        /// Returns all outgoing edges from <paramref name="nodeId"/> as an array.
        /// Used by Dijkstra to iterate neighbours.
        /// </summary>
        public Edge[] GetNeighbours(int nodeId)
        {
            if (nodeId < 0 || nodeId >= _capacity)
                return Array.Empty<Edge>();

            // Count first
            int count = 0;
            var cur   = _heads[nodeId];
            while (cur != null) { count++; cur = cur.Next; }

            // Fill
            var result = new Edge[count];
            cur = _heads[nodeId];
            for (int i = 0; i < count; i++)
            {
                result[i] = cur!.EdgeData;
                cur        = cur.Next;
            }
            return result;
        }

        /// <summary>
        /// Builds and returns the REVERSED graph (all edges flipped).
        /// Dijkstra on the reversed graph from the incident node gives
        /// shortest distance from every ambulance to the incident in one pass.
        /// </summary>
        public Graph BuildReversed()
        {
            var rev = new Graph(_capacity - 1);
            for (int u = 0; u < _capacity; u++)
            {
                var cur = _heads[u];
                while (cur != null)
                {
                    var e = cur.EdgeData;
                    // Flip direction; keep same weights
                    rev.AddEdge(e.ToNode, e.FromNode,
                                e.DistanceKm, e.BaseSpeedKmh, e.CongestionFactor);
                    cur = cur.Next;
                }
            }
            return rev;
        }

        // ── Debug ─────────────────────────────────────────────────────────────
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Graph [{_nodeCount} nodes used, {_edgeCount} edges, capacity={_capacity}]");
            for (int u = 0; u < _capacity; u++)
            {
                if (_heads[u] == null) continue;
                sb.Append($"  Node {u}: ");
                var cur = _heads[u];
                while (cur != null)
                {
                    sb.Append($"→{cur.EdgeData.ToNode}({cur.EdgeData.EffectiveWeight:F1}min) ");
                    cur = cur.Next;
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        // ── Internal guard ────────────────────────────────────────────────────
        private void ValidateId(int id)
        {
            if (id < 0 || id >= _capacity)
                throw new ArgumentOutOfRangeException(nameof(id),
                    $"Node id {id} is out of range [0, {_capacity - 1}].");
        }
    }
}
