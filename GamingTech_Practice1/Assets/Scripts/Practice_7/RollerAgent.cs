using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RollerAgent : Agent
{
    [Header("References")]
    public Transform target;
    public float forceMultiplier = 10f;

    [Header("Part E: Potential-Based Shaping")]
    public bool useShaping = false;
    private const float gamma = 0.99f;
    private const float maxDistance = 14.2f;
    private float prevPotential = 0f;

    [Header("Part D: Fair Evaluation Mode")]
    public bool evalMode = false;
    public int evalSeed = 12345;
    public int targetEvalEpisodes = 100;

    private System.Random evalRng;
    private int evalEpisodesStarted = 0;
    private int evalSuccesses = 0;
    private int evalFalls = 0;
    private int currentEpisodeStepCount = 0;
    private List<int> successfulStepCounts = new List<int>();
    private bool evalCompleted = false;

    private Rigidbody rb;
    private Vector3 defaultStartPos = new Vector3(0f, 0.5f, 0f);

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (evalMode)
        {
            evalRng = new System.Random(evalSeed);
            Time.timeScale = 10f;
        }
    }

    public override void OnEpisodeBegin()
    {
        if (evalMode)
        {
            if (evalEpisodesStarted >= targetEvalEpisodes && !evalCompleted)
            {
                evalCompleted = true;
                Time.timeScale = 1f;

                float successRate = (evalSuccesses / (float)targetEvalEpisodes) * 100f;
                float meanSteps = 0f;
                if (successfulStepCounts.Count > 0)
                {
                    long totalSteps = 0;
                    foreach (int s in successfulStepCounts) totalSteps += s;
                    meanSteps = totalSteps / (float)successfulStepCounts.Count;
                }

                Debug.Log($"<color=cyan><b>[EVALUATION REPORT COMPLETED]</b></color>\n" +
                          $"Episodes: {targetEvalEpisodes}\n" +
                          $"Success Rate: <b>{successRate:F1}%</b> ({evalSuccesses}/{targetEvalEpisodes})\n" +
                          $"Falls: <b>{evalFalls}</b>\n" +
                          $"Mean Steps to Target: <b>{meanSteps:F1}</b>");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }

            evalEpisodesStarted++;
            currentEpisodeStepCount = 0;

            transform.localPosition = defaultStartPos;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            float targetX = (float)(evalRng.NextDouble() * 8.0 - 4.0);
            float targetZ = (float)(evalRng.NextDouble() * 8.0 - 4.0);
            target.localPosition = new Vector3(targetX, 0.5f, targetZ);
        }
        else
        {
            transform.localPosition = defaultStartPos;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            target.localPosition = new Vector3(
                Random.Range(-4f, 4f),
                0.5f,
                Random.Range(-4f, 4f)
            );
        }

        if (useShaping)
        {
            prevPotential = CalculatePotential();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.linearVelocity.x);
        sensor.AddObservation(rb.linearVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (evalMode && evalCompleted) return;

        currentEpisodeStepCount++;

        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        rb.AddForce(controlSignal * forceMultiplier);

        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);

        if (distanceToTarget < 1.42f)
        {
            if (useShaping) AddReward(-prevPotential);
            AddReward(1.0f);

            if (evalMode)
            {
                evalSuccesses++;
                successfulStepCounts.Add(currentEpisodeStepCount);
            }

            EndEpisode();
            return;
        }

        if (transform.localPosition.y < 0f)
        {
            if (useShaping) AddReward(-prevPotential);

            if (evalMode)
            {
                evalFalls++;
            }

            EndEpisode();
            return;
        }

        if (useShaping)
        {
            float currentPotential = CalculatePotential();
            float shapingReward = (gamma * currentPotential) - prevPotential;
            AddReward(shapingReward);
            prevPotential = currentPotential;
        }
    }

    private float CalculatePotential()
    {
        float dist = Vector3.Distance(transform.localPosition, target.localPosition);
        return 1f - (dist / maxDistance); 
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}