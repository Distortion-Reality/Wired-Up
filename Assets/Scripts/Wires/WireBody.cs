using Fusion;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkObject))]
public class WireBody : NetworkBehaviour
{
    Player player;

    Vector3 defaultLocalScale;
    float defaultLocalLength, defaultLength;

    Vector3 LocalScale
    {
        get => transform.parent.localScale;
        set => transform.parent.localScale = value;
    }

    public float Length => defaultLength * LocalScale.y;

    public override void Spawned()
    {
        defaultLocalScale = LocalScale;
    }

    public void Init(Wire wire, WireHead wireHead)
    {
        player = GetComponentInParent<Player>();

        defaultLocalLength = GetComponent<MeshFilter>().mesh.bounds.size.y;
        defaultLength = defaultLocalLength * wire.transform.localScale.y;

        Collider wireBodyCollider = GetComponent<Collider>();
        Physics.IgnoreCollision(wireBodyCollider, player.GetComponent<Collider>());
        Physics.IgnoreCollision(wireBodyCollider, wireHead.GetComponent<Collider>());
    }

    public void ApplyLocalScale(Vector3 scale)
    {
        LocalScale += scale / defaultLocalLength;
    }

    public void ResetLocalScale()
    {
        LocalScale = defaultLocalScale;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!Object.HasInputAuthority || other.CompareTag("Terrain"))
            return;

        switch (player.FighterStatusLocal)
        {
            case Fighter.Status.Waiting:
                if (other.gameObject != player.Target.gameObject)
                    player.InterruptWaiting();
                break;

            case Fighter.Status.Connecting:
                player.EndAbility();
                break;
        }
    }
}