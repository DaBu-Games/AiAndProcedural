using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class CaveRandomWalk
{
    private Vector2Int _gridSize;
    private CellType[,] _cave;
    private List<OreDeposit> _oreDeposits = new List<OreDeposit>();
    
    private readonly CaveGenerationValues _values;
    
    public CaveRandomWalk(Vector2Int gridSize, CaveGenerationValues values)
    {
        _gridSize = gridSize;
        _values = values;
    }
    
    public CellType[,] GetCave() => _cave;

    public void StartRandomWalk()
    {
        FillCave();
        GenerateOreDeposits();
        RandomWalk();
    }
    
    private void FillCave()
    {
        _cave = new CellType[_gridSize.x, _gridSize.y];

        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                _cave[x, y] = CellType.Wall;
            }
        }
    }

    private void GenerateOreDeposits()
    {
        int oreDeposits = Random.Range(_values.MinOreDeposits, _values.MaxOreDeposits);

        for (int i = 0; i < oreDeposits; i++)
        {
            MakeRandomOreDeposit();
        }
    }

    private void MakeRandomOreDeposit()
    {
        Vector2Int pos = GetRandomPosition();
        int depositRadius = Random.Range(_values.MinOreDepositRadius, _values.MaxOreDepositRadius);
        
        List<Vector2Int> orePositions = new List<Vector2Int>();
        HashSet<OreDeposit> overlappingDeposits = new HashSet<OreDeposit>();

        for (int x = -depositRadius; x <= depositRadius; x++)
        {
            for (int y = -depositRadius; y <= depositRadius; y++)
            {
                int dx = x + pos.x;
                int dy = y + pos.y;
                
                if(dx < 0 || dy < 0 || dx >= _gridSize.x || dy >= _gridSize.y)
                    continue;
                
                float distance = Mathf.Sqrt(x * x + y * y);
                if(distance > depositRadius)
                    continue;
                
                Vector2Int position = new Vector2Int(dx, dy);
                
                if (_cave[dx, dy] == CellType.Ore)
                {
                    OreDeposit existingDeposit = GetExistingOreDeposit(position);
                    if(existingDeposit != null)
                        overlappingDeposits.Add(existingDeposit);
                }
                else if (_cave[dx, dy] == CellType.Wall)
                {
                    orePositions.Add(position);
                    _cave[dx, dy] = CellType.Ore;
                }
            }
        }

        if (overlappingDeposits.Count > 0)
        {
            OreDeposit mergedDeposit = MergeOreDeposits(overlappingDeposits);
            mergedDeposit.AddOrePositions(orePositions);

            foreach (var deposit in overlappingDeposits)
            {
                _oreDeposits.Remove(deposit);
            }
            
            _oreDeposits.Add(mergedDeposit);
        }
        else
        {
            OreDeposit deposit = new OreDeposit();
            deposit.AddOrePositions(orePositions);
            _oreDeposits.Add(deposit);
        }
    }

    private OreDeposit GetExistingOreDeposit(Vector2Int pos)
    {
        foreach (var deposit in _oreDeposits)
        {
            if (deposit.HasOrePosition(pos))
            {
                return deposit;
            }
        }
        
        return null;
    }

    private OreDeposit MergeOreDeposits(HashSet<OreDeposit> depositsToMerge)
    {
        OreDeposit mergedDeposit = new OreDeposit();
        
        foreach (var deposit in depositsToMerge)
        {
            mergedDeposit.AddOrePositions(deposit.GetOrePositions());
        }
        
        return mergedDeposit;
    }
    
    private Vector2Int GetRandomPosition() => new Vector2Int(Random.Range(0, _gridSize.x), Random.Range(0, _gridSize.y));
    
    private void RandomWalk()
    {
        int changedCells = 0;
        
        int maxCells = _gridSize.x * _gridSize.y;
        int minTurnedCells = (int)(maxCells * _values.MinTurnedCellsPercentage);
        int maxTurnedCells = (int)(maxCells * _values.MaxTurnedCellsPercentage);
        int minCells = Random.Range(minTurnedCells, maxTurnedCells);

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

                if (currentType == CellType.Ore)
                {
                    GetExistingOreDeposit(currentCell).SetExposed();
                }
            }

            currentCell = GetRandomNeighbour(currentCell);
            iterations++;
        }
    }

    private Vector2Int GetRandomNeighbour(Vector2Int pos)
    {
        int direction = Random.Range(0, 4);
    
        Vector2Int nextCell = pos;
    
        switch (direction)
        {
            case 0:
                nextCell.y++;
                break;
            case 1:
                nextCell.y--;
                break;
            case 2:
                nextCell.x--;
                break;
            case 3:
                nextCell.x++;
                break;
        }
        
        nextCell.x = Mathf.Clamp(nextCell.x, 0, _gridSize.x -1);
        nextCell.y = Mathf.Clamp(nextCell.y, 0, _gridSize.y -1);
    
        return nextCell;
    }

    public void CarveOutOreDeposits()
    {
        foreach (var deposit in _oreDeposits)
        {
            if (!deposit.IsExposed())
                continue;

            foreach (var position in deposit.GetOrePositions())
            {
                _cave[position.x, position.y] = CellType.Floor;
            }
        }
    }
}
