using System;
using System.Collections.Generic;
using UnityEngine;

public class GuardContext
{
    public float distanceToTarget;
    public float ammoPercent;
    public float healthPercent;
}

public class UtilityAction
{
    public string name;
    public List<Func<GuardContext, float>> considerations;

    public UtilityAction(string name, List<Func<GuardContext, float>> considerations)
    {
        this.name = name;
        this.considerations = considerations;
    }

    public float Evaluate(GuardContext context, out Dictionary<string, float> breakdown)
    {
        breakdown = new Dictionary<string, float>();
        float finalScore = 1.0f;

        if (considerations == null || considerations.Count == 0)
        {
            return 0.3f;
        }

        int index = 0;
        foreach (var consideration in considerations)
        {
            float score = consideration(context);
            string key = $"Consideration_{index++}";
            breakdown[key] = score;
            finalScore *= score;
        }

        return finalScore;
    }
}

public class UtilityAIScorer : MonoBehaviour
{
    public static (UtilityAction bestAction, Dictionary<string, float> scores) EvaluateActions(GuardContext context)
    {
        List<UtilityAction> actions = new List<UtilityAction>
        {
            new UtilityAction("Attack", new List<Func<GuardContext, float>>
            {
                ctx => ResponseCurves.Inverse(ctx.distanceToTarget),
                ctx => ResponseCurves.Linear(ctx.ammoPercent),
                ctx => ResponseCurves.Linear(ctx.healthPercent)
            }),

            new UtilityAction("Reload", new List<Func<GuardContext, float>>
            {
                ctx => ResponseCurves.Inverse(ctx.ammoPercent)
            }),

            new UtilityAction("Flee", new List<Func<GuardContext, float>>
            {
                ctx => ResponseCurves.Inverse(ctx.healthPercent)
            }),

            new UtilityAction("Patrol", new List<Func<GuardContext, float>>())
        };

        UtilityAction bestAction = null;
        float highestScore = -1f;
        Dictionary<string, float> bestBreakdown = null;

        foreach (var action in actions)
        {
            float score;
            Dictionary<string, float> breakdown;

            if (action.considerations.Count == 0)
            {
                score = 0.3f;
                breakdown = new Dictionary<string, float> { { "Baseline", 0.3f } };
            }
            else
            {
                score = action.Evaluate(context, out breakdown);
            }

            Debug.Log($"Action: {action.name} | Final Score: {score:F3}");

            if (score > highestScore)
            {
                highestScore = score;
                bestAction = action;
                bestBreakdown = breakdown;
            }
        }

        return (bestAction, bestBreakdown);
    }
}