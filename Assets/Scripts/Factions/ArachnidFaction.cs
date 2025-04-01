using FlaxEngine;
using System.Collections.Generic;

public class ArachnidFaction : FactionBase
{
    [Header("Arachnid Units")]
    public UnitBase SpiderlingPrefab;
    public UnitBase WebweaverPrefab;
    public UnitBase QueenPrefab;

    [Header("Arachnid Structures")]
    public StructureBase WebNestPrefab;
    public StructureBase SilkFactoryPrefab;

    [Header("Web Mechanics")]
    public Material WebMaterial;
    public float WebSlowAmount = 0.5f; // 50% speed reduction
    public List<Actor> ActiveWebs = new List<Actor>();

    public override string FactionName => "Arachnid Swarm";
    public override Color FactionColor => Color.Green;

    public override void Initialize()
    {
        StartingPosition = new Vector3(50, 0, 50);
        
        // Spawn initial web nest
        var nest = Actor.New<StructureBase>();
        nest.Position = StartingPosition;
        Structures.Add(nest);
    }

    public override void Update()
    {
        // Update all arachnid units
        foreach (var unit in Units)
        {
            unit.Update();
        }

        // Decay old webs
        for (int i = ActiveWebs.Count - 1; i >= 0; i--)
        {
            if (ActiveWebs[i] == null)
            {
                ActiveWebs.RemoveAt(i);
            }
        }
    }

    public override void SpawnStartingUnits()
    {
        // Spawn 8 spiderlings around nest
        for (int i = 0; i < 8; i++)
        {
            var spawnPos = StartingPosition + new Vector3(
                Random.Range(-15, 15),
                0,
                Random.Range(-15, 15)
            );
            SpawnUnitAt(SpiderlingPrefab, spawnPos);
        }

        // Spawn 1 webweaver
        SpawnUnitAt(WebweaverPrefab, StartingPosition + Vector3.Forward * 5);
    }

    public override UnitBase CreateUnit(UnitType unitType)
    {
        return unitType switch
        {
            UnitType.Spiderling => SpiderlingPrefab,
            UnitType.Webweaver => WebweaverPrefab,
            UnitType.Queen => QueenPrefab,
            _ => null
        };
    }

    public void CreateWebTrap(Vector3 position, float radius)
    {
        var webTrap = Actor.New<StaticModel>();
        webTrap.Position = position;
        webTrap.Scale = new Vector3(radius, 0.1f, radius);
        webTrap.StaticModel.Model = Content.LoadAsync<Model>("Base/Sphere");
        webTrap.StaticModel.Material = WebMaterial;
        
        var collider = webTrap.AddChild<SphereCollider>();
        collider.Radius = radius;
        collider.IsTrigger = true;
        
        ActiveWebs.Add(webTrap);
        
        // Web trap will automatically destroy after 30 seconds
        Destroy(webTrap, 30f);
    }

    // Called when enemy unit enters web
    public void OnWebTriggerEnter(UnitBase unit)
    {
        if (unit.Faction != this)
        {
            unit.Movement.MoveSpeed *= (1 - WebSlowAmount);
        }
    }

    // Called when enemy unit exits web
    public void OnWebTriggerExit(UnitBase unit)
    {
        if (unit.Faction != this)
        {
            unit.Movement.MoveSpeed /= (1 - WebSlowAmount);
        }
    }
}