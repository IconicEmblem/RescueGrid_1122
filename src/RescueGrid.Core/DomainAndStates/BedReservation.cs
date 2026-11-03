using System;

namespace RescueGrid.Core.DomainAndStates
{
    /// <summary>A bed held at a hospital for an incident. Released if the unit loses signal
    /// or no ACK arrives within 3 seconds.</summary>
    public class BedReservation
    {
        public int Id { get; }
        public int IncidentId { get; }
        public int HospitalId { get; }
        public string BedCategory { get; }
        public DateTime CreatedAt { get; }
        public string Status { get; set; }   // HELD, CONFIRMED, RELEASED

        public BedReservation(int id, int incidentId, int hospitalId, string bedCategory)
        {
            Id = id;
            IncidentId = incidentId;
            HospitalId = hospitalId;
            BedCategory = bedCategory;
            CreatedAt = DateTime.UtcNow;
            Status = "HELD";
        }
    }
}
