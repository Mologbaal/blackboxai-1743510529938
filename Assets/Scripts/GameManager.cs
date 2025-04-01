using FlaxEngine;

public class GameManager : Script
{
    [Serialize] public FactionBase[] Factions;
    [Serialize] public PlanetGenerator PlanetGenerator;
    [Serialize] public HudSystem HudSystem;

    private FactionBase _currentFaction;
    private bool _gameStarted;

    public override void OnStart()
    {
        Instance = this;
        
        // Initialize networking if available
        _networkGameManager = Actor.GetScript<NetworkGameManager>();
        if (_networkGameManager != null && _networkGameManager.NetworkManager != null)
        {
            _networkGameManager.NetworkManager.Initialize();
        }

        // Initialize game systems
        PlanetGenerator.Initialize();
        HudSystem.Initialize(this);
    }

    public void StartGame(FactionType selectedFaction)
    {
        _currentFaction = Factions[(int)selectedFaction];
        _gameStarted = true;
        
        // Register faction with network manager if multiplayer
        if (IsMultiplayer)
        {
            _networkGameManager.RegisterFaction(_currentFaction);
        }

        // Spawn initial units
        _currentFaction.SpawnStartingUnits();
        
        // Focus camera on starting position
        Camera.MainActor.Position = _currentFaction.StartingPosition;

        // Show HUD
        HudSystem.ShowHud();
    }

    public FactionBase GetFaction(FactionType type)
    {
        return Factions[(int)type];
    }

    public UnitBase GetUnitByID(int networkID)
    {
        if (_networkGameManager != null)
        {
            return _networkGameManager.GetUnit(networkID);
        }
        return null;
    }

    public void SetPlayerFaction(FactionBase faction)
    {
        _currentFaction = faction;
    }

    public override void OnUpdate()
    {
        if (!_gameStarted) return;
        
        // Main game loop
        _currentFaction.Update();
        PlanetGenerator.Update();
    }
}

public enum FactionType
{
    Human,
    Arachnid,
    Reptilian
}