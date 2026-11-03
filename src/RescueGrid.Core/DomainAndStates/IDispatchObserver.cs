namespace RescueGrid.Core.DomainAndStates
{
    /// <summary>Observer pattern: the dashboard (SignalR hub later) implements this
    /// to receive live dispatch events.</summary>
    public interface IDispatchObserver
    {
        void OnEvent(string eventType, string message);
    }
}
