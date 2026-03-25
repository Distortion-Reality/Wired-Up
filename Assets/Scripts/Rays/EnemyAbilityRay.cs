using Fusion;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
public class EnemyAbilityRay : NetworkBehaviour
{
    const float speed = 20f;

    Fighter user;
    Ability ability;

    public void FireRay(Fighter user, Ability ability)
    {
        this.user = user;
        this.ability = ability;

        if (Object.HasStateAuthority)
        {
            StartCoroutine(MoveRay());
        }
    }

    IEnumerator MoveRay()
    {
        float totalDistance = 0f;

        while (totalDistance < user.AbilityRange && user)
        {
            Vector3 translation = speed * Time.deltaTime * Vector3.forward;
            transform.Translate(translation, Space.Self);
            totalDistance += Vector3.Magnitude(translation);

            yield return null;
        }

        Runner.Despawn(Object);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Object || !Object.HasStateAuthority)
            return;

        if (user == null || other.gameObject == user.gameObject)
            return;

        if (other.CompareTag("Player"))
        {
            Player target = other.GetComponent<Player>();
            ability.DoAbility(user, target);
        }

        Runner.Despawn(Object);
    }
}
