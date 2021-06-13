using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WireHead : EntityBehaviour<IPlayerState>
{
    Player player;

    Vector3 defaultLocalPosition;
    float defaultLength;

    public float Length => defaultLength * transform.localScale.y;

    public override void Attached()
    {
        state.SetTransforms(state.wireHeadTransform, transform);
    }

    public void Init(Wire wire, WireBody wireBody)
    {
        player = GetComponentInParent<Player>();

        defaultLocalPosition = transform.localPosition;
        defaultLength = GetComponent<MeshFilter>().mesh.bounds.size.y * wire.transform.localScale.y;

        Collider wireHeadCollider = GetComponent<Collider>();
        Physics.IgnoreCollision(wireHeadCollider, player.GetComponent<Collider>());
        Physics.IgnoreCollision(wireHeadCollider, wireBody.GetComponent<Collider>());
    }

    public void ApplyLocalTranslation(Vector3 translation)
    {
        transform.localPosition += translation;
    }

    public void ResetLocalPosition()
    {
        transform.localPosition = defaultLocalPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Terrain"))
            return;

        switch (player.FighterAbilityStatus)
        {
            case Fighter.AbilityStatus.WAITING:
                if (other.gameObject != player.Target.gameObject)
                    player.InterruptWaiting();
                break;

            case Fighter.AbilityStatus.CONNECTING:
                if (other.CompareTag("Enemy") || other.CompareTag("Player"))
                    player.EnqueueUserAbilityToTarget(other.GetComponent<Fighter>());
                else
                    player.EndAbility();
                break;
        }
    }
}