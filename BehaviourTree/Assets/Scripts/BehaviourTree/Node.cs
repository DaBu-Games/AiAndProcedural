using UnityEngine;
using System.Collections.Generic;
using System.Text;

public enum NodeStatus
{
    Running,
    Success,
    Failure
}

public abstract class Node
{
    public string Name { get; private set; }
    protected readonly List<Node> Children = new List<Node>();
    protected int CurrentChild = 0;

    protected Node(string name)
    {
        this.Name = name;
    }

    public void AddChild(Node child) => Children.Add(child);

    public Node GetCurrentNode()
    {
        if (Children.Count == 0)
            return this;
        
        int index = Mathf.Clamp(CurrentChild, 0, Children.Count - 1);
        return Children[index].GetCurrentNode();
    }

    public virtual NodeStatus Process()
    {
        return Children[CurrentChild].Process();
    } 

    public virtual void Reset()
    {
        CurrentChild = 0;
        foreach (var child in Children)
        {
            child.Reset();
        }
    }
}

public class SequenceNode : Node
{
    public SequenceNode(string name) : base(name) { }

    public override NodeStatus Process()
    {
        if (CurrentChild < Children.Count)
        {
            switch (Children[CurrentChild].Process())
            {
                case NodeStatus.Running:
                    return NodeStatus.Running;
                case NodeStatus.Failure:
                    Reset();
                    return NodeStatus.Failure;
                default:
                    CurrentChild++;
                    return CurrentChild == Children.Count ? NodeStatus.Success : NodeStatus.Running;
            }
        }
        
        Reset();
        return NodeStatus.Failure;
    }
}

public class SelectorNode : Node
{
    public SelectorNode(string name) : base(name) { }

    public override NodeStatus Process()
    {
        if (CurrentChild < Children.Count)
        {
            switch (Children[CurrentChild].Process())
            {
                case NodeStatus.Running:
                    return NodeStatus.Running;
                case NodeStatus.Failure:
                    CurrentChild++;
                    return NodeStatus.Running;
                default:
                    Reset();
                    return NodeStatus.Success;
            }
        }
        
        Reset();
        return NodeStatus.Failure;
    }
}

public class LeafNode : Node
{
    private IStrategy _strategy;

    public LeafNode(string name, IStrategy strategy) : base(name) => _strategy = strategy;

    public override NodeStatus Process() => _strategy.Process();
    
    public override void Reset() => _strategy.Reset();
}

public class BehaviourTree : Node
{
    public BehaviourTree(string name) : base(name){ }

    public override NodeStatus Process()
    {
        Children[CurrentChild].Process();
        int next = CurrentChild + 1;
        CurrentChild = next >= Children.Count ? 0 : next;
        return NodeStatus.Running;
    }
}