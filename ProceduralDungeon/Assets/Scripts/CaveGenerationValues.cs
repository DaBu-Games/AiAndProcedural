using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CaveGenerationValues")]
public class CaveGenerationValues : ScriptableObject
{
    [Header("Random walk settings")] 
    [Range(0f, 1f)]
    public float MinTurnedCellsPercentage = 0.3f;
    [Range(0f, 1f)]
    public float MaxTurnedCellsPercentage = 0.5f;
    
    [Header("Cellular automata settings")] 
    public int AutomataIterations = 5;
    public int AutomataWallTreshhold = 5;
    public int AutomataOpenSpaceThreshold = 2;

    [Header("Ore deposits settings")] 
    public int MinOreDeposits = 5;
    public int MaxOreDeposits = 10;
    public int MinOreDepositRadius = 5;
    public int MaxOreDepositRadius = 15;
}
