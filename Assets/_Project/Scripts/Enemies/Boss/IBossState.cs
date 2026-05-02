public interface IBossState
{
    void Enter(BossController boss);
    void Tick(BossController boss);
    void Exit(BossController boss);
}
