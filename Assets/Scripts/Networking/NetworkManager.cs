using FlaxEngine;
using FlaxEngine.Networking;

public class NetworkManager : Script
{
    [Header("Network Settings")]
    public int Port = 7777;
    public string DefaultServerIP = "127.0.0.1";
    public float NetworkUpdateRate = 0.1f; // seconds

    [Header("References")]
    public GameManager GameManager;
    public PlanetGenerator PlanetGenerator;

    private NetworkClient _client;
    private NetworkHost _host;
    private float _lastNetworkUpdate;
    private bool _isServer;

    public override void OnStart()
    {
        // Initialize networking
        Network.Initialize(new NetworkConfig
        {
            Address = DefaultServerIP,
            Port = Port,
            ConnectionsLimit = 4
        });
    }

    public void HostGame(string playerName)
    {
        _host = Network.Host();
        _isServer = true;
        Debug.Log("Server started on port " + Port);
        
        // Add host as first player
        var joinMessage = new PlayerJoinMessage
        {
            PlayerName = playerName,
            IsHost = true
        };
        OnPlayerJoin(joinMessage, Network.LocalConnection);
    }

    public void JoinGame(string ipAddress, string playerName)
    {
        _client = Network.Connect(ipAddress, Port);
        _isServer = false;
        Debug.Log("Connecting to " + ipAddress + ":" + Port);

        // Send join message after connection is established
        _client.Connected += () => {
            var joinMessage = new PlayerJoinMessage
            {
                PlayerName = playerName,
                IsHost = false
            };
            Send(joinMessage);
        };
    }

    public override void OnUpdate()
    {
        if (Time.GameTime - _lastNetworkUpdate < NetworkUpdateRate)
            return;

        _lastNetworkUpdate = Time.GameTime;

        if (_isServer)
        {
            SyncGameState();
        }
    }

    private void SyncGameState()
    {
        // Sync planet state
        var planetPacket = new PlanetStatePacket
        {
            TerritoryData = PlanetGenerator.GetTerritoryData(),
            ResourceNodes = PlanetGenerator.GetResourceNodePositions()
        };
        Network.Send(planetPacket, NetworkChannelType.Reliable);

        // Sync units
        foreach (var faction in GameManager.Factions)
        {
            foreach (var unit in faction.Units)
            {
                var unitPacket = new UnitStatePacket
                {
                    UnitID = unit.NetworkID,
                    Position = unit.Position,
                    Rotation = unit.Rotation,
                    Health = unit.Health,
                    Faction = (byte)faction.FactionType
                };
                Network.Send(unitPacket, NetworkChannelType.Unreliable);
            }
        }
    }

    [NetworkRpc]
    public void OnPlayerConnected(NetworkConnection connection)
    {
        if (!_isServer) return;

        // Send current lobby state to new player
        foreach (var player in _lobbyPlayers)
        {
            var joinMessage = new PlayerJoinMessage
            {
                PlayerName = player.Value.Name,
                IsHost = player.Key == Network.LocalConnection
            };
            Send(joinMessage, connection);
        }
    }

    private void OnPlayerJoin(PlayerJoinMessage message, NetworkConnection connection)
    {
        if (_isServer)
        {
            // Assign faction to new player
            var faction = GameManager.GetAvailableFaction();
            _lobbyPlayers[connection] = new PlayerInfo
            {
                Name = message.PlayerName,
                Faction = faction,
                IsReady = false
            };

            // Notify all players
            Send(message);
            Send(new PlayerFactionMessage
            {
                PlayerName = message.PlayerName,
                Faction = (byte)faction
            });
        }

        LobbyController?.AddPlayer(connection, message.PlayerName);
    }

    private void OnPlayerReady(PlayerReadyMessage message, NetworkConnection connection)
    {
        if (_isServer && _lobbyPlayers.TryGetValue(connection, out var player))
        {
            player.IsReady = message.IsReady;
            Send(message);
        }

        LobbyController?.UpdatePlayerReadyState(connection, message.IsReady);
    }

    private void OnPlayerFaction(PlayerFactionMessage message, NetworkConnection connection)
    {
        if (_isServer && _lobbyPlayers.TryGetValue(connection, out var player))
        {
            player.Faction = (FactionType)message.Faction;
            Send(message);
        }

        LobbyController?.UpdatePlayerFaction(connection, (FactionType)message.Faction);
    }

    private void OnStartGame(StartGameMessage message, NetworkConnection connection)
    {
        if (_isServer)
        {
            // Initialize game for all players
            foreach (var player in _lobbyPlayers)
            {
                var initPacket = new GameInitPacket
                {
                    Faction = (byte)player.Value.Faction,
                    SpawnPosition = PlanetGenerator.GetFactionSpawnPosition(player.Value.Faction)
                };
                Send(initPacket, player.Key);
            }
        }

        LobbyController?.HideLobby();
        GameManager.StartGame((FactionType)message.Faction);
    }

    public void SendReadyState(bool isReady)
    {
        Send(new PlayerReadyMessage { IsReady = isReady });
    }

    public void SendFactionChange(FactionType faction)
    {
        Send(new PlayerFactionMessage { Faction = (byte)faction });
    }

    public void StartGame()
    {
        if (_isServer)
        {
            Send(new StartGameMessage());
        }
    }

    private Dictionary<NetworkConnection, PlayerInfo> _lobbyPlayers = new Dictionary<NetworkConnection, PlayerInfo>();

    private class PlayerInfo
    {
        public string Name;
        public FactionType Faction;
        public bool IsReady;
    }

    [NetworkRpc]
    public void OnPlayerDisconnected(NetworkConnection connection)
    {
        // Handle player disconnection
        var faction = GameManager.GetFactionByConnection(connection);
        if (faction != null)
        {
            GameManager.RemoveFaction(faction);
        }
    }

    public override void OnDestroy()
    {
        Network.Shutdown();
    }
}

// Network packet structures
public struct UnitStatePacket : INetworkSerializable
{
    public int UnitID;
    public Vector3 Position;
    public Quaternion Rotation;
    public float Health;
    public byte Faction;

    public void Serialize(ref NetworkMessage message)
    {
        message.WriteInt32(UnitID);
        message.WriteVector3(Position);
        message.WriteQuaternion(Rotation);
        message.WriteFloat(Health);
        message.WriteByte(Faction);
    }

    public void Deserialize(ref NetworkMessage message)
    {
        UnitID = message.ReadInt32();
        Position = message.ReadVector3();
        Rotation = message.ReadQuaternion();
        Health = message.ReadFloat();
        Faction = message.ReadByte();
    }
}

public struct PlanetStatePacket : INetworkSerializable
{
    public byte[] TerritoryData;
    public Vector3[] ResourceNodes;

    public void Serialize(ref NetworkMessage message)
    {
        message.WriteBytes(TerritoryData);
        message.WriteArray(ResourceNodes);
    }

    public void Deserialize(ref NetworkMessage message)
    {
        TerritoryData = message.ReadBytes();
        ResourceNodes = message.ReadArray<Vector3>();
    }
}

public struct GameInitPacket : INetworkSerializable
{
    public byte Faction;
    public Vector3 SpawnPosition;

    public void Serialize(ref NetworkMessage message)
    {
        message.WriteByte(Faction);
        message.WriteVector3(SpawnPosition);
    }

    public void Deserialize(ref NetworkMessage message)
    {
        Faction = message.ReadByte();
        SpawnPosition = message.ReadVector3();
    }
}