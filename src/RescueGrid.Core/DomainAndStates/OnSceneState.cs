namespace RescueGrid.Core.DomainAndStates
{
    public class OnSceneState : IUnitState
    {
        public string Name => "ON_SCENE";

        public bool Handle(RescueUnit unit, string trigger, bool fromServer = false)
        {
            switch (trigger)
            {
                case "PICKUP":
                    unit.ChangeState(new ToHospitalState());
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
