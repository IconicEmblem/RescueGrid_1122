using System.Collections.Generic;

namespace RescueGrid.Core.DomainAndStates
{
    /// <summary>Command pattern: one dispatch that can be undone.
    /// Execute() sends the unit out; Rollback() frees the unit and releases the held bed
    /// (used when a unit drops signal or the hospital NACKs).</summary>
    public class DispatchCommand
    {
        private readonly List<IDispatchObserver> _observers = new List<IDispatchObserver>();

        public Incident Incident { get; }
        public RescueUnit Unit { get; }
        public Hospital Hospital { get; }
        public BedReservation Reservation { get; private set; }
        public bool Executed { get; private set; }

        public DispatchCommand(Incident incident, RescueUnit unit, Hospital hospital)
        {
            Incident = incident;
            Unit = unit;
            Hospital = hospital;
        }

        public void AddObserver(IDispatchObserver observer) => _observers.Add(observer);

        private void Notify(string type, string message)
        {
            foreach (var o in _observers) o.OnEvent(type, message);
        }

        public bool Execute()
        {
            if (!Unit.Handle("DISPATCH")) return false;   // unit must be AVAILABLE

            if (!Hospital.Beds.TryGetValue(Incident.Category, out var stock) || !stock.TryHold())
            {
                Unit.Handle("CANCEL");                    // no bed: put unit back
                Notify("BED_NACK", $"{Hospital.Name} has no free {Incident.Category} bed");
                return false;
            }

            Reservation = new BedReservation(Incident.Id, Incident.Id, Hospital.Id, Incident.Category);
            Incident.Status = "DISPATCHED";
            Executed = true;
            Notify("DISPATCHED", $"{Unit.Id} -> Incident#{Incident.Id}, bed held at {Hospital.Name}");
            return true;
        }

        public void Rollback()
        {
            if (!Executed) return;

            if (Hospital.Beds.TryGetValue(Incident.Category, out var stock)) stock.Release();
            if (Reservation != null) Reservation.Status = "RELEASED";

            Unit.ChangeState(new AvailableState());       // rollback is a server action
            Incident.Status = "OPEN";                     // incident goes back to the triage queue
            Executed = false;
            Notify("ROLLED_BACK", $"Dispatch of {Unit.Id} for Incident#{Incident.Id} rolled back");
        }
    }
}
