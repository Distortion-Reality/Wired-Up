using System.Collections;
using UnityEngine;

public class PurpleAttack1 : Ability
{
    const int power = 10;

    public override int Energy => 10;
    public override AbilityId Id => AbilityId.PurpleAttack1;

    public override void DoAbility(Fighter user, Fighter target)
    {
        int targetHP = target.Stats[StatisticManager.StatisticId.HP].CurrentValue;
        int damage = Effects.CalculateAndApplyDamage(user, target, power);
        user.StartCoroutine(WaitForParticles(user, targetHP, damage, user.DamageParticles));
    }

    IEnumerator WaitForParticles(Fighter user, int targetHP, int damage, ParticlesId particlesId)
    {
        float duration = Object.FindObjectOfType<ParticlesManager>()
            .GetParticlesPrefab(particlesId).GetComponent<ParticleSystem>().main.duration;
        yield return new WaitForSeconds(duration / 2);

        Effects.ApplyHealing(user, user, Mathf.Min(targetHP, damage));
        DelayEndAbility(user, ParticlesId.Heal);
    }
}
