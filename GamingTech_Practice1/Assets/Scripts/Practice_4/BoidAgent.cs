using System.Collections.Generic;
using UnityEngine;

public class BoidAgent : SteeringAgent
{
    [Header("Flocking Parameters")]
    public float perceptionRadius = 3f;
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1.0f;
    public float cohesionWeight = 1.0f;

    [HideInInspector] public List<BoidAgent> allBoids;

    protected override void Update()
    {
        List<BoidAgent> neighbors = GetNeighbors();

        Vector2 separation = ComputeSeparation(neighbors) * separationWeight;
        Vector2 alignment = ComputeAlignment(neighbors) * alignmentWeight;
        Vector2 cohesion = ComputeCohesion(neighbors) * cohesionWeight;

        Vector2 flockingForce = separation + alignment + cohesion;

        if (neighbors.Count == 0)
        {
            flockingForce += Wander();
        }

        ApplySteering(flockingForce);
        WrapAroundScreen();
    }

    private List<BoidAgent> GetNeighbors()
    {
        List<BoidAgent> neighbors = new List<BoidAgent>();
        foreach (var boid in allBoids)
        {
            if (boid == this) continue;
            float sqrDist = ((Vector2)boid.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDist < perceptionRadius * perceptionRadius)
            {
                neighbors.Add(boid);
            }
        }
        return neighbors;
    }

    private Vector2 ComputeSeparation(List<BoidAgent> neighbors)
    {
        Vector2 steering = Vector2.zero;
        int count = 0;

        foreach (var neighbor in neighbors)
        {
            Vector2 toNeighbor = (Vector2)transform.position - (Vector2)neighbor.transform.position;
            float dist = toNeighbor.magnitude;
            if (dist > 0f)
            {
                steering += toNeighbor.normalized / dist;
                count++;
            }
        }

        if (count > 0)
        {
            steering /= count;
            steering = steering.normalized * maxSpeed - velocity;
        }
        return steering;
    }

    private Vector2 ComputeAlignment(List<BoidAgent> neighbors)
    {
        Vector2 avgVelocity = Vector2.zero;
        int count = 0;

        foreach (var neighbor in neighbors)
        {
            avgVelocity += neighbor.velocity;
            count++;
        }

        if (count > 0)
        {
            avgVelocity /= count;
            Vector2 desired = avgVelocity.normalized * maxSpeed;
            return desired - velocity;
        }
        return Vector2.zero;
    }

    private Vector2 ComputeCohesion(List<BoidAgent> neighbors)
    {
        Vector2 centerOfMass = Vector2.zero;
        int count = 0;

        foreach (var neighbor in neighbors)
        {
            centerOfMass += (Vector2)neighbor.transform.position;
            count++;
        }

        if (count > 0)
        {
            centerOfMass /= count;
            return Seek(centerOfMass);
        }
        return Vector2.zero;
    }

    private void WrapAroundScreen()
    {
        Vector2 pos = transform.position;
        if (pos.x > 15f) pos.x = -15f;
        if (pos.x < -15f) pos.x = 15f;
        if (pos.y > 10f) pos.y = -10f;
        if (pos.y < -10f) pos.y = 10f;
        transform.position = pos;
    }
}