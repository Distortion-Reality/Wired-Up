using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WireBody : MonoBehaviour
{
    Player player;

    Vector3 defaultLocalScale;
    float defaultLocalLength, defaultLength;

    public float DefaultLocalLength { get => defaultLocalLength; }

    public Vector3 LocalScale
    {
        get => transform.parent.localScale;
        private set => transform.parent.localScale = value;
    }

    public float Length => defaultLength * LocalScale.y;

    public bool IsExtended => LocalScale.y > defaultLocalScale.y;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<Player>();

        defaultLocalScale = LocalScale;
        defaultLocalLength = GetComponent<MeshFilter>().mesh.bounds.size.y;

        Wire wire = GetComponentInParent<Wire>();
        defaultLength = defaultLocalLength * wire.transform.localScale.y;

        Collider wireBodyCollider = GetComponent<Collider>();
        Physics.IgnoreCollision(wireBodyCollider, player.GetComponent<Collider>());
        Physics.IgnoreCollision(wireBodyCollider,
            wire.GetComponentInChildren<WireHead>().GetComponent<Collider>());
    }

    public void ApplyLocalScale(Vector3 scale)
    {
        LocalScale += scale;
    }

    public void ResetLocalScale()
    {
        LocalScale = defaultLocalScale;
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
                player.EndAbility();
                break;
        }
    }
}
