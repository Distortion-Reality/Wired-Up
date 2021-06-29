using System.Collections;
using UnityEngine;

public class PurpleAssist3 : Ability
{
    const float percentage = 0.25f;

    public override int Energy => 30;
    public override AbilityId Id => AbilityId.PurpleAssist3;

    public override void DoAbility(Fighter user, Fighter target)
    {
        int hp = Mathf.RoundToInt(percentage * user.Stats[StatisticManager.StatisticId.HP].CurrentValue);
        Effects.ApplyDamage(user, user, hp);
        user.StartCoroutine(WaitForParticles(user, target, hp, user.DamageParticles));
    }

    IEnumerator WaitForParticles(Fighter user, Fighter target, int hp, ParticlesId particlesId)
    {
        float duration = Object.FindObjectOfType<ParticlesManager>()
            .GetParticlesPrefab(particlesId).GetComponent<ParticleSystem>().main.duration;
        yield return new WaitForSeconds(duration / 2);

        Effects.ApplyHealing(user, target, hp);
        DelayEndAbility(user, ParticlesId.Heal);
    }
}
