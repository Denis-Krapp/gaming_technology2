using System.Collections.Generic;
using UnityEngine;

public class FlockSpawner : MonoBehaviour
{
    public GameObject boidPrefab;
    public int boidCount = 40;
    public float spawnRadius = 5f;

    private List<BoidAgent> boids = new List<BoidAgent>();

    private void Start()
    {
        for (int i = 0; i < boidCount; i++)
      {
            Vector2 spawnPos = Random.insideUnitCircle * spawnRadius;
            GameObject obj = Instantiate(boidPrefab, spawnPos, Quaternion.identity);
            BoidAgent boid = obj.GetComponent<BoidAgent>();
            
            boid.velocity = Random.insideUnitCircle.normalized * boid.maxSpeed;
            boids.Add(boid);
        }

        foreach (var boid in boids)
        {
            boid.allBoids = boids;
        }
    }
}