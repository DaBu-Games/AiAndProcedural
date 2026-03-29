using UnityEngine;

public class CaveCellularAutomata
{
    private CellType[,] _cave;
    private CellType[,] _tempCave;
    
    private readonly CaveGenerationValues _values;

    public CaveCellularAutomata(CaveGenerationValues values)
    {
        _values = values;
    }
    
    public CellType[,] GetCave() => _cave;

    public void StartCellularAutomata(CellType[,] cave)
    {
        _cave = (CellType[,])cave.Clone();
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
            
                int rule1 = GetWallsCount(position, 1);
                int rule2 = GetWallsCount(position, 2);
            
                if(rule1 >= _values.AutomataWallTreshhold || rule2 <= _values.AutomataOpenSpaceThreshold)
                    _tempCave[x, y] = CellType.Wall;
                else
                    _tempCave[x, y] = CellType.Floor;
            }
        }
    }

    private int GetWallsCount(Vector2Int pos, int step)
    {
        int count = 0;
        
        for (int x = -step; x <= step; x++)
        {
            for (int y = -step; y <= step; y++)
            {
                if(x == 0 && y == 0)
                    continue;
                
                int posX = pos.x + x;
                int posY = pos.y + y;

                if (CaveRandomWalk.IsOutOfBounds(new Vector2Int(posX, posY), _cave))
                {
                    count++;
                    continue;
                }
                
                if(_cave[posX, posY] != CellType.Floor)
                    count++;
            }
        }
        
        return count;
    }
}
