using System.Collections;
using UnityEngine;

public abstract class Effect : ScriptableObject
{
    readonly bool self = false,
        aoe = false;
    readonly float radius = 0f,
        time = 0f, rate = 0f;

    public void ApplyEffect(Fighter user)
    {
        Fighter target = self ? user : user.Target;

        if (aoe)
        {
            Collider[] targetsColliders = new Collider[3];
            Physics.OverlapSphereNonAlloc(target.transform.position, radius, targetsColliders,
                LayerMask.GetMask(target.GetType().Name));
            foreach (Collider targetCollider in targetsColliders)
            {
                Fighter targetFighter = targetCollider.GetComponent<Fighter>();
                targetFighter.TargetAbilityManager.StartCoroutine(ApplyEffectOverTime(user, targetFighter));
            }
        }
        else
            target.TargetAbilityManager.StartCoroutine(ApplyEffectOverTime(user, target));
    }

    protected abstract void ApplySingleEffect(Fighter user, Fighter target);

    IEnumerator ApplyEffectOverTime(Fighter user, Fighter target)
    {
        float elapsedTime = 0f;

        do
        {
            ApplySingleEffect(user, target);

            yield return new WaitForSeconds(rate);

            elapsedTime += rate;
        } while (elapsedTime < time);
    }
}
