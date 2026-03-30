using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CaveRandomWalk
{
    private CellType[,] _cave;
    
    private readonly CaveGenerationValues _values;
    
    public CaveRandomWalk(CaveGenerationValues values) => _values = values;
    
    public CellType[,] GetCave() => _cave;

    public void StartRandomWalk(Vector2Int caveSize)
    {
        _cave = new CellType[caveSize.x, caveSize.y];
        
        FillCave();
        RandomWalk();
    }
    
    private void FillCave()
    {
        for (int x = 0; x < _cave.GetLength(0); x++)
        {
            for (int y = 0; y < _cave.GetLength(1); y++)
            {
                _cave[x, y] = CellType.Wall;
            }
        }
    }
    
    private void RandomWalk()
    {
        int changedCells = 0;
        
        int maxCells = _cave.GetLength(0) * _cave.GetLength(1);
        int minCells = (int)(maxCells * _values.TurnedCellsPercentage);

        Vector2Int currentCell = GetRandomPosition();

        int maxIterations = minCells * 10;
        int iterations = 0;
        
        while (changedCells < minCells && iterations < maxIterations)
        {
            CellType currentType = _cave[currentCell.x, currentCell.y];

            if (currentType != CellType.Floor)
            {
                changedCells++;
                _cave[currentCell.x, currentCell.y] = CellType.Floor;
            }

            currentCell = GetRandomNeighbour(currentCell);
            iterations++;
        }
    }
    
    private Vector2Int GetRandomPosition() => new (Random.Range(0, _cave.GetLength(0)), Random.Range(0, _cave.GetLength(1)));

    private Vector2Int GetRandomNeighbour(Vector2Int pos)
    {
        List<Vector2Int> validNeighbours = new List<Vector2Int>();

        foreach (var dir in CaveManager.Directions)
        {
            Vector2Int nextCell = pos + dir;
            
            if (!CaveManager.IsOutOfBounds(nextCell, _cave))
                validNeighbours.Add(nextCell);
        }
        
        if(validNeighbours.Count == 0)
            return pos;
        
        return validNeighbours[Random.Range(0, validNeighbours.Count)];
    }
}
