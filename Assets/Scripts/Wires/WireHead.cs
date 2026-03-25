using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WireHead : MonoBehaviour
{
    Player player;

    Vector3 defaultLocalPosition;
    float defaultLength;

    public float Length => defaultLength * transform.localScale.y;

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
        if (!player.PlayerEntity.Object.HasInputAuthority || other.CompareTag("Terrain"))
            return;

        switch (player.FighterStatusLocal)
        {
            case Fighter.Status.Waiting:
                if (other.gameObject != player.Target.gameObject && !other.GetComponent<WireHead>())
                    player.InterruptWaiting();
                break;

            case Fighter.Status.Connecting:
                if (other.CompareTag(player.Target.tag) && (other.CompareTag("Enemy") || other.CompareTag("Player") ||
                    other.CompareTag("Interactable")))
                    player.EnqueueUserAbilityToTarget(other.GetComponent<Fighter>());
                else
                    player.EndAbility();
                break;
        }
    }
}
