using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class CaveManager : MonoBehaviour
{
    [SerializeField] private int seed;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private CaveGenerationValues values;
    [SerializeField] private bool carveOut = false;
    
    private CaveRandomWalk _randomWalk;
    private CaveCellularAutomata _automata;
    private CellType[,] _currentCave;

    private void Start()
    {
        Random.InitState(seed);
        _randomWalk = new CaveRandomWalk(values);
        _randomWalk.StartRandomWalk(gridSize);
        _currentCave = _randomWalk.GetCave();
        
        _automata = new CaveCellularAutomata(values);
    }

    private void Update()
    {
        if (carveOut)
        {
            _automata.StartCellularAutomata(_randomWalk.GetCave());
            _randomWalk.SetCave(_automata.GetCave());
            _randomWalk.CarveOutOreDeposits();
            _currentCave = _automata.GetCave();
            carveOut = false;
        }
    }

    private void OnDrawGizmos()
    {
        if(_currentCave == null)
            return;
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Set color based on cell type
                switch (_currentCave[x, y])
                {
                    case CellType.Wall:
                        Gizmos.color = Color.black;
                        break;
                    case CellType.Floor:
                        Gizmos.color = Color.white;
                        break;
                    case CellType.Ore:
                        Gizmos.color = Color.blue;
                        break;
                }
                
                Vector3 pos = transform.position + new Vector3(x * cellSize, y * cellSize, 0);
                Gizmos.DrawCube(pos, Vector3.one * cellSize);
            }
        }
    }
}
