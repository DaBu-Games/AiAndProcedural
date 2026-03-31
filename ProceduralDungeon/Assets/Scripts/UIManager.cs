using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField  seedInput;
    [SerializeField] private TMP_InputField  gridInputX;
    [SerializeField] private TMP_InputField  gridInputY;

    public void GenerateCave()
    {
        int seed = int.Parse(seedInput.text);
        int x = int.Parse(gridInputX.text);
        int y = int.Parse(gridInputY.text);
        
        GameEvents.OnGenerateCave.Invoke(seed, new Vector2Int(x, y));
    }
}
