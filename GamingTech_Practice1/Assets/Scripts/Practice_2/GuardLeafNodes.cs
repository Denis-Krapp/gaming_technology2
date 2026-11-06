using UnityEngine;

public class IsHealthBelow : Node
{
    private float thresholdPct;
    public IsHealthBelow(float pct) => thresholdPct = pct;

    public override NodeStatus Tick(Blackboard blackboard)
    {
        var guard = blackboard.Get<GuardAgent>("agent");
        float healthPct = (guard.health / 100f) * 100f;
        return healthPct <= thresholdPct ? NodeStatus.Success : NodeStatus.Failure;
    }
}

public class IsPlayerInRange : Node
{
    private float range;
    public IsPlayerInRange(float range) => this.range = range;

    public override NodeStatus Tick(Blackboard blackboard)
    {
        var guard = blackboard.Get<GuardAgent>("agent");
        if (guard.player == null) return NodeStatus.Failure;

        float dist = Vector3.Distance(guard.transform.position, guard.player.position);
        return dist <= range ? NodeStatus.Success : NodeStatus.Failure;
    }
}

public class IsPlayerVisible : Node
{
    public override NodeStatus Tick(Blackboard blackboard)
    {
        var guard = blackboard.Get<GuardAgent>("agent");
        if (guard.player == null) return NodeStatus.Failure;

        float dist = Vector3.Distance(guard.transform.position, guard.player.position);
        return dist <= guard.detectionRadius ? NodeStatus.Success : NodeStatus.Failure;
    }
}


public class ActionNode : Node
{
    private System.Action<GuardAgent> action;
    private string stateName;
    private Color debugColor;

    public ActionNode(string name, Color color, System.Action<GuardAgent> actionBody)
    {
        stateName = name;
        debugColor = color;
        action = actionBody;
    }

    public override NodeStatus Tick(Blackboard blackboard)
    {
        var guard = blackboard.Get<GuardAgent>("agent");
        guard.currentDebugState = "[BT] " + stateName;
        guard.SetColor(debugColor);
        action(guard);
        return NodeStatus.Running;
    }
}