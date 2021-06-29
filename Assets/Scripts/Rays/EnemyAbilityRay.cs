using System.Collections;
using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyAbilityRay : MonoBehaviour
{
    const float speed = 2f;

    Fighter user;
    Ability ability;

    public void FireRay(Fighter user, Ability ability)
    {
        this.user = user;
        this.ability = ability;

        StartCoroutine(MoveRay());
    }

    IEnumerator MoveRay()
    {
        float startPosition = transform.localPosition.z;

        while (transform.localPosition.z - startPosition < user.AbilityRange && user)
        {
            transform.Translate(speed * Time.deltaTime * Vector3.forward, Space.Self);
            yield return null;
        }

        BoltNetwork.Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player target = other.GetComponent<Player>();
            ability.DoAbility(user, target);
        }
        
        BoltNetwork.Destroy(gameObject);
    }
}
