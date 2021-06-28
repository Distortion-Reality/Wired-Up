using System.Collections;
using UnityEngine;
using Photon.Bolt;

public static class Effects
{
    static void SendSpawnParticleEvent(Fighter user, ParticlesId id, Vector3 position)
    {
        Color color = ((Player) user).Character.UnityColor();
        SpawnParticleEvent.Post(ReliabilityModes.ReliableOrdered, (int) id, color, position);
    }

    public static void ApplyDamage(Fighter user, Fighter target, int power)
    {
        ChangeHP(user, target, -Damage(user, target, power));
    }

    public static int Damage(Fighter user, Fighter target, int power)
    {
        int userInt = user.Stats[StatisticManager.StatisticId.Int].CurrentValue;
        int targetArm = target.Stats[StatisticManager.StatisticId.Arm].CurrentValue;
        float randomVal1 = Random.Range(0.8f, 1f);
        float randomVal2 = Random.Range(0.8f, 1f);

        return Mathf.RoundToInt(userInt * power * randomVal1 / (targetArm * randomVal2));
    }

    public static int ApplyHealing(Fighter user, Fighter target, float percentage)
    {
        int healing = Healing(target, percentage);
        ChangeHP(user, target, healing);
        return healing;
    }

    public static int Healing(Fighter target, float percentage)
    {
        return Mathf.RoundToInt(percentage * target.Stats[StatisticManager.StatisticId.HP].BaseValue);
    }

    public static void ChangeHP(Fighter user, Fighter target, int change)
    {
        target.ChangeStat(StatisticManager.StatisticId.HP, change);

        if (change > 0)
            SendSpawnParticleEvent(user, ParticlesId.Heal, target.transform.position);
        else
            SendSpawnParticleEvent(user, ParticlesId.Damage, target.transform.position);
    }

    public static void BuffStat(Fighter user, Fighter target, StatisticManager.StatisticId statId, int stages)
    {
        target.TargetAbilityManager.StartCoroutine(ApplyStatBuff(user, target, statId, stages));
    }

    static IEnumerator ApplyStatBuff(Fighter user, Fighter target, StatisticManager.StatisticId statId, int stages)
    {
        target.ChangeStat(statId, stages);

        if (stages > 0)
            SendSpawnParticleEvent(user, ParticlesId.Buff, target.transform.position);
        else
            SendSpawnParticleEvent(user, ParticlesId.Debuff, target.transform.position);

        yield return new WaitForSeconds(45f);
        target.ChangeStat(statId, - stages);
    }

    public static void ApplyStun(Fighter user, Fighter target)
    {
        target.ApplyStatus(Fighter.Status.Stunned);

        SendSpawnParticleEvent(user, ParticlesId.Stun, target.transform.position);
    }

    public static Collider[] AreaOfEffect(Fighter target, float radius)
    {
        Collider[] colliders = new Collider[3];
        Physics.OverlapSphereNonAlloc(target.BasePosition, radius, colliders,
            LayerMask.GetMask(target.GetType().Name));

        return colliders;
    }
}
