using System.Collections.Generic;
using UnityEngine;

public class PartGComparison : MonoBehaviour
{
    private void Start()
    {
        RunComparison();
    }

    public void RunComparison()
    {
        Debug.Log("=== PART G: UTILITY AI VS GOAP COMPARISON ===");
        Debug.Log("Situation: Close range, zero ammo, full health.");

        GuardContext utContext = new GuardContext
        {
            distanceToTarget = 0.10f,
            ammoPercent = 0.00f,
            healthPercent = 1.00f
        };

        var (bestAction, _) = UtilityAIScorer.EvaluateActions(utContext);
        Debug.Log($"<color=cyan>1. Utility AI Output (Right Now):</color> <b>{bestAction.name}</b>");

        WorldState current = new WorldState();
        current.Set("EnemyVisible", true);
        current.Set("InRange", true);
        current.Set("HasAmmo", false);
        current.Set("HasAmmoInInventory", true);

        WorldState goal = new WorldState();
        goal.Set("EnemyDead", true);

        List<GAction> plan = GOAPPlanner.Plan(current, goal, GOAPTestRunner.GetStandardActions(), out float totalCost);

        if (plan != null && plan.Count > 0)
        {
            string planString = string.Join(" → ", plan.ConvertAll(a => a.name));
            Debug.Log($"<color=yellow>2. GOAP Output (Full Plan):</color> <b>[{planString}]</b> (Total Cost: {totalCost})");
        }
        else
        {
            Debug.LogWarning("2. GOAP Output: No valid plan found.");
        }
    }
}