using UnityEngine;

public class CaveCellularAutomata
{
    private CellType[,] _cave;
    private CellType[,] _tempCave;
    
    private readonly CaveGenerationValues _values;

    public CaveCellularAutomata(CaveGenerationValues values) => _values = values;
    
    public CellType[,] GetCave() => _cave;

    public void StartCellularAutomata(CellType[,] cave)
    {
        _cave = cave;
        _tempCave = new CellType[_cave.GetLength(0), _cave.GetLength(1)];

        for (int i = 0; i < _values.AutomataIterations; i++)
        {
            IterateThroughCave();
            (_cave, _tempCave) = (_tempCave, _cave);
        }
    }

    private void IterateThroughCave()
    {
        for (int x = 0; x < _cave.GetLength(0); x++)
        {
            for (int y = 0; y < _cave.GetLength(1); y++)
            {
                if (_cave[x, y] == CellType.Ore)
                {
                    _tempCave[x, y] = CellType.Ore;
                    continue;
                }
                    
                Vector2Int position = new Vector2Int(x, y);

                CellType[] lookUp = new[] { CellType.Wall };
            
                int rule1 = CaveManager.GetCellTypeCount(position, 1, _cave, lookUp);
                int rule2 = CaveManager.GetCellTypeCount(position, 2, _cave, lookUp);
            
                if(rule1 >= _values.AutomataWallTreshhold || rule2 <= _values.AutomataOpenSpaceThreshold)
                    _tempCave[x, y] = CellType.Wall;
                else
                    _tempCave[x, y] = CellType.Floor;
            }
        }
    }

    
}
