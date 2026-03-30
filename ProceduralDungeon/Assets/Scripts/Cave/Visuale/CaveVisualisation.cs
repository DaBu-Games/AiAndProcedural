using System.Collections.Generic;
using UnityEngine;

public class CaveVisualisation : MonoBehaviour
{
    [Header("Sprite settings")] 
    [SerializeField] private List<CellSprites> cellSprites = new List<CellSprites>();
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private float cellPadding;
    
    private Dictionary<CellType, Sprite> _spriteDictionary;
    private CellType[,] _cave;
    private float _spacing;

    private void Start()
    {
        _spriteDictionary = new Dictionary<CellType, Sprite>();

        foreach (var entry in cellSprites)
        {
            if (!_spriteDictionary.ContainsKey(entry.CellType))
                _spriteDictionary.Add(entry.CellType, entry.CellSprite);
        }
        
        _spacing = 1f + cellPadding;
    }

    public void SetCave(CellType[,] cave)
    {
        _cave = cave;
        UpdateCave();
        cameraManager.CenterOnGrid(new Vector2Int(_cave.GetLength(0), _cave.GetLength(1)));
    }

    private void UpdateCave()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        
        for (int x = 0; x < _cave.GetLength(0); x++)
        {
            for (int y = 0; y < _cave.GetLength(1); y++)
            {
                Sprite sprite = _spriteDictionary[_cave[x, y]];
                
                Vector3 position = new Vector3(x * _spacing, y * _spacing, 0f);
                
                GameObject obj = new GameObject($"Cave_{x}_{y}");
                obj.transform.parent = transform;
                obj.transform.position = position;
                obj.AddComponent<SpriteRenderer>().sprite = sprite;
            }
        }
    }
    
}
