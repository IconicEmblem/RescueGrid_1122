using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RescueGrid1122.NetworkingAndWatchdog
{
    public class TcpMessageServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private bool _isRunning;
        private readonly ConcurrentDictionary<string, TcpClient> _connectedClients = new ConcurrentDictionary<string, TcpClient>();

        public TcpMessageServer(int port = 11221)
        {
            _port = port;
        }

        public void Start()
        {
            if (_isRunning) return;

            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _cts = new CancellationTokenSource();
            _isRunning = true;

            Task.Run(() => AcceptClientsLoop(_cts.Token));
            Console.WriteLine($"[TcpMessageServer] Started and listening on TCP port {_port}...");
        }

        private async Task AcceptClientsLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    string clientId = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
                    _connectedClients[clientId] = client;
                    Console.WriteLine($"[TCP Connected] Client connected: {clientId}");

                    _ = Task.Run(() => HandleClientAsync(client, clientId, token));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TcpMessageServer Error] {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, string clientId, CancellationToken token)
        {
            try
            {
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    string line;
                    while (!token.IsCancellationRequested && (line = await reader.ReadLineAsync()) != null)
                    {
                        ProcessCommand(line, writer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP Error ({clientId})] {ex.Message}");
            }
            finally
            {
                _connectedClients.TryRemove(clientId, out _);
                client.Close();
                Console.WriteLine($"[TCP Disconnected] Client removed: {clientId}");
            }
        }

        public void ProcessCommand(string rawMessage, StreamWriter writer)
        {
            var parts = rawMessage.Split('|');
            string commandType = parts[0].ToUpper().Trim();

            Console.WriteLine($"[TCP Command RX] Type: {commandType} | Raw: {rawMessage}");

            switch (commandType)
            {
                case "INCIDENT":
                    writer.WriteLine("ACK_INCIDENT|Received and queued for triage.");
                    break;
                case "RESERVE_BED":
                    writer.WriteLine("ACK_LOCKED|Bed reserved successfully.");
                    break;
                case "DISPATCH":
                    writer.WriteLine("ACK_DISPATCH|Unit dispatched.");
                    break;
                case "CANCEL":
                    writer.WriteLine("ACK_CANCEL|Dispatch cancelled.");
                    break;
                default:
                    writer.WriteLine("ERR|Unknown command protocol.");
                    break;
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _cts?.Cancel();
            _listener?.Stop();
            _isRunning = false;
            Console.WriteLine("[TcpMessageServer] Stopped.");
        }
    }
}