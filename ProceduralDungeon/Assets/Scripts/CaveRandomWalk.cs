using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CaveRandomWalk
{
    private CellType[,] _cave;
    private List<OreDeposit> _oreDeposits = new List<OreDeposit>();
    
    private readonly CaveGenerationValues _values;

    public static bool IsOutOfBounds(Vector2Int pos, CellType[,] cave)
    {
        return pos.x < 0 || pos.x >= cave.GetLength(0) || pos.y < 0 || pos.y >= cave.GetLength(1);
    }
    
    public CaveRandomWalk(CaveGenerationValues values)
    {
        _values = values;
    }
    
    public CellType[,] GetCave() => _cave;
    
    public void SetCave(CellType[,] cave) => _cave = cave;

    public void StartRandomWalk(Vector2Int caveSize)
    {
        _cave = new CellType[caveSize.x, caveSize.y];
        
        FillCave();
        GenerateOreDeposits();
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
                Vector2Int position = new Vector2Int(dx, dy);
                
                if(IsOutOfBounds(position, _cave))
                    continue;
                
                float distance = Mathf.Sqrt(x * x + y * y);
                if(distance > depositRadius)
                    continue;
                
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
    
    private Vector2Int GetRandomPosition() => new (Random.Range(0, _cave.GetLength(0)), Random.Range(0, _cave.GetLength(1)));
    
    private void RandomWalk()
    {
        int changedCells = 0;
        
        int maxCells = _cave.GetLength(0) * _cave.GetLength(1);
        int minTurnedCells = (int)(maxCells * _values.MinTurnedCellsPercentage);
        int maxTurnedCells = (int)(maxCells * _values.MaxTurnedCellsPercentage);
        int minCells = Random.Range(minTurnedCells, maxTurnedCells + 1);

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
                    GetExistingOreDeposit(currentCell)?.SetExposed();
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
        
        if (IsOutOfBounds(nextCell, _cave))
        {
            return pos;
        }
    
        return nextCell;
    }

    public void CarveOutOreDeposits()
    {
        foreach (var deposit in _oreDeposits)
        {
            CellType turnType = deposit.IsExposed() ? CellType.Floor : CellType.Wall;

            foreach (var position in deposit.GetOrePositions())
            {
                _cave[position.x, position.y] = turnType;
            }
        }
    }
}
