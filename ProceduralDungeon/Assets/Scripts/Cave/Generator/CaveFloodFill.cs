using System.Collections.Generic;
using UnityEngine;

public class CaveFloodFill
{
    private CellType[,] _cave;
    private int _regionAmount = 0;
    private int _entranceAmount = 0;
    
    private readonly CaveGenerationValues _values;
    
    public CaveFloodFill(CaveGenerationValues values) => _values = values;
    
    public CellType[,] GetCave() => _cave;

    public void StartFloodFill(CellType[,] cave)
    {
        _cave = cave;
        CheckRegions();
    }

    private void CheckRegions()
    {
        List<List<Vector2Int>> regions = GetRegions();
        
        _regionAmount = 0;
        int currentRegionIndex = 0;
        for(int i = 0; i < regions.Count; i++)
        {
            int amount = regions[i].Count;
            
            if(amount <= _regionAmount)
                continue;
            
            _regionAmount = amount;
            currentRegionIndex = i;
        }
        
        List<Vector2Int> mainRegion = regions[currentRegionIndex];
        regions.RemoveAt(currentRegionIndex);
        
        RemoveSmallerRegions(regions);
        CreateCaveContent(mainRegion);
    }

    private List<List<Vector2Int>> GetRegions()
    {
        bool[,] visited = new bool[_cave.GetLength(0), _cave.GetLength(1)];
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

        for (int x = 0; x < _cave.GetLength(0); x++)
        {
            for (int y = 0; y < _cave.GetLength(1); y++)
            {
                if(_cave[x, y] == CellType.Wall || visited[x, y])
                    continue;
                
                List<Vector2Int> region = new List<Vector2Int>();
                Queue<Vector2Int> queue = new Queue<Vector2Int>();
                
                queue.Enqueue(new Vector2Int(x, y));

                while (queue.Count > 0)
                {
                    Vector2Int current = queue.Dequeue();
                    region.Add(current);

                    foreach (var dir in CaveManager.Directions)
                    {
                        Vector2Int next = current + dir;
                        
                        if(CaveManager.IsOutOfBounds(next, _cave))
                            continue;
                        
                        if(visited[next.x, next.y] || _cave[next.x, next.y] == CellType.Wall)
                            continue;
                        
                        visited[next.x, next.y] = true;
                        queue.Enqueue(next);
                    }
                }
                regions.Add(region);
            }
        }
        
        return regions;
    }

    private void RemoveSmallerRegions( List<List<Vector2Int>> regions)
    {
        for (int i = 0; i < regions.Count; i++)
        {
            RemoveRegion(regions[i]);
        }
    }

    private void RemoveRegion(List<Vector2Int> region)
    {
        for (int i = 0; i < region.Count; i++)
        {
            _cave[region[i].x, region[i].y] = CellType.Wall;
        }
    }

    private void CreateCaveContent(List<Vector2Int> region)
    {
        foreach (var pos in region)
        {
            // Entrance
            if (IsOnEdge(pos) &&  CaveManager.GetCellTypeCount(pos, 1, _cave, new [] {CellType.Wall}) 
                <= _values.EntranceWallThreshhold)
            {
                _cave[pos.x, pos.y] = CellType.Entrance;
                _entranceAmount++;
            }
            // Place enemy
            else if (CaveManager.GetCellTypeCount(pos, 1, _cave, new []{CellType.Wall, CellType.Ore, CellType.Enemy}) 
                < _values.EnemyWallThreshhold)
            {
                _cave[pos.x, pos.y] = CellType.Enemy;
            }
        }
    }
    
    private bool IsOnEdge(Vector2Int pos)
    {
        return pos.x == 0 || pos.y == 0 || pos.x == _cave.GetLength(0) - 1 || pos.y == _cave.GetLength(1) - 1;
    }
}
