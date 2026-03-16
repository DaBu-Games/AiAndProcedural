using System;
using UnityEngine;
using UnityEngine.AI;

public interface IStrategy
{
    NodeStatus Process();

    void Reset() {}
}

public class ActionStrategy : IStrategy
{
    private readonly Action _action;
    
    public ActionStrategy(Action action) => _action = action;
    
    public NodeStatus Process()
    {
        _action.Invoke();
        return NodeStatus.Success;
    }
}

public class ConditionStrategy : IStrategy
{
    private readonly Func<bool> _condition;
    
    public ConditionStrategy(Func<bool> condition) => _condition = condition;
    
    public NodeStatus Process()
    {
        return _condition.Invoke() ? NodeStatus.Success : NodeStatus.Failure;
    }
}

public class MoveStrategy : IStrategy
{
    private readonly Vector3 _targetPosition;
    private readonly Transform _entity;
    private readonly NavMeshAgent _agent;
    private bool _pathStarted = false;

    public MoveStrategy(Vector3 target, Transform entity, NavMeshAgent agent)
    {
        _targetPosition = target;
        _entity = entity;
        _agent = agent;
    }
    
    public NodeStatus Process()
    {
        if (!_pathStarted)
        {
            _agent.SetDestination(_targetPosition);
            _entity.LookAt(_targetPosition);
            _pathStarted = true;
            return NodeStatus.Running;
        }
        
        if (_agent.remainingDistance <= _agent.stoppingDistance + 0.01f)
        {
            //Debug.Log("succes move");
            return NodeStatus.Success;
        }
        
        return NodeStatus.Running;
    }
    
    public void Reset()
    {
        _pathStarted = false;
        //Debug.Log("reset follow");
    }
}

public class FollowStrategy : IStrategy
{
    private readonly BlackBoard _blackBoard;
    private readonly Transform _entity;
    private readonly NavMeshAgent _agent;
    private BlackBoardKey _lastSeePlayerPosKey;
    private bool _isPathCalculated;
    
    public FollowStrategy(BlackBoard blackBoard, Transform entity, NavMeshAgent agent)
    {
        _blackBoard = blackBoard;
        _entity = entity;
        _agent = agent;
        _lastSeePlayerPosKey = _blackBoard.GetOrRegisterKey("lastSeePlayerPos");
    }

    public NodeStatus Process()
    {
        _blackBoard.TryGetValue(_lastSeePlayerPosKey, out Vector3 targetPos);
        _agent.SetDestination(targetPos);
        _entity.LookAt(targetPos);
        
        if (Vector3.Distance(_entity.position, targetPos) <= _agent.stoppingDistance)
        {
            //Debug.Log("follow succes");
            _blackBoard.SetValue(_lastSeePlayerPosKey, Vector3.zero);
            return NodeStatus.Success;
        }
        
        return NodeStatus.Running;
    }
}

public class WaitStrategy : IStrategy
{
    private float _waitTime;
    private float _startTime;
    private bool _hasStarted = false;
    
    public WaitStrategy(float waitTime) => _waitTime = waitTime;

    public NodeStatus Process()
    {
        if (!_hasStarted)
        {
            _startTime = Time.time;
            _hasStarted = true;
        }

        if (Time.time - _startTime >= _waitTime)
        {
            _hasStarted = false;
            return NodeStatus.Success;
        }
        
        return NodeStatus.Running;
    }

    public void Reset()
    {
        _hasStarted = false;
    }
}

