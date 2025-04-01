using FlaxEngine;
using System.Collections.Generic;

public class HumanFaction : FactionBase
{
    [Header("Human Units")]
    public UnitBase InfantryPrefab;
    public UnitBase TankPrefab;
    public UnitBase EngineerPrefab;

    [Header("Human Structures")] 
    public StructureBase BasePrefab;
    public StructureBase FactoryPrefab;

    public override string FactionName => "Human Alliance";
    public override Color FactionColor => Color.Blue;

    public override void Initialize()
    {
        StartingPosition = new Vector3(0, 0, -50);
        
        // Spawn initial base
        var baseStructure = Actor.New<StructureBase>();
        baseStructure.Position = StartingPosition;
        Structures.Add(baseStructure);
    }

    public override void Update()
    {
        // Update all human units
        foreach (var unit in Units)
        {
            unit.Update();
        }
    }

    public override void SpawnStartingUnits()
    {
        // Spawn 5 infantry units around base
        for (int i = 0; i < 5; i++)
        {
            var spawnPos = StartingPosition + new Vector3(
                Random.Range(-10, 10),
                0,
                Random.Range(-10, 10)
            );
            SpawnUnitAt(InfantryPrefab, spawnPos);
        }
    }

    public override UnitBase CreateUnit(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Infantry => InfantryPrefab,
            UnitType.Tank => TankPrefab,
            UnitType.Engineer => EngineerPrefab,
            _ => null
        };
    }
}