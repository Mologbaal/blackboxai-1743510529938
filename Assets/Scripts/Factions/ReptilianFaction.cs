using FlaxEngine;
using System.Collections.Generic;

public class ReptilianFaction : FactionBase
{
    [Header("Reptilian Units")]
    public UnitBase LurkerPrefab;
    public UnitBase FlyerPrefab;
    public UnitBase WarbossPrefab;

    [Header("Reptilian Structures")]
    public StructureBase UndergroundBasePrefab;
    public StructureBase ChameleonTurretPrefab;

    [Header("Camouflage Mechanics")]
    public Material CamouflageMaterial;
    public float CamouflageDetectionReduction = 0.5f; // 50% less detection range
    public float CamouflageActivationDelay = 3f; // Seconds to become fully camouflaged

    private Dictionary<UnitBase, float> _camouflageTimers = new Dictionary<UnitBase, float>();

    public override string FactionName => "Reptilian Dominion";
    public override Color FactionColor => Color.Red;

    public override void Initialize()
    {
        StartingPosition = new Vector3(-50, 0, 0);
        
        // Spawn initial underground base
        var baseStructure = Actor.New<StructureBase>();
        baseStructure.Position = StartingPosition;
        Structures.Add(baseStructure);
    }

    public override void Update()
    {
        // Update all reptilian units
        foreach (var unit in Units)
        {
            unit.Update();
            UpdateCamouflage(unit);
        }
    }

    public override void SpawnStartingUnits()
    {
        // Spawn 3 lurkers (camouflaged scouts)
        for (int i = 0; i < 3; i++)
        {
            var spawnPos = StartingPosition + new Vector3(
                Random.Range(-20, 20),
                0,
                Random.Range(-20, 20)
            );
            var lurker = SpawnUnitAt(LurkerPrefab, spawnPos);
            ActivateCamouflage(lurker, true);
        }

        // Spawn 1 warboss
        SpawnUnitAt(WarbossPrefab, StartingPosition + Vector3.Backward * 10);
    }

    public override UnitBase CreateUnit(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Lurker => LurkerPrefab,
            UnitType.Flyer => FlyerPrefab,
            UnitType.Warboss => WarbossPrefab,
            _ => null
        };
    }

    public void ActivateCamouflage(UnitBase unit, bool immediate = false)
    {
        if (unit.Visuals == null) return;

        if (immediate)
        {
            unit.Visuals.ApplyCamouflage(CamouflageMaterial);
            _camouflageTimers[unit] = CamouflageActivationDelay;
        }
        else
        {
            _camouflageTimers[unit] = 0f; // Start counting up
        }
    }

    public void DeactivateCamouflage(UnitBase unit)
    {
        if (unit.Visuals == null || !_camouflageTimers.ContainsKey(unit)) return;

        unit.Visuals.RemoveCamouflage();
        _camouflageTimers.Remove(unit);
    }

    private void UpdateCamouflage(UnitBase unit)
    {
        if (!_camouflageTimers.TryGetValue(unit, out var timer)) return;

        // Gradually apply camouflage effect
        if (timer < CamouflageActivationDelay)
        {
            timer += Time.DeltaTime;
            _camouflageTimers[unit] = timer;

            float progress = timer / CamouflageActivationDelay;
            unit.Visuals.SetCamouflageProgress(progress);

            if (timer >= CamouflageActivationDelay)
            {
                unit.Visuals.ApplyCamouflage(CamouflageMaterial);
            }
        }

        // Auto-camouflage when stationary
        if (unit.Movement.ReachedDestination && !_camouflageTimers.ContainsKey(unit))
        {
            ActivateCamouflage(unit);
        }
        // Break camouflage when moving or attacking
        else if ((!unit.Movement.ReachedDestination || unit.Combat.HasValidTarget) 
                && _camouflageTimers.ContainsKey(unit))
        {
            DeactivateCamouflage(unit);
        }
    }

    public float GetDetectionRangeModifier(UnitBase unit)
    {
        return _camouflageTimers.ContainsKey(unit) ? 
            (1 - CamouflageDetectionReduction) : 1f;
    }
}