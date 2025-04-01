using FlaxEngine;
using FlaxEngine.GUI;

public class HudSystem : Script
{
    [Header("UI References")]
    public UIControl HudControl;
    public GameManager GameManager;

    [Header("Minimap Settings")] 
    public float MinimapUpdateInterval = 0.5f;
    public Material MinimapMaterial;

    private Text _mineralsText;
    private Text _energyText;
    private Text _populationText;
    private Text _unitNameText;
    private ProgressBar _healthBar;
    private Image _minimapImage;
    private float _lastMinimapUpdate;
    private UnitBase _selectedUnit;

    public override void OnStart()
    {
        if (HudControl == null || HudControl.Control == null)
        {
            Debug.LogError("HUD control not assigned!");
            return;
        }

        // Get UI elements references
        var canvas = HudControl.Get<Canvas>();
        _mineralsText = canvas.GetChild<Text>("MineralsText");
        _energyText = canvas.GetChild<Text>("EnergyText");
        _populationText = canvas.GetChild<Text>("PopulationText");
        _unitNameText = canvas.GetChild<Text>("UnitName");
        _healthBar = canvas.GetChild<ProgressBar>("HealthBar");
        _minimapImage = canvas.GetChild<Image>("MinimapImage");

        // Setup minimap
        if (MinimapMaterial != null)
        {
            _minimapImage.Brush = new MaterialBrush(MinimapMaterial);
        }

        // Hide HUD by default
        HudControl.IsActive = false;
    }

    public override void OnUpdate()
    {
        if (GameManager == null || !HudControl.IsActive) return;

        // Update resource displays
        if (_mineralsText != null) 
            _mineralsText.Text = GameManager.CurrentFaction.Minerals.ToString();
        if (_energyText != null)
            _energyText.Text = GameManager.CurrentFaction.Energy.ToString();
        if (_populationText != null)
            _populationText.Text = $"{GameManager.CurrentFaction.Units.Count}/{GameManager.CurrentFaction.MaxPopulation}";

        // Update selected unit info
        if (_selectedUnit != null)
        {
            _unitNameText.Text = _selectedUnit.GetType().Name;
            _healthBar.Value = _selectedUnit.Health / _selectedUnit.MaxHealth;
        }

        // Update minimap at intervals
        if (Time.GameTime - _lastMinimapUpdate > MinimapUpdateInterval)
        {
            UpdateMinimap();
            _lastMinimapUpdate = Time.GameTime;
        }
    }

    public void ShowHud()
    {
        HudControl.IsActive = true;
    }

    public void HideHud()
    {
        HudControl.IsActive = false;
    }

    public void SelectUnit(UnitBase unit)
    {
        _selectedUnit = unit;
        
        if (unit == null)
        {
            _unitNameText.Text = "No Selection";
            _healthBar.Value = 0;
        }
    }

    private void UpdateMinimap()
    {
        // TODO: Implement minimap rendering
        // This would update the minimap texture to show:
        // - Terrain features
        // - Unit positions
        // - Territory control
    }

    // Button event handlers
    public void OnAttackCommand()
    {
        if (_selectedUnit != null)
        {
            // TODO: Implement attack command mode
        }
    }

    public void OnMoveCommand()
    {
        if (_selectedUnit != null)
        {
            // TODO: Implement move command mode
        }
    }

    public void OnSpecialCommand()
    {
        if (_selectedUnit != null)
        {
            // TODO: Implement faction-specific special ability
        }
    }
}