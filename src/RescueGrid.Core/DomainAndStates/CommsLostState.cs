namespace RescueGrid.Core.DomainAndStates
{
    /// <summary>Set only by the server Watchdog after 8 s of silence.
    /// A unit cannot leave this state on its own; the server restores it.</summary>
    public class CommsLostState : IUnitState
    {
        public string Name => "COMMS_LOST";

        public bool Handle(RescueUnit unit, string trigger, bool fromServer = false)
        {
            if (trigger == "SIGNAL_RESTORED" && fromServer)
            {
                unit.ChangeState(new AvailableState());
                return true;
            }
            return false;
        }
    }
}
