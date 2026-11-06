public interface IState
{
    void Enter(GuardAgent guard);
    void Update(GuardAgent guard);
    void Exit(GuardAgent guard);
}
