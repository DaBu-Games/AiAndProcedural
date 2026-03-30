using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CaveGenerationValues")]
public class CaveGenerationValues : ScriptableObject
{
    [Header("Random walk settings")] 
    [Range(0f, 1f)]
    public float TurnedCellsPercentage = 0.3f;
    
    [Header("Cellular automata settings")] 
    public int AutomataIterations = 5;
    public int AutomataWallTreshhold = 5;
    public int AutomataOpenSpaceThreshold = 2;

    [Header("Ore deposits settings")] 
    public int MinOreDeposits = 5;
    public int MaxOreDeposits = 10;
    public int MinOreDepositRadius = 5;
    public int MaxOreDepositRadius = 15;

    [Header("Create content settings")] 
    public int EntranceWallThreshhold = 3;
    public int EnemyWallThreshhold = 1;
}
