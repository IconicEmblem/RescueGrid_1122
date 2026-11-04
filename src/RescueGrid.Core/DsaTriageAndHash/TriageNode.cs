using System;

namespace RescueGrid.Core.DsaTriageAndHash
{
    /// <summary>One emergency ticket waiting in the triage queue.</summary>
    public class TriageNode
    {
        public string IncidentId { get; }
        public int BaseSeverity { get; }
        public DateTime ArrivalTime { get; }

        /// <summary>Priority key = base severity - 2.5 x arrival minutes.</summary>
        public double Key { get; }

        public TriageNode(string incidentId, int baseSeverity, DateTime arrivalTime, double key)
        {
            IncidentId = incidentId;
            BaseSeverity = baseSeverity;
            ArrivalTime = arrivalTime;
            Key = key;
        }
    }
}
