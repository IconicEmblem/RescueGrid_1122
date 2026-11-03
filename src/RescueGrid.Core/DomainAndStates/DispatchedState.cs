namespace RescueGrid.Core.DomainAndStates
{
    public class DispatchedState : IUnitState
    {
        public string Name => "DISPATCHED";

        public bool Handle(RescueUnit unit, string trigger, bool fromServer = false)
        {
            switch (trigger)
            {
                case "ARRIVE":
                    unit.ChangeState(new OnSceneState());
                    return true;
                case "CANCEL":
                    unit.ChangeState(new AvailableState());
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
