using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RescueGrid1122.NetworkingAndWatchdog
{
    public class Watchdog
    {
        private readonly int _timeoutSec;
        private readonly int _checkIntervalSec;
        private CancellationTokenSource _cts;
        private bool _isRunning;
        private readonly ConcurrentDictionary<int, DateTime> _unitLastSeen = new ConcurrentDictionary<int, DateTime>();

        public event Action<int> OnUnitCommsLost;

        public Watchdog(int timeoutSec = 8, int checkIntervalSec = 3)
        {
            _timeoutSec = timeoutSec;
            _checkIntervalSec = checkIntervalSec;
        }

        public void RecordHeartbeat(int unitId)
        {
            _unitLastSeen[unitId] = DateTime.UtcNow;
        }

        public void Start()
        {
            if (_isRunning) return;

            _cts = new CancellationTokenSource();
            _isRunning = true;

            Task.Run(() => MonitorLoop(_cts.Token));
            Console.WriteLine($"[Watchdog] Started monitoring (Timeout: {_timeoutSec}s, Interval: {_checkIntervalSec}s)...");
        }

        private async Task MonitorLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_checkIntervalSec), token);
                    Check();
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Watchdog Error] {ex.Message}");
                }
            }
        }

        public void Check()
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _unitLastSeen)
            {
                int unitId = kvp.Key;
                double elapsedSeconds = (now - kvp.Value).TotalSeconds;

                if (elapsedSeconds > _timeoutSec)
                {
                    Console.WriteLine($"[WARNING] Unit {unitId} silent for {elapsedSeconds:F1}s! Flagging COMMS_LOST.");
                    OnUnitCommsLost?.Invoke(unitId);
                    _unitLastSeen.TryRemove(unitId, out _);
                }
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _cts?.Cancel();
            _isRunning = false;
            Console.WriteLine("[Watchdog] Stopped.");
        }
    }
}