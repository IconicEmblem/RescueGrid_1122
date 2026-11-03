namespace RescueGrid.Core.DomainAndStates
{
    public class AvailableState : IUnitState
    {
        public string Name => "AVAILABLE";

        public bool Handle(RescueUnit unit, string trigger, bool fromServer = false)
        {
            switch (trigger)
            {
                case "DISPATCH":
                    unit.ChangeState(new DispatchedState());
                    return true;
                case "SIGNAL_LOST" when fromServer:
                    unit.ChangeState(new CommsLostState());
                    return true;
                default:
                    return false;
            }
        }
    }
}
