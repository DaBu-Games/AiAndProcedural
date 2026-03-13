using System;
using UnityEngine;

public class TestBlackBoard : MonoBehaviour
{
    readonly BlackBoard blackBoard = new BlackBoard();

    private void Awake()
    {
        BlackBoardKey IsTestKey = blackBoard.GetOrRegister("IsTested");
        blackBoard.SetValue(IsTestKey, false);

        if (blackBoard.TryGetValue(IsTestKey, out bool isTested))
        {
            Debug.Log($"IsTested: {isTested}");
        }
        
        blackBoard.SetValue(IsTestKey, true);
        
        if (blackBoard.TryGetValue(IsTestKey, out bool isTeste))
        {
            Debug.Log($"IsTested: {isTeste}");
        }
    }
}
