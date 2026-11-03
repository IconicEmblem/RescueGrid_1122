namespace RescueGrid.Core.DsaGraphAndRouting
{
    /// <summary>
    /// Represents a directed, weighted edge in the Lahore road graph.
    /// Weight = travel time in minutes (Distance_Km / Base_Speed_Kmh * 60 * CongestionFactor).
    /// </summary>
    public class Edge
    {
        // ── Identity ──────────────────────────────────────────────────────────
        public int FromNode { get; }
        public int ToNode   { get; }

        // ── Static road properties ────────────────────────────────────────────
        public double DistanceKm    { get; }
        public double BaseSpeedKmh  { get; }

        // ── Dynamic congestion (admin can raise this for choke-point sim) ─────
        private double _congestionFactor;
        public double CongestionFactor
        {
            get => _congestionFactor;
            set => _congestionFactor = value < 1.0 ? 1.0 : value; // never below 1×
        }

        // ── Derived weight (minutes) used by Dijkstra ─────────────────────────
        /// <summary>
        /// Travel time in minutes. Recalculated on every read so Dijkstra always
        /// sees the latest congestion value without rebuilding the graph.
        /// </summary>
        public double Weight => (DistanceKm / BaseSpeedKmh) * 60.0 * _congestionFactor;

        // ── Road closed flag (infinite weight, but edge kept for rollback) ────
        public bool IsClosed { get; private set; }

        // ── Constructor ───────────────────────────────────────────────────────
        /// <summary>
        /// Creates a directed road edge from <paramref name="from"/> to <paramref name="to"/>.
        /// </summary>
        /// <param name="from">Source node id (intersection).</param>
        /// <param name="to">Destination node id.</param>
        /// <param name="distanceKm">Physical road length in kilometres.</param>
        /// <param name="baseSpeedKmh">Free-flow speed limit in km/h.</param>
        /// <param name="congestionFactor">
        /// Initial congestion multiplier ≥ 1.0.
        /// 1.0 = free-flow; 2.0 = twice as slow; 3.0+ = heavy jam.
        /// </param>
        public Edge(int from, int to, double distanceKm, double baseSpeedKmh,
                    double congestionFactor = 1.0)
        {
            FromNode         = from;
            ToNode           = to;
            DistanceKm       = distanceKm;
            BaseSpeedKmh     = baseSpeedKmh > 0 ? baseSpeedKmh : 1.0;  // guard / 0
            _congestionFactor = congestionFactor < 1.0 ? 1.0 : congestionFactor;
            IsClosed         = false;
        }

        // ── Choke-point simulation helpers ────────────────────────────────────
        /// <summary>Closes this road (sets weight to effectively infinity).</summary>
        public void Close() => IsClosed = true;

        /// <summary>Re-opens a previously closed road.</summary>
        public void Open()  => IsClosed = false;

        /// <summary>
        /// Returns the effective Dijkstra weight:
        /// double.MaxValue / 2 when closed, normal travel time otherwise.
        /// </summary>
        public double EffectiveWeight =>
            IsClosed ? double.MaxValue / 2.0 : Weight;

        public override string ToString() =>
            $"Edge({FromNode}→{ToNode} | {DistanceKm:F1} km @ {BaseSpeedKmh} km/h " +
            $"× {_congestionFactor:F2} = {Weight:F2} min{(IsClosed ? " [CLOSED]" : "")})";
    }
}
