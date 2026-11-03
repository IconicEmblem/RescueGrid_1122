using System;

namespace RescueGrid.Core.DomainAndStates
{
    /// <summary>An emergency call received by the dispatch centre (SRS Fig. 4.12).</summary>
    public class Incident
    {
        public int Id { get; }
        public string Category { get; }      // e.g. "TRAUMA", "BURN", "FIRE"
        public int Severity { get; set; }    // base severity used by the triage queue
        public int NodeId { get; }           // road node where the incident happened
        public DateTime ReceivedAt { get; }
        public string Status { get; set; }   // OPEN, DISPATCHED, RESOLVED, CANCELLED

        public Incident(int id, string category, int severity, int nodeId)
        {
            Id = id;
            Category = category;
            Severity = severity;
            NodeId = nodeId;
            ReceivedAt = DateTime.UtcNow;
            Status = "OPEN";
        }

        public override string ToString() =>
            $"Incident#{Id} [{Category}] severity={Severity} node={NodeId} status={Status}";
    }
}
