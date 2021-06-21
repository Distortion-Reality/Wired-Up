public class Healing : Effect
{
    readonly int amount;

    public Healing(int amount)
    {
        this.amount = amount;
    }

    public override void ApplyEffect(Fighter user)
    {
        user.Target.Heal(amount);
    }
}
