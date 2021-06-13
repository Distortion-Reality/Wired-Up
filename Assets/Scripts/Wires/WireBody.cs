using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WireBody : EntityBehaviour<IPlayerState>
{
    Player player;

    Vector3 defaultLocalScale;
    float defaultLocalLength, defaultLength;

    Vector3 LocalScale
    {
        get => state.wireBodyScale;
        set => state.wireBodyScale = value;
    }

    public float Length => defaultLength * LocalScale.y;

    void ScaleChanged()
    {
        transform.parent.localScale = state.wireBodyScale;
    }

    public override void Attached()
    {
        if (entity.IsOwner)
        {
            state.wireBodyScale = transform.parent.localScale;
        }
        state.AddCallback("wireBodyScale", ScaleChanged);

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
        if (!entity.IsOwner)
            return;

        if (other.CompareTag("Terrain"))
            return;

        switch (player.FighterAbilityStatus)
        {
            case Fighter.AbilityStatus.WAITING:
                if (other.gameObject != player.Target.gameObject)
                    player.InterruptWaiting();
                break;

            case Fighter.AbilityStatus.CONNECTING:
                player.EndAbility();
                break;
        }
    }
}