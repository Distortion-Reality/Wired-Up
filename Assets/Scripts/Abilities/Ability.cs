using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    readonly List<Effect> effects;
    readonly int eng;

    public Ability(List<Effect> effects, int eng)
    {
        this.effects = effects;
        this.eng = eng;
    }

    public int Eng => eng;

    public void DoAbility(Fighter user)
    {
        StartCoroutine(ApplyEffects(user));
    }

    IEnumerator ApplyEffects(Fighter user)
    {
        foreach (Effect effect in effects)
            effect.ApplyEffect(user);

        yield return new WaitForSeconds(2f);

        user.EndAbility();
    }
}
