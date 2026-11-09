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

    [Header("Part G: Reward Hacking Probe")]
    public bool useHoverHack = false;

    [Header("Part F: Inference Evaluation")]
    public bool trackInferenceStats = false;
    public int targetEvaluationEpisodes = 100;
    private int episodesStarted = 0;
    private int episodesSucceeded = 0;
    private bool evaluationComplete = false;

    
    private Rigidbody rb;
    private Vector3 startLocalPos;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startLocalPos = transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        if (trackInferenceStats && !evaluationComplete)
        {
            episodesStarted++;
            Debug.Log($"[Inference] Starting episode {episodesStarted}/{targetEvaluationEpisodes} (Current successes: {episodesSucceeded})");

            if (episodesStarted > targetEvaluationEpisodes)
            {
                evaluationComplete = true;
                float successRate = (episodesSucceeded / (float)targetEvaluationEpisodes) * 100f;
                Debug.Log($"<color=cyan><b>[INFERENCE EVALUATION COMPLETE]</b></color> " +
                        $"Success Rate: <b>{successRate:F1}%</b> ({episodesSucceeded}/{targetEvaluationEpisodes} episodes)");
                return;
            }
        }

        if (transform.localPosition.y < 0f)
        {
            transform.localPosition = new Vector3(0, 0.5f, 0);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        target.localPosition = new Vector3(
            Random.Range(-4f, 4f),
            0.5f,
            Random.Range(-4f, 4f)
        );

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
        if (trackInferenceStats && evaluationComplete)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actions.ContinuousActions[0];
        controlSignal.z = actions.ContinuousActions[1];
        rb.AddForce(controlSignal * forceMultiplier);

        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);

        if (useHoverHack && distanceToTarget < 2.0f)
        {
            AddReward(0.02f);
        }

        if (distanceToTarget < 1.42f)
        {
            if (useShaping)
            {
                AddReward(-prevPotential);
            }

            AddReward(1.0f);

            if (trackInferenceStats && !evaluationComplete)
            {
                episodesSucceeded++;
                Debug.Log($"<color=green>[SUCCESS]</color> Episode {episodesStarted} reached target! Total successes: {episodesSucceeded}");
            }

            EndEpisode();
            return;
        }

        if (transform.localPosition.y < 0f)
        {
            if (useShaping)
            {
                AddReward(-prevPotential);
            }

            if (trackInferenceStats && !evaluationComplete)
            {
                Debug.Log($"<color=red>[FAIL]</color> Episode {episodesStarted} fell off the edge.");
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
        return -dist / maxDistance;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}