using System.Collections;
using UnityEngine;

public class GreenAssist3 : Ability
{
    const float percentage = 0.05f;
    const float rate = 1f;
    const int times = 5;
    const float radius = 4f;

    public override int Energy => 40;
    public override AbilityId Id => AbilityId.GreenAssist3;

    public override void DoAbility(Fighter user, Fighter target)
    {
        int selfDamage = user.Stats[StatisticManager.StatisticId.HP].CurrentValue / 8;
        Effects.ApplyDamage(user, user, selfDamage);
        user.StartCoroutine(WaitForParticles(user, target, user.DamageParticles));
    }

    IEnumerator WaitForParticles(Fighter user, Fighter target, ParticlesId particlesId)
    {
        float duration = Object.FindObjectOfType<ParticlesManager>()
            .GetParticlesPrefab(particlesId).GetComponent<ParticleSystem>().main.duration;
        yield return new WaitForSeconds(duration);

        target.TargetAbilityManager.StartCoroutine(HealingOverTimeAOE(user, target));

        user.EndAbility();
    }

    IEnumerator HealingOverTimeAOE(Fighter user, Fighter target)
    {
        for (int i = 0; i < times; i++)
        {
            Collider[] colliders = Effects.AreaOfEffect(target, radius);
            foreach (Collider collider in colliders)
            {
                if (collider)
                {
                    Fighter fighter = collider.GetComponent<Fighter>();
                    Effects.CalculateAndApplyHealing(user, fighter, percentage);
                }
            }

            yield return new WaitForSeconds(rate);
        }
    }
}
