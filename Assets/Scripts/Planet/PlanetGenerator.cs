using FlaxEngine;
using System.Collections.Generic;

public class PlanetGenerator : Script
{
    [Header("Generation Settings")]
    public int MapSize = 256;
    public float TerrainHeight = 50f;
    public float NoiseScale = 0.1f;
    public int Seed = 0;

    [Header("Territory System")] 
    public int TerritoryGridSize = 16;
    public Material NeutralTerritoryMaterial;
    public Material[] FactionTerritoryMaterials;

    [Header("References")]
    public Terrain Terrain;
    public Prefab ResourceNodePrefab;

    private Texture2D _territoryTexture;
    private Color32[] _territoryPixels;
    private FactionBase[] _factions;
    private List<Actor> _resourceNodes = new List<Actor>();

    public void Initialize()
    {
        // Initialize random seed
        if (Seed == 0) Seed = Random.Range(1, int.MaxValue);
        Random.InitState(Seed);

        // Create territory control texture
        _territoryTexture = Texture2D.New();
        _territoryTexture.Init(
            width: TerritoryGridSize,
            height: TerritoryGridSize,
            format: PixelFormat.R8G8B8A8_UNorm
        );
        _territoryPixels = new Color32[TerritoryGridSize * TerritoryGridSize];

        GenerateTerrain();
        GenerateResources();
    }

    private void GenerateTerrain()
    {
        if (Terrain == null) return;

        // Generate heightmap using Perlin noise
        var heightmap = Terrain.Heightmap;
        var heightmapData = new float[heightmap.Width * heightmap.Height];

        for (int y = 0; y < heightmap.Height; y++)
        {
            for (int x = 0; x < heightmap.Width; x++)
            {
                float xCoord = (float)x / heightmap.Width * NoiseScale;
                float yCoord = (float)y / heightmap.Height * NoiseScale;

                float height = Mathf.PerlinNoise(xCoord + Seed, yCoord + Seed) * TerrainHeight;
                heightmapData[y * heightmap.Width + x] = height;
            }
        }

        heightmap.SetData(heightmapData);
    }

    private void GenerateResources()
    {
        // Generate random resource nodes
        int nodeCount = MapSize / 10;
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(0, MapSize),
                0,
                Random.Range(0, MapSize)
            );

            // Sample terrain height at this position
            position.Y = Terrain.GetHeightAt(position.X, position.Z) + 1f;

            var node = PrefabManager.SpawnPrefab(ResourceNodePrefab, Actor);
            node.Position = position;
            _resourceNodes.Add(node);
        }
    }

    public void UpdateTerritory(Vector3 worldPosition, FactionBase faction)
    {
        if (faction == null) return;

        // Convert world position to territory grid coordinates
        int gridX = Mathf.FloorToInt(worldPosition.X / MapSize * TerritoryGridSize);
        int gridY = Mathf.FloorToInt(worldPosition.Z / MapSize * TerritoryGridSize);
        gridX = Mathf.Clamp(gridX, 0, TerritoryGridSize - 1);
        gridY = Mathf.Clamp(gridY, 0, TerritoryGridSize - 1);

        // Update territory pixel
        int pixelIndex = gridY * TerritoryGridSize + gridX;
        _territoryPixels[pixelIndex] = faction.FactionColor;

        // Update texture
        _territoryTexture.SetData(_territoryPixels);
    }

    public float GetFactionTerritoryPercentage(FactionBase faction)
    {
        if (faction == null) return 0f;

        int factionPixels = 0;
        Color32 factionColor = faction.FactionColor;

        foreach (var pixel in _territoryPixels)
        {
            if (pixel.Equals(factionColor))
            {
                factionPixels++;
            }
        }

        return (float)factionPixels / _territoryPixels.Length;
    }

    public Vector3 GetRandomSpawnPosition(FactionBase faction)
    {
        // Find all territory cells belonging to this faction
        var factionCells = new List<Vector2Int>();
        Color32 factionColor = faction.FactionColor;

        for (int y = 0; y < TerritoryGridSize; y++)
        {
            for (int x = 0; x < TerritoryGridSize; x++)
            {
                if (_territoryPixels[y * TerritoryGridSize + x].Equals(factionColor))
                {
                    factionCells.Add(new Vector2Int(x, y));
                }
            }
        }

        if (factionCells.Count == 0)
        {
            return faction.StartingPosition;
        }

        // Pick random territory cell
        var randomCell = factionCells[Random.Range(0, factionCells.Count)];

        // Convert back to world position
        return new Vector3(
            randomCell.x * (MapSize / TerritoryGridSize),
            0,
            randomCell.y * (MapSize / TerritoryGridSize)
        );
    }
}