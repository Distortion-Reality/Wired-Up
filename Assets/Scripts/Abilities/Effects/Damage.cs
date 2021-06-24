using UnityEngine;

public class Damage : Effect
{
    readonly int power;

    public Damage(int power)
    {
        this.power = power;
    }

    protected override void ApplySingleEffect(Fighter user, Fighter target)
    {
        target.ChangeHP(- CalculateDamage(user, target));
    }

    int CalculateDamage(Fighter user, Fighter target)
    {
        int userInt = user.Stats[StatisticManager.StatisticId.Int].CurrentValue;
        int targetArm = target.Stats[StatisticManager.StatisticId.Arm].CurrentValue;
        float randomVal1 = Random.Range(0.8f, 1f);
        float randomVal2 = Random.Range(0.8f, 1f);

        return Mathf.RoundToInt(userInt * power * randomVal1 / (targetArm * randomVal2));
    }
}
