using UnityEngine;

public class UtilityTestRunner : MonoBehaviour
{
    [Header("Select Scenario to Test")]
    public TestScenario scenarioToRun;

    public enum TestScenario
    {
        LectureExample,
        CloseFullHealthFullAmmo,
        CloseZeroAmmo,
        FarLowHealth
    }

    private void Start()
    {
        RunScenario(scenarioToRun);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RunScenario(scenarioToRun);
        }
    }

    public void RunScenario(TestScenario scenario)
    {
        GuardContext context = new GuardContext();

        switch (scenario)
        {
            case TestScenario.LectureExample:
                context.distanceToTarget = 0.90f;
                context.ammoPercent = 0.80f;
                context.healthPercent = 0.70f;
                Debug.Log("=== RUNNING SCENARIO: Lecture Example ===");
                break;

            case TestScenario.CloseFullHealthFullAmmo:
                context.distanceToTarget = 0.10f;
                context.ammoPercent = 1.00f;
                context.healthPercent = 1.00f;
                Debug.Log("=== RUNNING SCENARIO: (a) Close, Full Ammo, Full Health ===");
                break;

            case TestScenario.CloseZeroAmmo:
                context.distanceToTarget = 0.10f;
                context.ammoPercent = 0.00f;
                context.healthPercent = 1.00f;
                Debug.Log("=== RUNNING SCENARIO: (b) Close Range, Zero Ammo ===");
                break;

            case TestScenario.FarLowHealth:
                context.distanceToTarget = 0.85f;
                context.ammoPercent = 0.50f;
                context.healthPercent = 0.15f;
                Debug.Log("=== RUNNING SCENARIO: (c) Far Range, Critically Low Health ===");
                break;
        }

        var (bestAction, breakdown) = UtilityAIScorer.EvaluateActions(context);
        Debug.Log($"<color=green><b>WINNING ACTION: {bestAction.name}</b></color>\n");
    }
}