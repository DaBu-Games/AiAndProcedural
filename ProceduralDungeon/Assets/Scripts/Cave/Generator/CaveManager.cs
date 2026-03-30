using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class CaveManager : MonoBehaviour
{
    [SerializeField] private int seed;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private CaveGenerationValues values;
    [SerializeField] private CaveVisualisation caveVisualisation;

    private void Start()
    {
        GenerateCave();
    }

    private void GenerateCave()
    {
        Random.InitState(seed);
        CaveRandomWalk randomWalk = new CaveRandomWalk(values);
        randomWalk.StartRandomWalk(gridSize);
        
        CaveCellularAutomata automata = new CaveCellularAutomata(values);
        automata.StartCellularAutomata(randomWalk.GetCave());
        
        CaveGenerateDeposits deposits = new CaveGenerateDeposits(values);
        deposits.GenerateOreDeposits(automata.GetCave());

        CaveFloodFill caveFloodFill = new CaveFloodFill(values);
        caveFloodFill.StartFloodFill(deposits.GetCave());

        caveVisualisation.SetCave(caveFloodFill.GetCave());
    }
    
    public static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };
    
    public static bool IsOutOfBounds(Vector2Int pos, CellType[,] cave)
    {
        return pos.x < 0 || pos.x >= cave.GetLength(0) || pos.y < 0 || pos.y >= cave.GetLength(1);
    }
    
    public static int GetCellTypeCount(Vector2Int pos, int step, CellType[,] cave, CellType[] types)
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

                if (IsOutOfBounds(new Vector2Int(posX, posY), cave))
                {
                    count++;
                    continue;
                }
                
                if(types.Contains(cave[posX, posY]))
                    count++;
            }
        }
        
        return count;
    }
}
