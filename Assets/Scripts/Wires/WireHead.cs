using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WireHead : MonoBehaviour
{
    Player player;
    Ability ability = null;

    Vector3 defaultLocalPosition;
    float defaultLength;

    public Ability Ability { set => ability = value; }

    public float Length => defaultLength * transform.localScale.y;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<Player>();

        defaultLocalPosition = transform.localPosition;

        Wire wire = GetComponentInParent<Wire>();
        defaultLength = GetComponent<MeshFilter>().mesh.bounds.size.y * wire.transform.localScale.y;

        Collider wireHeadCollider = GetComponent<Collider>();
        Physics.IgnoreCollision(wireHeadCollider, player.GetComponent<Collider>());
        Physics.IgnoreCollision(wireHeadCollider,
            wire.GetComponentInChildren<WireBody>().GetComponent<Collider>());
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
                    player.EnqueueUserAbilityToTarget(other.GetComponent<Fighter>(), ability);
                else
                    player.EndAbility();
                break;
        }
    }
}
