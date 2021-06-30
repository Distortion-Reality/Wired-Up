using System.Collections;
using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAbilityRay : EntityBehaviour<IEnemyRayState>
{
    const float speed = 20f;

    Fighter user;
    Ability ability;

    public override void Attached()
    {
        state.SetTransforms(state.transform, transform, transform);
    }

    public void FireRay(Fighter user, Ability ability)
    {
        this.user = user;
        this.ability = ability;

        StartCoroutine(MoveRay());
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

        BoltNetwork.Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == user.gameObject)
            return;

        if (other.CompareTag("Player"))
        {
            Player target = other.GetComponent<Player>();
            ability.DoAbility(user, target);
        }
        
        BoltNetwork.Destroy(gameObject);
    }
}
