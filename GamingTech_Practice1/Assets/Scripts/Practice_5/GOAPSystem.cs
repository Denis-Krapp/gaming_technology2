using System.Collections.Generic;
using UnityEngine;

public class WorldState
{
    public Dictionary<string, bool> states = new Dictionary<string, bool>();

    public WorldState() { }

    public WorldState(Dictionary<string, bool> source)
    {
        foreach (var kvp in source)
            states[kvp.Key] = kvp.Value;
    }

    public void Set(string key, bool value)
    {
        states[key] = value;
    }

    public bool Get(string key)
    {
        return states.ContainsKey(key) && states[key];
    }

    public bool Satisfies(WorldState goal)
    {
        foreach (var kvp in goal.states)
        {
            if (!states.ContainsKey(kvp.Key) || states[kvp.Key] != kvp.Value)
                return false;
        }
        return true;
    }
}

public class GAction
{
    public string name;
    public Dictionary<string, bool> preconditions = new Dictionary<string, bool>();
    public Dictionary<string, bool> effects = new Dictionary<string, bool>();
    public float cost;

    public GAction(string name, float cost)
    {
        this.name = name;
        this.cost = cost;
    }

    public void AddPrecondition(string key, bool value) => preconditions[key] = value;
    public void AddEffect(string key, bool value) => effects[key] = value;
}

public class GOAPPlanner
{
    public static List<GAction> Plan(WorldState current, WorldState goal, List<GAction> availableActions, out float totalCost)
    {
        totalCost = 0f;
        List<Node> leaves = new List<Node>();
        Node startNode = new Node(null, 0f, current, null);

        if (BuildGraph(startNode, leaves, availableActions, goal))
        {
            Node cheapest = null;
            float lowestCost = float.MaxValue;

            foreach (var leaf in leaves)
            {
                if (leaf.runningCost < lowestCost)
                {
                    lowestCost = leaf.runningCost;
                    cheapest = leaf;
                }
            }

            if (cheapest != null)
            {
                totalCost = lowestCost;
                List<GAction> result = new List<GAction>();
                Node curr = cheapest;
                while (curr.action != null)
                {
                    result.Insert(0, curr.action);
                    curr = curr.parent;
                }

                result.Reverse();
                return result;
            }
        }

        return null;
    }

    private class Node
    {
        public Node parent;
        public float runningCost;
        public WorldState state;
        public GAction action;

        public Node(Node parent, float runningCost, WorldState state, GAction action)
        {
            this.parent = parent;
            this.runningCost = runningCost;
            this.state = state;
            this.action = action;
        }
    }

    private static bool BuildGraph(Node parent, List<Node> leaves, List<GAction> actions, WorldState goal)
    {
        bool foundPath = false;

        foreach (var action in actions)
        {
            bool achievesGoal = false;
            foreach (var g in goal.states)
            {
                if (action.effects.ContainsKey(g.Key) && action.effects[g.Key] == g.Value)
                {
                    achievesGoal = true;
                    break;
                }
            }

            if (achievesGoal)
            {
                WorldState currentState = new WorldState(parent.state.states);
                foreach (var effect in action.effects)
                {
                    currentState.Set(effect.Key, effect.Value);
                }

                WorldState newGoal = new WorldState();
                foreach (var g in goal.states)
                {
                    if (!action.effects.ContainsKey(g.Key) || action.effects[g.Key] != g.Value)
                    {
                        newGoal.Set(g.Key, g.Value);
                    }
                }
                foreach (var pre in action.preconditions)
                {
                    newGoal.Set(pre.Key, pre.Value);
                }

                Node node = new Node(parent, parent.runningCost + action.cost, currentState, action);

                if (parent.state.Satisfies(newGoal))
                {
                    leaves.Add(node);
                    foundPath = true;
                }
                else
                {
                    if (BuildGraph(node, leaves, actions, newGoal))
                    {
                        foundPath = true;
                    }
                }
            }
        }

        return foundPath;
    }
}