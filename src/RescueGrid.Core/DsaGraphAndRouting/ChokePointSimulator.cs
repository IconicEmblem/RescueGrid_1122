namespace RescueGrid.Core.DsaGraphAndRouting
{
    /// <summary>
    /// Admin-facing choke-point simulation controller for RescueGrid 1122.
    ///
    /// Wraps Graph mutation methods (UpdateCongestion, CloseEdge, OpenEdge)
    /// with an audit log so every simulated road event is recorded and
    /// can be rolled back for the next dispatch cycle.
    ///
    /// SRS Reference: FR-05 — Admins can simulate choke points or road closures,
    /// and the next Dijkstra call automatically uses the updated edge weights.
    /// </summary>
    public class ChokePointSimulator
    {
        // ── Internal audit entry ──────────────────────────────────────────────
        private class SimEvent
        {
            public int    FromNode;
            public int    ToNode;
            public string Kind;           // "CONGESTION" | "CLOSE" | "OPEN"
            public double OldCongestion;  // for rollback
            public double NewCongestion;
            public DateTime AppliedAt;

            public SimEvent(int from, int to, string kind,
                            double oldCong, double newCong)
            {
                FromNode      = from;
                ToNode        = to;
                Kind          = kind;
                OldCongestion = oldCong;
                NewCongestion = newCong;
                AppliedAt     = DateTime.UtcNow;
            }

            public override string ToString() =>
                $"[{AppliedAt:HH:mm:ss}] {Kind} edge {FromNode}→{ToNode} " +
                $"(congestion {OldCongestion:F2} → {NewCongestion:F2})";
        }

        // ── State ─────────────────────────────────────────────────────────────
        private readonly Graph       _graph;
        private readonly List<SimEvent> _log = new();

        // ── Constructor ───────────────────────────────────────────────────────
        public ChokePointSimulator(Graph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        // ── Public API ────────────────────────────────────────────────────────
        /// <summary>
        /// Raises the congestion factor on edge from→to.
        /// The next Dijkstra call will automatically use the heavier weight.
        /// </summary>
        /// <param name="from">Source node.</param>
        /// <param name="to">Destination node.</param>
        /// <param name="congestionFactor">
        /// New congestion multiplier (≥ 1.0).
        /// Typical viva values: 1.0 = free-flow, 2.5 = rush-hour jam, 5.0 = blocked.
        /// </param>
        /// <returns>True if the edge was found and updated.</returns>
        public bool SetCongestion(int from, int to, double congestionFactor)
        {
            // Read current congestion for rollback (approximate — find edge first)
            double old = GetCurrentCongestion(from, to);

            bool ok = _graph.UpdateCongestion(from, to, congestionFactor);
            if (ok)
                _log.Add(new SimEvent(from, to, "CONGESTION", old, congestionFactor));

            return ok;
        }

        /// <summary>
        /// Completely closes a road edge (weight → double.MaxValue/2).
        /// Models an underpass flood, accident blockage, or construction closure.
        /// </summary>
        public bool CloseRoad(int from, int to)
        {
            double old = GetCurrentCongestion(from, to);
            bool ok    = _graph.CloseEdge(from, to);
            if (ok)
                _log.Add(new SimEvent(from, to, "CLOSE", old, double.MaxValue / 2.0));
            return ok;
        }

        /// <summary>
        /// Re-opens a previously closed road edge.
        /// </summary>
        public bool OpenRoad(int from, int to)
        {
            bool ok = _graph.OpenEdge(from, to);
            if (ok)
                _log.Add(new SimEvent(from, to, "OPEN", double.MaxValue / 2.0, 1.0));
            return ok;
        }

        /// <summary>
        /// Resets ALL simulated events: re-opens closed roads and sets congestion
        /// back to 1.0 (free-flow) for every entry in the audit log.
        /// Call this at the start of each new demo scenario.
        /// </summary>
        public void ResetAll()
        {
            // Walk log in reverse and undo each event
            for (int i = _log.Count - 1; i >= 0; i--)
            {
                var ev = _log[i];
                if (ev.Kind == "CLOSE")
                    _graph.OpenEdge(ev.FromNode, ev.ToNode);
                else
                    _graph.UpdateCongestion(ev.FromNode, ev.ToNode, ev.OldCongestion);
            }
            _log.Clear();
        }

        /// <summary>Returns a read-only snapshot of the simulation audit log.</summary>
        public IReadOnlyList<string> GetLog()
        {
            var lines = new List<string>(_log.Count);
            foreach (var ev in _log)
                lines.Add(ev.ToString());
            return lines;
        }

        // ── Internal helper ───────────────────────────────────────────────────
        /// <summary>
        /// Reads the current congestion factor of the first matching edge.
        /// Returns 1.0 if the edge is not found (safe default for log).
        /// </summary>
        private double GetCurrentCongestion(int from, int to)
        {
            foreach (var edge in _graph.GetNeighbours(from))
            {
                if (edge.ToNode == to)
                    return edge.CongestionFactor;
            }
            return 1.0;
        }
    }
}
