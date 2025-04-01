using FlaxEngine;

public class UnitVisuals : Script
{
    [Header("Visual Components")]
    public AnimatedModel Model;
    public AudioSource AudioSource;
    public ParticleSystem HitEffect;
    public ParticleSystem DeathEffect;

    [Header("Materials")]
    public Material DefaultMaterial;
    public Material CamouflageMaterial;
    public Material HighlightMaterial;

    private UnitBase _unit;
    private MaterialInstance _dynamicMaterial;
    private float _camouflageProgress = 0f;
    private bool _isCamouflaged = false;

    public void Initialize(UnitBase unit)
    {
        _unit = unit;
        
        // Setup model if not assigned
        if (Model == null) Model = Actor.GetChild<AnimatedModel>();
        if (Model != null && DefaultMaterial == null)
        {
            DefaultMaterial = Model.GetMaterial(0);
        }

        // Create dynamic material instance for effects
        if (DefaultMaterial != null)
        {
            _dynamicMaterial = DefaultMaterial.CreateVirtualInstance();
            Model.SetMaterial(0, _dynamicMaterial);
        }
    }

    public void UpdateVisuals()
    {
        UpdateCamouflageEffect();
        UpdateSelectionHighlight();
    }

    public void SetCamouflageProgress(float progress)
    {
        _camouflageProgress = progress;
        UpdateCamouflageEffect();
    }

    public void ApplyCamouflage(Material camouflageMaterial)
    {
        if (_dynamicMaterial == null) return;
        
        // Blend between default and camouflage materials
        _dynamicMaterial.SetMaterial(DefaultMaterial);
        _dynamicMaterial.SetMaterial(camouflageMaterial, 1);
        _dynamicMaterial.SetScalarParameterValue("BlendAmount", 1f);
        
        _isCamouflaged = true;
    }

    public void RemoveCamouflage()
    {
        if (_dynamicMaterial == null) return;
        
        _dynamicMaterial.SetScalarParameterValue("BlendAmount", 0f);
        _isCamouflaged = false;
        _camouflageProgress = 0f;
    }

    private void UpdateCamouflageEffect()
    {
        if (_dynamicMaterial == null || _isCamouflaged) return;
        
        // Gradually apply camouflage effect
        _dynamicMaterial.SetScalarParameterValue("BlendAmount", _camouflageProgress);
    }

    private void UpdateSelectionHighlight()
    {
        // TODO: Implement based on selection state
    }

    public void PlayHitEffect()
    {
        if (HitEffect != null)
        {
            HitEffect.Activate();
        }
    }

    public void PlayDeathEffect()
    {
        if (DeathEffect != null)
        {
            DeathEffect.Activate();
            DeathEffect.Parent = null; // Detach so it persists after unit destruction
        }
    }

    public void PlayAttackAnimation()
    {
        if (Model != null)
        {
            Model.PlayAnimation("Attack");
        }
    }

    public void PlayMovementAnimation(float speed)
    {
        if (Model != null)
        {
            Model.PlayAnimation(speed > 0.1f ? "Run" : "Idle");
            Model.AnimationSpeed = speed / _unit.MoveSpeed;
        }
    }
}