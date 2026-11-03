namespace RescueGrid.Core.DomainAndStates
{
    /// <summary>State pattern interface (SRS Fig. 4.7 / 4.12).
    /// Triggers: DISPATCH, ARRIVE, PICKUP, HANDOVER, SIGNAL_LOST, SIGNAL_RESTORED.
    /// SIGNAL_LOST is server-only: unit telemetry can never set COMMS_LOST itself.</summary>
    public interface IUnitState
    {
        string Name { get; }

        /// <summary>Returns true if the transition was valid and applied.</summary>
        bool Handle(RescueUnit unit, string trigger, bool fromServer = false);
    }
}
