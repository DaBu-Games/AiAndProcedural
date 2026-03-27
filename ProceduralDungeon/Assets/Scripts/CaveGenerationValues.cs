using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CaveGenerationValues")]
public class CaveGenerationValues : ScriptableObject
{
    [Header("Random walk settings")] 
    [Range(0f, 1f)]
    public float MinTurnedCellsPercentage;
    [Range(0f, 1f)]
    public float MaxTurnedCellsPercentage;

    [Header("Ore deposits settings")] 
    public int MinOreDeposits;
    public int MaxOreDeposits;
    public int MinOreDepositRadius;
    public int MaxOreDepositRadius;
}
