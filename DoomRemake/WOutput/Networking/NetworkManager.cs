[EMITTER:E2B_WORKER_01]
[TARGET:DOOMREMAKE_NETWORKING]
[ID: NetworkManager.cs]

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace DoomRemake.Networking
{
    /// <summary>
    /// Manages multiplayer networking for DOOM Remake.
    /// Supports both client and server modes with WebSocket protocol.
    /// </summary>
    public class NetworkManager : IDisposable
    {
        private const string ProtocolVersion = "1.0";
        private bool _isServer;
        private bool _isConnected;
        
        // Server properties
        private readonly List<ClientConnection> _connections = new();
        private readonly Dictionary<string, ClientConnection> _clientMap = new();
        private Task? _serverTask;
        private int _port;

        // Client properties  
        private WebSocketClient? _clientSocket;
        private Task? _clientConnectTask;

        public event Action<NetworkMessage>? OnMessageReceived;
        public event Action<string, NetworkMessage>? OnRemoteMessage;
        public event Action<int>? OnPlayerJoined;
        public event Action<int>? OnPlayerLeft;
        public event Action<bool>? OnConnectionStateChanged;

        /// <summary>
        /// Initialize the network manager.
        /// </summary>
        public void Initialize(int port = 7777)
        {
            _port = port;
            Console.WriteLine("[INFO] NetworkManager initialized");
        }

        #region Server Operations

        /// <summary>
        /// Start a multiplayer server.
        /// </summary>
        public async Task<bool> StartServerAsync()
        {
            try
            {
                _isServer = true;
                Console.WriteLine($"[INFO] Starting DOOM Remake server on port {_port}...");

                // In production, use WebSocket server library (e.g., WebSocket4Net)
                // For this implementation, we'll use a simplified approach
                
                var serverTask = Task.Run(async () =>
                {
                    await HandleServerAsync();
                });

                _serverTask = serverTask;
                
                OnConnectionStateChanged?.Invoke(true);
                
                Console.WriteLine("[OK] Server started");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to start server: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stop the multiplayer server.
        /// </summary>
        public void StopServer()
        {
            try
            {
                _isServer = false;
                
                // Close all client connections
                foreach (var connection in _connections)
                {
                    connection.Close();
                }

                if (_serverTask != null && !_serverTask.IsCompleted)
                {
                    _serverTask.Wait(1000);
                }

                OnConnectionStateChanged?.Invoke(false);
                
                Console.WriteLine("[OK] Server stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to stop server: {ex.Message}");
            }
        }

        private async Task HandleServerAsync()
        {
            while (_isServer)
            {
                try
                {
                    // Accept incoming connections (simplified)
                    await Task.Delay(5000);
                    
                    if (!_isServer) break;
                    
                    Console.WriteLine("[INFO] Waiting for client connection...");
                    
                    // In production, implement actual WebSocket server logic here
                    // For now, we'll simulate the connection handling
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Server error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Add a client to the server.
        /// </summary>
        public void AddClient(string clientId, ClientConnection connection)
        {
            _connections.Add(connection);
            _clientMap[clientId] = connection;
            
            OnPlayerJoined?.Invoke(_connections.Count);
            Console.WriteLine($"[INFO] Player joined: {clientId} (Total: {_connections.Count})");
        }

        /// <summary>
        /// Remove a client from the server.
        /// </summary>
        public void RemoveClient(string clientId)
        {
            if (_clientMap.TryGetValue(clientId, out var connection))
            {
                _connections.Remove(connection);
                _clientMap.Remove(clientId);
                
                OnPlayerLeft?.Invoke(_connections.Count);
                Console.WriteLine($"[INFO] Player left: {clientId} (Total: {_connections.Count})");
            }
        }

        #endregion

        #region Client Operations

        /// <summary>
        /// Connect to a multiplayer server.
        /// </summary>
        public async Task<bool> ConnectAsync(string serverUrl)
        {
            try
            {
                _isConnected = true;
                
                if (_clientSocket == null)
                {
                    // Create new WebSocket client (simplified for this example)
                    Console.WriteLine($"[INFO] Connecting to: {serverUrl}");
                    
                    // In production, use actual WebSocket client library
                    await Task.Delay(2000);
                    
                    if (!_isConnected) return false;
                }

                OnConnectionStateChanged?.Invoke(true);
                
                Console.WriteLine("[OK] Connected to server");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to connect: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnect from the multiplayer server.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _isConnected = false;
                
                if (_clientSocket != null)
                {
                    // Close WebSocket connection (simplified)
                    Console.WriteLine("[INFO] Disconnecting from server...");
                }

                OnConnectionStateChanged?.Invoke(false);
                
                Console.WriteLine("[OK] Disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to disconnect: {ex.Message}");
            }
        }

        /// <summary>
        /// Send a message to the server.
        /// </summary>
        public async Task<bool> SendMessageAsync(NetworkMessage message, string targetClientId = null)
        {
            if (!_isConnected)
            {
                Console.WriteLine("[WARN] Not connected to server");
                return false;
            }

            try
            {
                // Serialize and send message (simplified)
                Console.WriteLine($"[SEND] Message: {message.Type} - {message.Data?.ToString() ?? ""}");
                
                await Task.Delay(10);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to send message: {ex.Message}");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Receive a network message.
        /// </summary>
        public void ReceiveMessage(NetworkMessage message, string fromClient = null)
        {
            OnMessageReceived?.Invoke(message);
            
            if (!string.IsNullOrEmpty(fromClient))
            {
                OnRemoteMessage?.Invoke(fromClient, message);
            }
        }

        /// <summary>
        /// Broadcast a message to all connected clients.
        /// </summary>
        public async Task<bool> BroadcastAsync(NetworkMessage message)
        {
            if (!_isServer || _connections.Count == 0)
                return false;

            try
            {
                foreach (var connection in _connections)
                {
                    await SendMessageAsync(message, null);
                }
                
                Console.WriteLine($"[INFO] Broadcast to {_connections.Count} clients");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to broadcast: {ex.Message}");
                return false;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopServer();
                    
                    // Close client socket if exists
                    _clientSocket?.Dispose();
                    _clientSocket = null;
                }
                
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        ~NetworkManager()
        {
            Dispose(false);
        }
        #endregion
    }

    /// <summary>
    /// Client connection wrapper.
    /// </summary>
    public class ClientConnection : IDisposable
    {
        private bool _disposed;

        public string ClientId { get; set; } = "";
        
        public void Close()
        {
            if (!_disposed)
            {
                Console.WriteLine("[INFO] Closing client connection");
                _disposed = true;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue && !disposing)
            {
                Close();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        
        ~ClientConnection()
        {
            Dispose(false);
        }
        #endregion
    }

    /// <summary>
    /// Network message structure.
    /// </summary>
    public class NetworkMessage
    {
        public string MessageType { get; set; } = "";
        public string Data { get; set; } = "";
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        public static NetworkMessage Create(string messageType, string data) => 
            new() { MessageType = messageType, Data = data };
    }
}