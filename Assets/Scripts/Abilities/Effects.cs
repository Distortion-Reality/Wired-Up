using System.Collections;
using UnityEngine;

public static class Effects
{
    public static int ApplyDamage(Fighter user, Fighter target, int power)
    {
        int damage = Damage(user, target, power);
        ChangeHP(target, -damage);
        return damage;
    }

    public static int Damage(Fighter user, Fighter target, int power)
    {
        int userInt = user.Stats[StatisticManager.StatisticId.Int].CurrentValue;
        int targetArm = target.Stats[StatisticManager.StatisticId.Arm].CurrentValue;
        float randomVal1 = Random.Range(0.8f, 1f);
        float randomVal2 = Random.Range(0.8f, 1f);

        return Mathf.RoundToInt(userInt * power * randomVal1 / (targetArm * randomVal2));
    }

    public static int ApplyHealing(Fighter target, float percentage)
    {
        int healing = Healing(target, percentage);
        ChangeHP(target, healing);
        return healing;
    }

    public static int Healing(Fighter target, float percentage)
    {
        return Mathf.RoundToInt(percentage * target.Stats[StatisticManager.StatisticId.HP].BaseValue);
    }

    public static void ChangeHP(Fighter target, int change)
    {
        target.ChangeStat(StatisticManager.StatisticId.HP, change);
    }

    public static void BuffStat(Fighter target, StatisticManager.StatisticId statId, int stages)
    {
        target.TargetAbilityManager.StartCoroutine(ApplyStatBuff(target, statId, stages));
    }

    static IEnumerator ApplyStatBuff(Fighter target, StatisticManager.StatisticId statId, int stages)
    {
        target.ChangeStat(statId, stages);
        yield return new WaitForSeconds(45f);
        target.ChangeStat(statId, - stages);
    }

    public static Collider[] AreaOfEffect(Fighter target, float radius)
    {
        Collider[] colliders = new Collider[3];
        Physics.OverlapSphereNonAlloc(target.BasePosition, radius, colliders,
            LayerMask.GetMask(target.GetType().Name));

        return colliders;
    }
}
