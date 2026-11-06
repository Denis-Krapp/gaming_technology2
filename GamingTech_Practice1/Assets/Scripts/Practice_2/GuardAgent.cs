using UnityEngine;

public class GuardAgent : MonoBehaviour
{
    [Header("State Machine")]
    public string currentDebugState;
    private IState currentState;

    [Header("References")]
    public Transform player;
    public Transform[] waypoints;
    private Renderer guardRenderer;

    [Header("Attributes & Tuning")]
    public float health = 100f;
    public float speed = 3.5f;
    public float detectionRadius = 7f;
    public float attackRange = 2f;
    public float fleeHealthThreshold = 20f;
    public float searchDuration = 5f;

    [HideInInspector] public int currentWaypointIndex = 0;
    [HideInInspector] public float searchTimer = 0f;

    void Start()
    {
        guardRenderer = GetComponent<Renderer>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        ChangeState(new IdleState());
    }

    void Update()
    {

    }

    public void ChangeState(IState newState)
    {
        if (currentState != null)
        {
            currentState.Exit(this);
        }

        currentState = newState;
        currentDebugState = newState.GetType().Name;

        if (currentState != null)
        {
            currentState.Enter(this);
        }
    }

    public void SetColor(Color color)
    {
        if (guardRenderer != null)
        {
            guardRenderer.material.color = color;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}