using System.Collections.Generic;
using UnityEngine;

public class GOAPTestRunner : MonoBehaviour
{
    [Header("Select GOAP Scenario to Test")]
    public GOAPScenario scenarioToRun;

    public enum GOAPScenario
    {
        TestCase1_BasicAttack,
        TestCase2_NeedsReload,
        TestCase3_CustomState
    }

    private void Start()
    {
        RunGOAPScenario(scenarioToRun);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RunGOAPScenario(scenarioToRun);
        }
    }

    public void RunGOAPScenario(GOAPScenario scenario)
    {
        WorldState current = new WorldState();
        WorldState goal = new WorldState();
        
        goal.Set("EnemyDead", true);

        List<GAction> actions = GetStandardActions();

        switch (scenario)
        {
            case GOAPScenario.TestCase1_BasicAttack:
                current.Set("EnemyVisible", true);
                current.Set("InRange", false);
                current.Set("HasAmmo", true);
                current.Set("HasAmmoInInventory", false);
                Debug.Log("=== GOAP TEST CASE 1: Basic Attack ===");
                break;

            case GOAPScenario.TestCase2_NeedsReload:
                current.Set("EnemyVisible", true);
                current.Set("InRange", false);
                current.Set("HasAmmo", false);
                current.Set("HasAmmoInInventory", true);
                Debug.Log("=== GOAP TEST CASE 2: Needs Reload ===");
                break;

           case GOAPScenario.TestCase3_CustomState:
            current.Set("EnemyVisible", true);
            current.Set("InRange", false);
            current.Set("HasAmmo", false);
            current.Set("HasAmmoInInventory", true);
            Debug.Log("=== GOAP TEST CASE 3: Custom State ===");
            break;
        }

        List<GAction> plan = GOAPPlanner.Plan(current, goal, actions, out float totalCost);

        if (plan != null && plan.Count > 0)
        {
            string planString = "";
            for (int i = 0; i < plan.Count; i++)
            {
                planString += plan[i].name + (i < plan.Count - 1 ? " → " : "");
            }
            Debug.Log($"<color=green><b>PLAN FOUND!</b></color>\nExecution Order: [{planString}]\nTotal Cost: {totalCost}");
        }
        else
        {
            Debug.LogWarning("<color=red><b>NO VALID PLAN FOUND!</b></color>");
        }
    }

    public static List<GAction> GetStandardActions()
    {
        List<GAction> actions = new List<GAction>();

        GAction move = new GAction("MoveToEnemy", 2f);
        move.AddPrecondition("EnemyVisible", true);
        move.AddEffect("InRange", true);
        actions.Add(move);

        GAction attack = new GAction("Attack", 1f);
        attack.AddPrecondition("InRange", true);
        attack.AddPrecondition("HasAmmo", true);
        attack.AddEffect("EnemyDead", true);
        actions.Add(attack);

        GAction reload = new GAction("Reload", 3f);
        reload.AddPrecondition("HasAmmoInInventory", true);
        reload.AddEffect("HasAmmo", true);
        actions.Add(reload);

        return actions;
    }
}