using UnityEngine;


public class IdleState : IState
{
    private float idleTimer;
    public void Enter(GuardAgent guard) 
    { 
        guard.SetColor(Color.white); 
        idleTimer = 2f;
    }
    
    public void Update(GuardAgent guard)
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            guard.ChangeState(new PatrolState());
        }
    }
    public void Exit(GuardAgent guard) {}
}


public class PatrolState : IState
{
    public void Enter(GuardAgent guard) { guard.SetColor(Color.green); }
    
    public void Update(GuardAgent guard)
    {
        if (guard.player != null && Vector3.Distance(guard.transform.position, guard.player.position) <= guard.detectionRadius)
        {
            guard.ChangeState(new ChaseState());
            return;
        }

        if (guard.waypoints == null || guard.waypoints.Length == 0) return;

        Transform targetWaypoint = guard.waypoints[guard.currentWaypointIndex];
        guard.transform.position = Vector3.MoveTowards(guard.transform.position, targetWaypoint.position, guard.speed * Time.deltaTime);

        if (Vector3.Distance(guard.transform.position, targetWaypoint.position) < 0.2f)
        {
            guard.currentWaypointIndex = (guard.currentWaypointIndex + 1) % guard.waypoints.Length;
        }
    }
    public void Exit(GuardAgent guard) {}
}


public class ChaseState : IState
{
    public void Enter(GuardAgent guard) {
        guard.SetColor(Color.orange);
        
        Debug.Log("<color=red>[FSM State Machine]</color> Охранник: 'Враг замечен! Вызываю подкрепление!'");
    }
    
    public void Update(GuardAgent guard)
    {
        if (guard.player == null) return;

        float distanceToPlayer = Vector3.Distance(guard.transform.position, guard.player.position);

        if (distanceToPlayer <= guard.attackRange)
        {
            guard.ChangeState(new AttackState());
            return;
        }

        if (distanceToPlayer > guard.detectionRadius)
        {
            guard.ChangeState(new SearchState());
            return;
        }

        guard.transform.position = Vector3.MoveTowards(guard.transform.position, guard.player.position, guard.speed * 1.5f * Time.deltaTime);
    }
    public void Exit(GuardAgent guard) {}
}


public class AttackState : IState
{
    public void Enter(GuardAgent guard) { guard.SetColor(Color.red); }
    
    public void Update(GuardAgent guard)
    {
        if (guard.health <= guard.fleeHealthThreshold)
        {
            guard.ChangeState(new FleeState());
            return;
        }

        if (guard.player == null) return;
        
        float distanceToPlayer = Vector3.Distance(guard.transform.position, guard.player.position);
        if (distanceToPlayer > guard.attackRange && distanceToPlayer <= guard.detectionRadius)
        {
            guard.ChangeState(new ChaseState());
        }
    }
    public void Exit(GuardAgent guard) {}
}


public class SearchState : IState
{
    public void Enter(GuardAgent guard) 
    { 
        guard.SetColor(Color.yellow); 
        guard.searchTimer = guard.searchDuration;
    }
    
    public void Update(GuardAgent guard)
    {
        if (guard.player != null && Vector3.Distance(guard.transform.position, guard.player.position) <= guard.detectionRadius)
        {
            guard.ChangeState(new ChaseState());
            return;
        }

        guard.searchTimer -= Time.deltaTime;
        if (guard.searchTimer <= 0)
        {
            guard.ChangeState(new PatrolState());
        }
    }
    public void Exit(GuardAgent guard) {}
}

public class FleeState : IState
{
    public void Enter(GuardAgent guard) { guard.SetColor(Color.blue); }
    
    public void Update(GuardAgent guard)
    {
        if (guard.player == null) return;

        Vector3 fleeDirection = (guard.transform.position - guard.player.position).normalized;
        guard.transform.position += fleeDirection * guard.speed * 1.2f * Time.deltaTime;
    }
    public void Exit(GuardAgent guard) {}
}