using FlaxEngine;
using FlaxEngine.GUI;

public class MainMenuScript : Script
{
    [Serialize] private UIControl _mainMenuControl;
    [Serialize] private GameManager _gameManager;

    private Button _humanButton;
    private Button _arachnidButton;
    private Button _reptilianButton;
    private Button _startButton;
    private FactionType _selectedFaction = FactionType.None;

    public override void OnStart()
    {
        if (_mainMenuControl == null || _mainMenuControl.Control == null)
        {
            Debug.LogError("Main menu UI control not assigned!");
            return;
        }

        // Get button references
        var canvas = _mainMenuControl.Get<Canvas>();
        _humanButton = canvas.GetChild<Button>("HumanButton");
        _arachnidButton = canvas.GetChild<Button>("ArachnidButton");
        _reptilianButton = canvas.GetChild<Button>("ReptilianButton");
        _startButton = canvas.GetChild<Button>("StartButton");

        // Setup button events
        _humanButton.Clicked += () => OnFactionSelected(FactionType.Human);
        _arachnidButton.Clicked += () => OnFactionSelected(FactionType.Arachnid);
        _reptilianButton.Clicked += () => OnFactionSelected(FactionType.Reptilian);
        _startButton.Clicked += OnStartGame;
    }

    private void OnFactionSelected(FactionType faction)
    {
        _selectedFaction = faction;

        // Update button highlights
        _humanButton.BackgroundColor = faction == FactionType.Human ? 
            new Color(0, 0.5f, 1f) : new Color(0, 0, 0.5f);
        
        _arachnidButton.BackgroundColor = faction == FactionType.Arachnid ? 
            new Color(0, 1f, 0.5f) : new Color(0, 0.5f, 0);
        
        _reptilianButton.BackgroundColor = faction == FactionType.Reptilian ? 
            new Color(1f, 0, 0.5f) : new Color(0.5f, 0, 0);

        // Enable start button
        _startButton.IsEnabled = true;
    }

    private void OnStartGame()
    {
        if (_selectedFaction == FactionType.None || _gameManager == null)
        {
            Debug.LogError("Cannot start game - no faction selected or game manager missing!");
            return;
        }

        // Hide menu
        _mainMenuControl.IsActive = false;

        // Start game with selected faction
        _gameManager.StartGame(_selectedFaction);
    }

    public void ShowMenu()
    {
        if (_mainMenuControl != null)
        {
            _mainMenuControl.IsActive = true;
            _selectedFaction = FactionType.None;
            
            // Reset button states
            if (_humanButton != null) _humanButton.BackgroundColor = new Color(0, 0, 0.5f);
            if (_arachnidButton != null) _arachnidButton.BackgroundColor = new Color(0, 0.5f, 0);
            if (_reptilianButton != null) _reptilianButton.BackgroundColor = new Color(0.5f, 0, 0);
            if (_startButton != null) _startButton.IsEnabled = false;
        }
    }
}