using Fusion;
using System.Collections;
using UnityEngine;

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
        SpawnParticles(target, damageParticlesId, particlesColor);
        ChangeHP(target, -damage);
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
            CalculateAndApplyDamage(user, target, power);
            yield return new WaitForSeconds(rate);
        }
    }

    public static int Healing(Fighter target, float percentage)
    {
        return Mathf.RoundToInt(percentage * target.Stats[StatisticManager.StatisticId.HP].BaseValue);
    }

    public static void ApplyHealing(Fighter user, Fighter target, int healing)
    {
        SpawnParticles(target, ParticlesId.Heal, user.CharacterUnityColor);
        ChangeHP(target, healing);
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
        SpawnParticles(target, ParticlesId.Buff, user.CharacterUnityColor);

    }

    public static void DebuffStat(Fighter user, Fighter target, StatisticManager.StatisticId statId, int stages)
    {
        target.TargetAbilityManager.StartCoroutine(ApplyStatBuff(target, statId, -stages));
        SpawnParticles(target, ParticlesId.Debuff, user.CharacterUnityColor);
    }

    static IEnumerator ApplyStatBuff(Fighter target, StatisticManager.StatisticId statId, int stages)
    {
        target.ChangeStat(statId, stages);
        yield return new WaitForSeconds(10f);
        target.ChangeStat(statId, -stages);
    }

    public static void Stun(Fighter user, Fighter target, float time = 5f)
    {
        target.ApplyStatus(Fighter.Status.Stunned, time);
        SpawnParticles(target, ParticlesId.Stun, user.CharacterUnityColor);
    }

    public static void BlockMovements(Fighter user, Fighter target, float time)
    {
        target.TargetAbilityManager.StartCoroutine(ApplyBlockMovements(target, time));
        SpawnParticles(target, ParticlesId.Stun, user.CharacterUnityColor);
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

    public static void SpawnParticles(Fighter target, ParticlesId particlesId, Color particlesColor)
    {
        if (!target.Entity.Object || !target.Entity.Object.IsValid)
            return;

        Vector3 position = NetworkManager.Instance.ParticlesManager.GetParticlesPosition(particlesId, target);
        NetworkManager.RPC_SpawnParticle(target.Entity.Runner, target.Entity.Id,
            position, target.transform.rotation,
            particlesId, particlesColor);
    }

    public static void FireRay(Enemy user, Quaternion rotation, Ability ability)
    {
        if (!user.Entity.HasStateAuthority)
            return;

        NetworkObject ray = user.Entity.Runner.Spawn(
            user.EnemyEntity.enemyAttackRayPrefab,
            user.FirePosition,
            rotation
            );

        ray.GetComponent<EnemyAbilityRay>().FireRay(user, ability);
    }
}
