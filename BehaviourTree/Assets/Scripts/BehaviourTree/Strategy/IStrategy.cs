using System;

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

