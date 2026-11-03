namespace RescueGrid.Core.DomainAndStates
{
    public class ToHospitalState : IUnitState
    {
        public string Name => "TO_HOSPITAL";

        public bool Handle(RescueUnit unit, string trigger, bool fromServer = false)
        {
            switch (trigger)
            {
                case "HANDOVER":
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
