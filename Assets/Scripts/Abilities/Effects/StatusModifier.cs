public class StatusModifier : Effect
{
    readonly Fighter.Status status;
    readonly float time;

    public StatusModifier(Fighter.Status status, float time)
    {
        this.status = status;
        this.time = time;
    }

    protected override void ApplySingleEffect(Fighter user, Fighter target)
    {
        target.ApplyStatus(status, time);
    }
}
