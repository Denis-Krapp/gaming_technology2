using System.Collections.Generic;
using UnityEngine;

public enum NodeStatus { Success, Failure, Running }

public class Blackboard
{
    private Dictionary<string, object> data = new Dictionary<string, object>();

    public void Set<T>(string key, T value) => data[key] = value;

    public T Get<T>(string key)
    {
        if (data.TryGetValue(key, out object value))
        {
            return (T)value;
        }
        return default;
    }
}

public abstract class Node
{
    public abstract NodeStatus Tick(Blackboard blackboard);
}


public class Sequence : Node
{
    protected List<Node> children = new List<Node>();
    
    public Sequence(params Node[] nodes) => children.AddRange(nodes);

    public override NodeStatus Tick(Blackboard blackboard)
    {
        foreach (var child in children)
        {
            NodeStatus status = child.Tick(blackboard);
            if (status != NodeStatus.Success)
            {
                return status;
            }
        }
        return NodeStatus.Success;
    }
}

public class Selector : Node
{
    protected List<Node> children = new List<Node>();

    public Selector(params Node[] nodes) => children.AddRange(nodes);

    public override NodeStatus Tick(Blackboard blackboard)
    {
        foreach (var child in children)
        {
            NodeStatus status = child.Tick(blackboard);
            if (status != NodeStatus.Failure)
            {
                return status;
            }
        }
        return NodeStatus.Failure;
    }
}
