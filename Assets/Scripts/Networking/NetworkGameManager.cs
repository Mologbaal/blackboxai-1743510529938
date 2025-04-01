using FlaxEngine;
using FlaxEngine.Networking;
using System.Collections.Generic;

public class NetworkGameManager : Script
{
    [Header("References")]
    public NetworkManager NetworkManager;
    public GameManager GameManager;
    public PlanetGenerator PlanetGenerator;

    private Dictionary<int, UnitBase> _networkedUnits = new Dictionary<int, UnitBase>();
    private float _lastStateUpdateTime;

    public override void OnStart()
    {
        NetworkManager.OnUnitStateReceived += OnUnitStateUpdate;
        NetworkManager.OnPlanetStateReceived += OnPlanetStateUpdate;
        NetworkManager.OnGameInitReceived += OnGameInit;
    }

    public override void OnUpdate()
    {
        if (Time.GameTime - _lastStateUpdateTime > NetworkManager.NetworkUpdateRate)
        {
            if (NetworkManager.IsServer)
            {
                SendGameState();
            }
            _lastStateUpdateTime = Time.GameTime;
        }
    }

    private void SendGameState()
    {
        // Send planet state to all clients
        var planetPacket = new PlanetStatePacket
        {
            TerritoryData = PlanetGenerator.GetTerritoryData(),
            ResourceNodes = PlanetGenerator.GetResourceNodePositions()
        };
        NetworkManager.SendToAll(planetPacket);

        // Send unit states to all clients
        foreach (var faction in GameManager.Factions)
        {
            foreach (var unit in faction.Units)
            {
                var networkUnit = unit.Actor.GetScript<NetworkUnit>();
                if (networkUnit != null)
                {
                    NetworkManager.SendToAll(networkUnit.GetNetworkState());
                }
            }
        }
    }

    private void OnUnitStateUpdate(UnitStatePacket packet)
    {
        if (NetworkManager.IsServer) return;

        if (!_networkedUnits.TryGetValue(packet.UnitID, out var unit))
        {
            // Create new unit if it doesn't exist
            var faction = GameManager.GetFaction((FactionType)packet.Faction);
            unit = faction.CreateUnit((UnitType)packet.UnitType);
            unit.Actor.AddScript<NetworkUnit>().IsLocal = false;
            _networkedUnits.Add(packet.UnitID, unit);
        }

        unit.GetScript<NetworkUnit>().UpdateFromNetwork(packet);
    }

    private void OnPlanetStateUpdate(PlanetStatePacket packet)
    {
        if (NetworkManager.IsServer) return;

        // Update planet territory and resources
        PlanetGenerator.ApplyTerritoryData(packet.TerritoryData);
        PlanetGenerator.UpdateResourceNodes(packet.ResourceNodes);
    }

    private void OnGameInit(GameInitPacket packet)
    {
        // Initialize player's faction
        var faction = GameManager.GetFaction((FactionType)packet.Faction);
        GameManager.SetPlayerFaction(faction);
        faction.StartingPosition = packet.SpawnPosition;
    }

    public void RegisterUnit(UnitBase unit)
    {
        var networkUnit = unit.Actor.GetOrAddScript<NetworkUnit>();
        networkUnit.Unit = unit;
        networkUnit.IsLocal = NetworkManager.IsLocalPlayer;
        networkUnit.NetworkID = unit.Actor.GetHashCode();
        _networkedUnits.Add(networkUnit.NetworkID, unit);
    }

    public override void OnDestroy()
    {
        NetworkManager.OnUnitStateReceived -= OnUnitStateUpdate;
        NetworkManager.OnPlanetStateReceived -= OnPlanetStateUpdate;
        NetworkManager.OnGameInitReceived -= OnGameInit;
    }
}