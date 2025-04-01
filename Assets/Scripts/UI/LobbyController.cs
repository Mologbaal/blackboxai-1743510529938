using FlaxEngine;
using FlaxEngine.GUI;
using System.Collections.Generic;

public class LobbyController : Script
{
    [Header("References")]
    public UIControl LobbyControl;
    public NetworkManager NetworkManager;
    public MainMenuScript MainMenu;

    [Header("UI Elements")]
    public Button ReadyButton;
    public Button StartButton;
    public Text[] PlayerNameTexts;
    public Text[] PlayerFactionTexts;

    private Dictionary<NetworkConnection, PlayerLobbyData> _players = new Dictionary<NetworkConnection, PlayerLobbyData>();
    private bool _isReady;

    public override void OnStart()
    {
        if (LobbyControl == null || LobbyControl.Control == null)
        {
            Debug.LogError("Lobby UI control not assigned!");
            return;
        }

        // Get UI references
        var canvas = LobbyControl.Get<Canvas>();
        ReadyButton = canvas.GetChild<Button>("ReadyButton");
        StartButton = canvas.GetChild<Button>("StartButton");

        // Initialize player slots
        PlayerNameTexts = new Text[4];
        PlayerFactionTexts = new Text[4];
        for (int i = 0; i < 4; i++)
        {
            var slot = canvas.GetChild<Panel>($"PlayerSlot{i+1}");
            PlayerNameTexts[i] = slot.GetChild<Text>($"Player{i+1}Name");
            PlayerFactionTexts[i] = slot.GetChild<Text>($"Player{i+1}Faction");
        }

        // Setup button events
        ReadyButton.Clicked += OnReadyClicked;
        StartButton.Clicked += OnStartClicked;

        // Hide lobby by default
        LobbyControl.IsActive = false;
    }

    public void ShowLobby(bool isHost)
    {
        LobbyControl.IsActive = true;
        StartButton.IsVisible = isHost;
        _isReady = false;
        ReadyButton.TextControl.Text = "Ready";
        UpdatePlayerList();
    }

    private void OnReadyClicked()
    {
        _isReady = !_isReady;
        ReadyButton.TextControl.Text = _isReady ? "Cancel" : "Ready";
        
        // Notify server of ready state
        NetworkManager.SendReadyState(_isReady);
    }

    private void OnStartClicked()
    {
        if (NetworkManager.IsServer)
        {
            // Start game for all players
            NetworkManager.StartGame();
        }
    }

    public void AddPlayer(NetworkConnection connection, string playerName)
    {
        _players[connection] = new PlayerLobbyData
        {
            Name = playerName,
            Faction = FactionType.None,
            IsReady = false
        };
        UpdatePlayerList();
    }

    public void RemovePlayer(NetworkConnection connection)
    {
        _players.Remove(connection);
        UpdatePlayerList();
    }

    public void UpdatePlayerFaction(NetworkConnection connection, FactionType faction)
    {
        if (_players.TryGetValue(connection, out var player))
        {
            player.Faction = faction;
            UpdatePlayerList();
        }
    }

    public void UpdatePlayerReadyState(NetworkConnection connection, bool isReady)
    {
        if (_players.TryGetValue(connection, out var player))
        {
            player.IsReady = isReady;
            UpdatePlayerList();
        }
    }

    private void UpdatePlayerList()
    {
        // Reset all slots
        for (int i = 0; i < 4; i++)
        {
            PlayerNameTexts[i].Text = "Empty Slot";
            PlayerNameTexts[i].Color = new Color(0.67f, 0.67f, 0.67f, 1f);
            PlayerFactionTexts[i].Text = "-";
            PlayerFactionTexts[i].Color = new Color(0.67f, 0.67f, 0.67f, 1f);
        }

        // Update with current players
        int index = 0;
        foreach (var player in _players.Values)
        {
            if (index >= 4) break;

            PlayerNameTexts[index].Text = player.Name + (player.IsReady ? " (Ready)" : "");
            PlayerNameTexts[index].Color = Color.White;
            
            PlayerFactionTexts[index].Text = player.Faction.ToString();
            PlayerFactionTexts[index].Color = GetFactionColor(player.Faction);

            index++;
        }
    }

    private Color GetFactionColor(FactionType faction)
    {
        return faction switch
        {
            FactionType.Human => new Color(0, 0.5f, 1f),
            FactionType.Arachnid => new Color(0, 1f, 0.5f),
            FactionType.Reptilian => new Color(1f, 0.5f, 0),
            _ => Color.White
        };
    }

    private class PlayerLobbyData
    {
        public string Name;
        public FactionType Faction;
        public bool IsReady;
    }
}