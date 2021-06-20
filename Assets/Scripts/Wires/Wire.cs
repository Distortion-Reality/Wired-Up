using System.Collections;
using UnityEngine;
using Photon.Bolt;

public class Wire : EntityBehaviour<IPlayerState>
{
    Player player;
    WireHead wireHead;
    WireBody wireBody;

    float defaultLength;
    Quaternion defaultLocalRotation;

    bool connectWhenDisconnected = false;
    
    readonly float extendingSpeed = 30f,
        retractingSpeed = 60f;

    float Length => wireHead.Length + wireBody.Length;

    float Extension => Length - defaultLength;

    public override void Attached()
    {
        Init();

        if (entity.IsOwner)
            state.wireActive = gameObject.activeSelf;

        state.AddCallback("wireActive", ActiveChanged);
    }

    void ActiveChanged()
    {
        if (!entity.IsOwner)
            gameObject.SetActive(state.wireActive);
    }

    void Init()
    {
        player = GetComponentInParent<Player>();

        wireHead = GetComponentInChildren<WireHead>(true);
        wireBody = GetComponentInChildren<WireBody>(true);
        wireHead.Init(this, wireBody);
        wireBody.Init(this, wireHead);

        defaultLength = Length;
        defaultLocalRotation = transform.localRotation;
    }

    public void Connect()
    {
        if (player.FighterAbilityStatus == Fighter.AbilityStatus.DISCONNECTING)
            StartCoroutine(ConnectWhenDisconnected());
        else
        {
            gameObject.SetActive(true);
            StartConnecting();
        }
    }

    public void StayConnected()
    {
        StartCoroutine(AdjustExtension());
    }

    public void Disconnect()
    {
        player.FighterAbilityStatus = Fighter.AbilityStatus.DISCONNECTING;
        StartCoroutine(Retract());
    }

    void StartConnecting()
    {
        player.FighterAbilityStatus = Fighter.AbilityStatus.CONNECTING;
        StartCoroutine(Extend());
    }

    float ExtensionThisFrame(float extendingSpeed)
    {
        return extendingSpeed * Time.deltaTime;
    }

    void ApplyExtension(float extension)
    {
        Vector3 localExtensionVector = Vector3.up * extension / transform.localScale.y;

        wireHead.ApplyLocalTranslation(localExtensionVector);
        wireBody.ApplyLocalScale(localExtensionVector);
    }

    IEnumerator ConnectWhenDisconnected()
    {
        connectWhenDisconnected = true;
        yield return new WaitUntil(() => player.FighterAbilityStatus != Fighter.AbilityStatus.DISCONNECTING);

        connectWhenDisconnected = false;
        StartConnecting();
    }

    IEnumerator Extend()
    {
        while (player.FighterAbilityStatus == Fighter.AbilityStatus.CONNECTING)
        {
            if (Length >= player.Stats[StatisticManager.StatisticId.Lng].CurrentValue)
                player.EndAbility();
            else
            {
                ApplyExtension(ExtensionThisFrame(extendingSpeed));
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
        yield return new WaitForEndOfFrame();
        
        while (Extension > 0)
        {
            transform.rotation = disconnectingRotation;
            float retraction = ExtensionThisFrame(retractingSpeed);

            if (retraction >= Extension)
            {
                wireHead.ResetLocalPosition();
                wireBody.ResetLocalScale();
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

    void OnEnable()
    {
        if (entity.IsOwner)
            state.wireActive = true;
    }

    void OnDisable()
    {
        if (entity.IsOwner)
            state.wireActive = false;
    }
}
