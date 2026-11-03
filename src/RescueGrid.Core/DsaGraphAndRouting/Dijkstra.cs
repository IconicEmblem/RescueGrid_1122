namespace RescueGrid.Core.DsaGraphAndRouting
{
    /// <summary>
    /// Congestion-aware Dijkstra router for RescueGrid 1122.
    ///
    /// Two public methods implement the two algorithms discussed in the SRS:
    ///
    /// 1. <see cref="ShortestPathsFromSource"/> — classic single-source Dijkstra.
    ///    Run on the REVERSED graph from the incident node to get the
    ///    shortest travel time from every ambulance to the incident in one pass.
    ///
    /// 2. <see cref="FindFastestUnit"/> — convenience wrapper that calls
    ///    ShortestPathsFromSource on the reversed graph and returns the
    ///    ambulance node id with the lowest travel time among available units.
    ///
    /// 3. <see cref="ShortestPath"/> — point-to-point path reconstruction
    ///    (used to find route from scene to best hospital).
    ///
    /// Complexity: O((V + E) log V) with the custom MinHeap.
    /// No built-in Dictionary is used for dist[]; a plain double[] array is used instead.
    /// </summary>
    public static class Dijkstra
    {
        private const double Infinity = double.MaxValue / 2.0;

        // ── 1. Single-source shortest paths ───────────────────────────────────
        /// <summary>
        /// Runs Dijkstra from <paramref name="sourceNode"/> on the given graph.
        /// </summary>
        /// <param name="graph">The graph to run on (pass the reversed graph to find
        /// distances TO a particular node from all others in one pass).</param>
        /// <param name="sourceNode">Starting node id.</param>
        /// <param name="capacity">
        /// Must be ≥ highest node id + 1 (same value used to construct the Graph).
        /// </param>
        /// <returns>
        /// A <see cref="DijkstraResult"/> containing dist[] and prev[] arrays.
        /// dist[v] = minimum travel time in minutes from source to v (Infinity if unreachable).
        /// prev[v] = predecessor of v on the shortest path (-1 if none).
        /// </returns>
        public static DijkstraResult ShortestPathsFromSource(
            Graph graph, int sourceNode, int capacity)
        {
            // dist[] and prev[] as raw arrays — no built-in Dictionary
            double[] dist = new double[capacity];
            int[]    prev = new int[capacity];

            for (int i = 0; i < capacity; i++)
            {
                dist[i] = Infinity;
                prev[i] = -1;
            }

            dist[sourceNode] = 0.0;

            // MinHeap stores (travelTime, nodeId)
            var heap = new MinHeap<int>(capacity);
            heap.Insert(0.0, sourceNode);

            bool[] visited = new bool[capacity];

            while (!heap.IsEmpty)
            {
                var (d, u) = heap.ExtractMinWithPriority();

                // Lazy-deletion: skip stale entries
                if (visited[u]) continue;
                visited[u] = true;

                // Relax all outgoing edges from u
                foreach (var edge in graph.GetNeighbours(u))
                {
                    int    v      = edge.ToNode;
                    double newDist = d + edge.EffectiveWeight;

                    if (newDist < dist[v])
                    {
                        dist[v] = newDist;
                        prev[v] = u;
                        heap.Insert(newDist, v);   // lazy: push updated entry
                    }
                }
            }

            return new DijkstraResult(sourceNode, dist, prev);
        }

        // ── 2. Find the fastest available ambulance to an incident ─────────────
        /// <summary>
        /// Finds the available unit (ambulance/fire) that can reach
        /// <paramref name="incidentNode"/> in the shortest travel time.
        ///
        /// Internally runs single-source Dijkstra on the REVERSED graph from
        /// the incident node — one pass gives travel times from every node
        /// to the incident, so all available units are evaluated simultaneously.
        /// </summary>
        /// <param name="originalGraph">The live road graph (will be reversed internally).</param>
        /// <param name="incidentNode">Node where the emergency occurred.</param>
        /// <param name="availableUnitNodes">
        /// Array of node ids where AVAILABLE units are currently located.
        /// </param>
        /// <param name="capacity">Graph capacity (max node id + 1).</param>
        /// <returns>
        /// The node id of the fastest unit, or -1 if none are reachable.
        /// Also outputs <paramref name="travelTimeMinutes"/> for the best unit.
        /// </returns>
        public static int FindFastestUnit(
            Graph   originalGraph,
            int     incidentNode,
            int[]   availableUnitNodes,
            int     capacity,
            out double travelTimeMinutes)
        {
            travelTimeMinutes = Infinity;

            if (availableUnitNodes == null || availableUnitNodes.Length == 0)
                return -1;

            // Build reversed graph once per dispatch decision
            Graph reversed = originalGraph.BuildReversed();

            // Single-source from incident on reversed graph
            DijkstraResult result = ShortestPathsFromSource(reversed, incidentNode, capacity);

            int bestUnit = -1;
            foreach (int unitNode in availableUnitNodes)
            {
                if (unitNode < 0 || unitNode >= capacity) continue;
                double t = result.Dist[unitNode];
                if (t < travelTimeMinutes)
                {
                    travelTimeMinutes = t;
                    bestUnit          = unitNode;
                }
            }

            return bestUnit;
        }

        // ── 3. Point-to-point shortest path with reconstruction ────────────────
        /// <summary>
        /// Returns the shortest path (list of node ids) and travel time from
        /// <paramref name="from"/> to <paramref name="to"/> on the given graph.
        /// Used to compute the route from the incident scene to the hospital.
        /// </summary>
        /// <returns>
        /// Ordered list of node ids from <paramref name="from"/> to <paramref name="to"/>,
        /// inclusive. Empty list if no path exists.
        /// Also outputs <paramref name="travelTimeMinutes"/>.
        /// </returns>
        public static List<int> ShortestPath(
            Graph  graph,
            int    from,
            int    to,
            int    capacity,
            out double travelTimeMinutes)
        {
            DijkstraResult result = ShortestPathsFromSource(graph, from, capacity);
            travelTimeMinutes = result.Dist[to];

            if (travelTimeMinutes >= Infinity)
                return new List<int>(); // unreachable

            // Reconstruct path by following prev[] backwards
            var path = new List<int>();
            int cur  = to;
            while (cur != -1)
            {
                path.Add(cur);
                cur = result.Prev[cur];
            }
            path.Reverse();
            return path;
        }
    }

    // ── Result container ──────────────────────────────────────────────────────
    /// <summary>
    /// Holds the output of a single-source Dijkstra run.
    /// </summary>
    public class DijkstraResult
    {
        public int      Source { get; }
        public double[] Dist   { get; }   // dist[v] = shortest travel time to v
        public int[]    Prev   { get; }   // prev[v] = predecessor (-1 = none)

        public DijkstraResult(int source, double[] dist, int[] prev)
        {
            Source = source;
            Dist   = dist;
            Prev   = prev;
        }

        /// <summary>True if node v is reachable from the source.</summary>
        public bool IsReachable(int v) => Dist[v] < double.MaxValue / 2.0;
    }
}
