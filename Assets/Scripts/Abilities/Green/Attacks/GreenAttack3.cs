using System.Collections;
using UnityEngine;

public class GreenAttack3 : Ability
{
    const int power = 10;

    public override int Energy => 30;
    public override AbilityId Id => AbilityId.GreenAttack3;

    public override void DoAbility(Fighter user, Fighter target)
    {
        Effects.CalculateAndApplyDamage(user, target, power);
        user.StartCoroutine(WaitForParticles(user, target, user.DamageParticles));
    }

    IEnumerator WaitForParticles(Fighter user, Fighter target, ParticlesId particlesId)
    {
        float duration = Object.FindObjectOfType<ParticlesManager>()
            .GetParticlesPrefab(particlesId).GetComponent<ParticleSystem>().main.duration;
        yield return new WaitForSeconds(duration / 2);

        Effects.Stun(user, target);
        DelayEndAbility(user, ParticlesId.Stun);
    }
}
