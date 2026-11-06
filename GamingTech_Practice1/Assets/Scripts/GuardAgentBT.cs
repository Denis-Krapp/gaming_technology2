using UnityEngine;


public class GuardAgentBT : MonoBehaviour
{
    private Node rootNode;
    private Blackboard blackboard;
    public GuardAgent guardComponent;

    [Header("BT Specific Flags")]
    public bool hasCalledForHelp = false;

    void Start()
    {
        if (guardComponent == null) guardComponent = GetComponent<GuardAgent>();

        blackboard = new Blackboard();
        blackboard.Set("agent", guardComponent);
        rootNode = new Selector(
            new Sequence(
                new IsHealthBelow(20f),
                new ActionNode("Flee", Color.blue, (g) => {
                    Vector3 dir = (g.transform.position - g.player.position).normalized;
                    g.transform.position += dir * g.speed * 1.2f * Time.deltaTime;
                })
            ),
            new Sequence(
                new IsPlayerInRange(guardComponent.attackRange),
                new ActionNode("Attack", Color.red, (g) => {
                })
            ),
            new Sequence(
                new IsPlayerVisible(),
                new ActionNode("Chase", Color.orange, (g) => {
                    CheckCallForHelp();
                    g.transform.position = Vector3.MoveTowards(g.transform.position, g.player.position, g.speed * 1.5f * Time.deltaTime);
                })
            ),
            new ActionNode("Patrol", Color.green, (g) => {
                hasCalledForHelp = false; 

                if (g.waypoints == null || g.waypoints.Length == 0) return;
                Transform wp = g.waypoints[g.currentWaypointIndex];
                g.transform.position = Vector3.MoveTowards(g.transform.position, wp.position, g.speed * Time.deltaTime);
                if (Vector3.Distance(g.transform.position, wp.position) < 0.2f)
                {
                    g.currentWaypointIndex = (g.currentWaypointIndex + 1) % g.waypoints.Length;
                }
            })
        );
    }

    void Update()
    {
        blackboard.Set("player_pos", guardComponent.player != null ? guardComponent.player.position : Vector3.zero);

        rootNode.Tick(blackboard);
    }

    private void CheckCallForHelp()
    {
        if (!hasCalledForHelp)
        {
            hasCalledForHelp = true;
            Debug.Log("<color=cyan>[Behavior Tree]</color> Охранник: 'Мне нужна помощь! Обнаружен нарушитель!'");
        }
    }
}
