public class Damage : Effect
{
    readonly int power;

    public Damage(int power)
    {
        this.power = power;
    }

    public override void ApplyEffect(Fighter user)
    {
        user.Target.Stats[StatisticManager.StatisticId.HP].CurrentValue -= power;
    }
}
