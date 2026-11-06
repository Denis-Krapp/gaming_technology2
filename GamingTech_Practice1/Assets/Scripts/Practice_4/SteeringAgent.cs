using UnityEngine;

public class SteeringAgent : MonoBehaviour
{
    [Header("Steering Parameters")]
    public Vector2 velocity;
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float mass = 1f;

    [Header("Behavior Modes")]
    public Transform target;
    public bool isFleeing = false;

    protected virtual void Update()
    {
        if (target == null) return;

        Vector2 steeringForce = Vector2.zero;

        if (!isFleeing)
        {
            steeringForce = Seek(target.position);
        }
        else
        {
            steeringForce = Flee(target.position);
        }

        ApplySteering(steeringForce);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isFleeing = !isFleeing;
            Debug.Log($"Steering Mode: {(isFleeing ? "FLEE" : "SEEK")}");
        }
    }

    protected void ApplySteering(Vector2 steeringForce)
    {
        steeringForce = Vector2.ClampMagnitude(steeringForce, maxForce);
        Vector2 acceleration = steeringForce / mass;

        velocity += acceleration * Time.deltaTime;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (velocity.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public Vector2 Seek(Vector2 targetPos)
    {
        Vector2 desiredVelocity = (targetPos - (Vector2)transform.position).normalized * maxSpeed;
        return desiredVelocity - velocity;
    }

    public Vector2 Flee(Vector2 targetPos)
    {
        Vector2 desiredVelocity = ((Vector2)transform.position - targetPos).normalized * maxSpeed;
        return desiredVelocity - velocity;
    }

    [Header("Arrival Settings")]
    public float slowingRadius = 3f;

    public Vector2 Arrival(Vector2 targetPos, float radius)
    {
        Vector2 toTarget = targetPos - (Vector2)transform.position;
        float distance = toTarget.magnitude;

        if (distance < 0.001f) return -velocity;

        float rampedSpeed = maxSpeed * (distance / radius);
        float clampedSpeed = Mathf.Min(rampedSpeed, maxSpeed);
        
        Vector2 desiredVelocity = toTarget.normalized * clampedSpeed;
        return desiredVelocity - velocity;
    }

    [Header("Wander Settings")]
    public float wanderCircleDistance = 2f;
    public float wanderCircleRadius = 1f;
    public float wanderAngleChange = 0.5f;
    private float wanderAngle = 0f;

    public Vector2 Wander()
    {
        Vector2 circleCenter = (Vector2)transform.position + velocity.normalized * wanderCircleDistance;

        wanderAngle += Random.Range(-wanderAngleChange, wanderAngleChange);

        Vector2 displacement = new Vector2(Mathf.Cos(wanderAngle), Mathf.Sin(wanderAngle)) * wanderCircleRadius;

        Vector2 wanderTarget = circleCenter + displacement;
        return Seek(wanderTarget);
    }

    [Header("Pursuit")]
    public SteeringAgent targetAgentRef;

    public Vector2 Pursue(Transform targetT, Vector2 targetVel)
    {
        Vector2 toTarget = (Vector2)targetT.position - (Vector2)transform.position;
        float distance = toTarget.magnitude;

        float predictionTime = distance / maxSpeed;

        Vector2 predictedPosition = (Vector2)targetT.position + targetVel * predictionTime;

        return Seek(predictedPosition);
    }
}
