using System.Collections.Generic;

namespace RescueGrid.Core.DomainAndStates
{
    /// <summary>A hospital with one BedStock per bed category (trauma, ICU, burn).</summary>
    public class Hospital
    {
        public int Id { get; }
        public string Name { get; }
        public int NodeId { get; }   // road node where the hospital sits
        public Dictionary<string, BedStock> Beds { get; } = new Dictionary<string, BedStock>();

        public Hospital(int id, string name, int nodeId)
        {
            Id = id;
            Name = name;
            NodeId = nodeId;
        }

        public void AddBedStock(string category, int total) =>
            Beds[category] = new BedStock(category, total);

        public int FreeBeds(string category) =>
            Beds.TryGetValue(category, out var stock) ? stock.FreeBeds() : 0;
    }
}
