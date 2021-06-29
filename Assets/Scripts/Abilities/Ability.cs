using System.Collections;
using UnityEngine;

public abstract class Ability
{
    public abstract int Energy { get; }

    public virtual AbilityId Id => AbilityId.None;

    public abstract void DoAbility(Fighter user, Fighter target);

    protected void DelayEndAbility(Fighter user, ParticlesId particlesId)
    {
        user.StartCoroutine(WaitForParticles(user, particlesId));
    }

    IEnumerator WaitForParticles(Fighter user, ParticlesId particlesId)
    {
        float duration = Object.FindObjectOfType<ParticlesManager>()
            .GetParticlesPrefab(particlesId).GetComponent<ParticleSystem>().main.duration;
        yield return new WaitForSeconds(duration);

        user.EndAbility();
    }
}
