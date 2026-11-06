using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentMover : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;
    private bool hasLoggedPath = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    private void Update()
    {
        if (!hasLoggedPath && !agent.pathPending && agent.hasPath)
        {
            NavMeshPath path = agent.path;
            Debug.Log($"[NavMeshPath] Status: {path.status} | Final Waypoint Corners Count: {path.corners.Length}");

            for (int i = 0; i < path.corners.Length; i++)
            {
                Debug.Log($"  Corner {i}: {path.corners[i]}");
            }

            hasLoggedPath = true;
        }

        if (agent.hasPath && agent.path.corners.Length > 1)
        {
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Debug.DrawLine(corners[i] + Vector3.up * 0.1f, corners[i + 1] + Vector3.up * 0.1f, Color.yellow);
            }
        }
    }
}