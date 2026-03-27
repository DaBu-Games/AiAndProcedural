using System.Collections.Generic;
using UnityEngine;

public class OreDeposit
{
    private List<Vector2Int> _orePositions = new List<Vector2Int>();
    private bool _exposed = false;
    
    public void AddOrePositions(List<Vector2Int> orePositions) => _orePositions.AddRange(orePositions);
    public List<Vector2Int> GetOrePositions() => _orePositions;
    
    public bool HasOrePosition(Vector2Int orePosition) => _orePositions.Contains(orePosition);
    public void SetExposed() => _exposed = true;
    public bool IsExposed() => _exposed;
}
