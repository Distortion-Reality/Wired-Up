using System.Collections;
using UnityEngine;
using Photon.Bolt;

public static class Effects
{
    public static int Damage(Fighter user, Fighter target, int power)
    {
        int userInt = user.Stats[StatisticManager.StatisticId.Int].CurrentValue;

        return Damage(userInt, target, power);
    }

    static int Damage(int userInt, Fighter target, int power)
    {
        int targetArm = target.Stats[StatisticManager.StatisticId.Arm].CurrentValue;
        float randomVal1 = Random.Range(0.8f, 1f);
        float randomVal2 = Random.Range(0.8f, 1f);

        return Mathf.RoundToInt(userInt * power * randomVal1 / (targetArm * randomVal2));
    }

    public static void ApplyDamage(Fighter user, Fighter target, int damage)
    {
        ApplyDamage(user.DamageParticles, user.CharacterUnityColor, target, damage);
    }

    static void ApplyDamage(ParticlesId damageParticlesId, Color particlesColor, Fighter target, int damage)
    {
        ChangeHP(target, -damage);
        SendSpawnParticleEvent(damageParticlesId, particlesColor, target.transform.position);
    }

    public static int CalculateAndApplyDamage(Fighter user, Fighter target, int power)
    {
        return CalculateAndApplyDamage(user.Stats[StatisticManager.StatisticId.Int].CurrentValue,
            user.DamageParticles, user.CharacterUnityColor, target, power);
    }

    public static int CalculateAndApplyDamage(int userInt, ParticlesId damageParticlesId, Color particlesColor, Fighter target, int power)
    {
        int damage = Damage(userInt, target, power);
        ApplyDamage(damageParticlesId, particlesColor, target, damage);
        return damage;
    }

    public static void DamageOverTime(Fighter user, Fighter target, int power, float rate, float times)
    {
        target.TargetAbilityManager.StartCoroutine(ApplyDamageOverTime(user, target, power, rate, times));
    }

    static IEnumerator ApplyDamageOverTime(Fighter user, Fighter target, int power, float rate, float times)
    {
        for (int i = 0; i < times; i++)
        {
            Effects.CalculateAndApplyDamage(user, target, power);
            yield return new WaitForSeconds(rate);
        }
    }

    public static int Healing(Fighter target, float percentage)
    {
        return Mathf.RoundToInt(percentage * target.Stats[StatisticManager.StatisticId.HP].BaseValue);
    }

    public static void ApplyHealing(Fighter user, Fighter target, int healing)
    {
        ChangeHP(target, healing);
        SendSpawnParticleEvent(ParticlesId.Heal, user.CharacterUnityColor, target.transform.position);
    }

    public static int CalculateAndApplyHealing(Fighter user, Fighter target, float percentage)
    {
        int healing = Healing(target, percentage);
        ApplyHealing(user, target, healing);
        return healing;
    }

    static void ChangeHP(Fighter target, int change)
    {
        target.ChangeStat(StatisticManager.StatisticId.HP, change);
    }

    public static void BuffStat(Fighter user, Fighter target, StatisticManager.StatisticId statId, int stages)
    {
        target.TargetAbilityManager.StartCoroutine(ApplyStatBuff(target, statId, stages));
        SendSpawnParticleEvent(ParticlesId.Buff, user.CharacterUnityColor, target.transform.position);
    }

    public static void DebuffStat(Fighter user, Fighter target, StatisticManager.StatisticId statId, int stages)
    {
        target.TargetAbilityManager.StartCoroutine(ApplyStatBuff(target, statId, -stages));
        SendSpawnParticleEvent(ParticlesId.Debuff, user.CharacterUnityColor, target.transform.position);
    }

    static IEnumerator ApplyStatBuff(Fighter target, StatisticManager.StatisticId statId, int stages)
    {
        target.ChangeStat(statId, stages);
        yield return new WaitForSeconds(45f);
        target.ChangeStat(statId, -stages);
    }

    public static void Stun(Fighter user, Fighter target, float time = 5f)
    {
        target.ApplyStatus(Fighter.Status.Stunned, time);
        SendSpawnParticleEvent(ParticlesId.Stun, user.CharacterUnityColor, target.transform.position);
    }

    public static void BlockMovements(Fighter user, Fighter target, float time)
    {
        target.TargetAbilityManager.StartCoroutine(ApplyBlockMovements(target, time));
        SendSpawnParticleEvent(ParticlesId.Stun, user.CharacterUnityColor, target.transform.position);
    }

    static IEnumerator ApplyBlockMovements(Fighter target, float time)
    {
        target.MovementsBlocked = true;
        yield return new WaitForSeconds(time);
        target.MovementsBlocked = false;
    }

    public static Collider[] AreaOfEffect(Fighter target, float radius)
    {
        Collider[] colliders = new Collider[3];
        Physics.OverlapSphereNonAlloc(target.BottomPosition, radius, colliders,
            LayerMask.GetMask(target.GetType().Name));

        return colliders;
    }

    static void SendSpawnParticleEvent(ParticlesId particlesId, Color particlesColor, Vector3 position)
    {
        SpawnParticleEvent.Post(ReliabilityModes.ReliableOrdered, (int)particlesId, particlesColor, position);
    }

    public static void FireRay(Fighter user, Ability ability)
    {
        // EnemyAttackRay ray = bolt entity
        GameObject ray = Object.Instantiate(Object.FindObjectOfType<AbilityManager>().enemyAbilityRay,
            user.transform.position, user.transform.rotation);
        ray.GetComponent<EnemyAbilityRay>().FireRay(user, ability);
    }
}
