using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float viewRadius = 1.5f;
    [Range(0, 360)]
    [SerializeField] private float viewAngle = 20f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Transform target;
    
    private bool _isTargetVisible = false;
    
    private BlackBoard _blackBoard;
    private BlackBoardKey _canSeePlayer;
    private BlackBoardKey _lastSeePlayerPosKey;

    public void Initialize(BlackBoard blackBoard)
    {
        _blackBoard = blackBoard;
       _canSeePlayer = _blackBoard.GetOrRegisterKey("canSeePlayer");
       _lastSeePlayerPosKey = _blackBoard.GetOrRegisterKey("lastSeePlayerPos");
       _blackBoard.SetValue(_canSeePlayer, _isTargetVisible);
       _blackBoard.SetValue(_lastSeePlayerPosKey, Vector3.zero);
    }

    private void Update()
    {
        bool currentVisibility = CheckTargetVisibility();

        if (_isTargetVisible != currentVisibility)
        {
            _isTargetVisible = currentVisibility;
            _blackBoard.SetValue(_canSeePlayer, _isTargetVisible);
        }
        
        if (currentVisibility)
        {
            _blackBoard.SetValue(_lastSeePlayerPosKey, target.position);
        }
    }

    private bool CheckTargetVisibility()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if (distanceToTarget > viewRadius)
        {
            return false;
        }
        
        Vector3 targetDir = (target.position - transform.position).normalized;
        if (Vector3.Angle(transform.forward, targetDir) > viewAngle / 2)
        {
            return false;
        }

        if (Physics.Raycast(transform.position, targetDir, distanceToTarget, obstacleMask))
        {
            return false;
        }
        
        return true;
    }
    

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;
        
        return new Vector3(MathF.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        
        float halfAngle = viewAngle / 2;
        Vector3 leftBoundary = DirFromAngle(-halfAngle, false);
        Vector3 rightBoundary = DirFromAngle(halfAngle, false);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, leftBoundary * viewRadius);
        Gizmos.DrawRay(transform.position, rightBoundary * viewRadius);

        if (_isTargetVisible)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
