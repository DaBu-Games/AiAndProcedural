using System.Collections.Generic;
using UnityEngine;

public class CaveGenerateDeposits
{
    private CellType[,] _cave;
    
    private readonly CaveGenerationValues _values;
    
    public CaveGenerateDeposits(CaveGenerationValues values) => _values = values;
    
    public CellType[,] GetCave() => _cave;
    
    private int shownDeposits = 0;
    
    public void GenerateOreDeposits(CellType[,] cave)
    {
        _cave = cave;
        
        MakeRandomOreDeposits(Random.Range(_values.MinOreDeposits, _values.MaxOreDeposits));
       
        
        if(shownDeposits < _values.MinOreDeposits)
            MakeRandomOreDeposits(_values.MinOreDeposits - shownDeposits);
        
        Debug.Log($"Shown deposits: {shownDeposits}");
    }

    private void MakeRandomOreDeposits(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            MakeRandomOreDeposit();
        }
    }

    private Vector2Int GetRandomPosition()
    {
        int minX = (int)(_cave.GetLength(0) * _values.OreAwayFromBorderPercentage);
        int minY = (int)(_cave.GetLength(1) * _values.OreAwayFromBorderPercentage);
        int maxX = _cave.GetLength(0) - minX;
        int maxY = _cave.GetLength(1) - minY;
        
        return new Vector2Int(Random.Range(minX, maxX), Random.Range(minY, maxY));
    }

    private void MakeRandomOreDeposit()
    {
        Vector2Int pos = GetRandomPosition();
        int depositRadius = Random.Range(_values.MinOreDepositRadius, _values.MaxOreDepositRadius);
        
        List<Vector2Int> orePositions = new List<Vector2Int>();
        bool isExposed = false;

        for (int x = -depositRadius; x <= depositRadius; x++)
        {
            for (int y = -depositRadius; y <= depositRadius; y++)
            {
                int depositX = x + pos.x;
                int depositY = y + pos.y;
                Vector2Int position = new Vector2Int(depositX, depositY);
                
                if(CaveManager.IsOutOfBounds(position, _cave))
                    continue;
                
                float distance = Mathf.Sqrt(x * x + y * y);
                if(distance > depositRadius)
                    continue;
                
                if(_cave[depositX, depositY] != CellType.Wall)
                    isExposed = true;
                
                orePositions.Add(position);
            }
        }

        if (!isExposed)
            return;
        
        if(orePositions.Count == 0)
            return;

        shownDeposits++;
        foreach (var orePosition in orePositions)
        {
            _cave[orePosition.x, orePosition.y] = CellType.Ore;
        }
    }
}
