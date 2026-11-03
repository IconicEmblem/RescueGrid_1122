namespace RescueGrid.Core.DomainAndStates
{
    /// <summary>An ambulance or fire unit. Behaviour depends on its current state object.</summary>
    public class RescueUnit
    {
        public string Id { get; }
        public string UnitType { get; }      // AMBULANCE, FIRE_TRUCK
        public int CurrentNode { get; set; }
        public IUnitState State { get; private set; }

        public RescueUnit(string id, string unitType, int currentNode)
        {
            Id = id;
            UnitType = unitType;
            CurrentNode = currentNode;
            State = new AvailableState();
        }

        public void ChangeState(IUnitState newState) => State = newState;

        /// <summary>Ask the current state to handle a trigger.</summary>
        public bool Handle(string trigger, bool fromServer = false) =>
            State.Handle(this, trigger, fromServer);
    }
}
