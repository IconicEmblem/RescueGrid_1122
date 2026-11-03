namespace RescueGrid.Core.DomainAndStates
{
    /// <summary>Bed counts for one category in one hospital.
    /// A bed is free only if occupied + reserved &lt; total (same rule as the SQL update).</summary>
    public class BedStock
    {
        public string Category { get; }
        public int Total { get; }
        public int Occupied { get; set; }
        public int Reserved { get; set; }

        public BedStock(string category, int total)
        {
            Category = category;
            Total = total;
        }

        public int FreeBeds() => Total - Occupied - Reserved;

        /// <summary>Hold one bed. Returns false if none is free.</summary>
        public bool TryHold()
        {
            if (FreeBeds() <= 0) return false;
            Reserved++;
            return true;
        }

        /// <summary>Release a previously held bed.</summary>
        public void Release()
        {
            if (Reserved > 0) Reserved--;
        }
    }
}
