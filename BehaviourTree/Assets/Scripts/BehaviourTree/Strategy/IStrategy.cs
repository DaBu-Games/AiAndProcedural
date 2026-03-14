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
    private readonly Transform _target;
    private readonly Transform _entity;
    private readonly NavMeshAgent _agent;

    public MoveStrategy(Transform target, Transform entity, NavMeshAgent agent)
    {
        _target = target;
        _entity = entity;
        _agent = agent;
    }
    
    public NodeStatus Process()
    {
        _agent.SetDestination(_target.position);
        _entity.LookAt(_target.position);

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            return NodeStatus.Success;
        }
        
        return NodeStatus.Running;
    }
}

