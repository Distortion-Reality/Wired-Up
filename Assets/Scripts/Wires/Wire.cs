using System.Collections;
using UnityEngine;

public class Wire : MonoBehaviour
{
    Player player;
    WireHead wireHead;
    WireBody wireBody;

    Quaternion defaultLocalRotation;

    bool connectWhenDisconnected = false;
    
    readonly float extendingSpeed = 15f,
        retractingSpeed = 30f;

    float Length => wireHead.Length + wireBody.Length;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<Player>();
        wireHead = GetComponentInChildren<WireHead>();
        wireBody = GetComponentInChildren<WireBody>();

        defaultLocalRotation = transform.localRotation;
    }

    public void Connect(Ability ability)
    {
        if (player.FighterAbilityStatus == Fighter.AbilityStatus.DISCONNECTING)
            StartCoroutine(ConnectWhenDisconnected(ability));
        else
        {
            gameObject.SetActive(true);
            StartConnecting(ability);
        }
    }

    public void StayConnected()
    {
        StartCoroutine(AdjustExtension());
    }

    public void Disconnect()
    {
        player.FighterAbilityStatus = Fighter.AbilityStatus.DISCONNECTING;
        wireHead.Ability = null;
        StartCoroutine(Retract());
    }

    void StartConnecting(Ability ability)
    {
        player.FighterAbilityStatus = Fighter.AbilityStatus.CONNECTING;
        wireHead.Ability = ability;
        StartCoroutine(Extend());
    }

    void ApplyExtension(float extension)
    {
        Vector3 translation = Vector3.up * extension;
        Vector3 scale = translation / wireBody.DefaultLocalLength;

        wireHead.ApplyLocalTranslation(translation);
        wireBody.ApplyLocalScale(scale);
    }

    IEnumerator ConnectWhenDisconnected(Ability ability)
    {
        connectWhenDisconnected = true;
        yield return new WaitUntil(() => player.FighterAbilityStatus != Fighter.AbilityStatus.DISCONNECTING);

        connectWhenDisconnected = false;
        StartConnecting(ability);
    }

    IEnumerator Extend()
    {
        while (player.FighterAbilityStatus == Fighter.AbilityStatus.CONNECTING)
        {
            if (Length >= player.Stats[StatisticManager.StatisticId.Lng].CurrentValue)
                Disconnect();
            else
            {
                ApplyExtension(extendingSpeed * Time.deltaTime / transform.localScale.y);

                yield return null;
            }
        }
    }

    IEnumerator AdjustExtension()
    {
        while (player.FighterAbilityStatus == Fighter.AbilityStatus.WAITING ||
            player.FighterAbilityStatus == Fighter.AbilityStatus.USING)
        {
            float distance = Vector3.Magnitude(
                Vector3.ProjectOnPlane(player.Target.transform.position - player.transform.position, Vector3.up));

            ApplyExtension(distance - Length);

            yield return null;
        }
    }

    IEnumerator Retract()
    {
        Quaternion disconnectingRotation = transform.rotation;

        while (wireBody.IsExtended)
        {
            transform.rotation = disconnectingRotation;

            float wireBodyLocalExtension = wireBody.LocalScale.y;
            float retraction = retractingSpeed * Time.deltaTime / transform.localScale.y;

            if (wireBodyLocalExtension <= retraction)
            {
                wireBody.ResetLocalScale();
                wireHead.ResetLocalPosition();
            }
            else
            {
                ApplyExtension(- retraction);

                yield return null;
            }
        }

        transform.localRotation = defaultLocalRotation;

        player.FighterAbilityStatus = Fighter.AbilityStatus.FREE;

        if (!connectWhenDisconnected)
            gameObject.SetActive(false);
    }
}
