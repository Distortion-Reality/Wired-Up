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
        WireStart();

        if (entity.IsOwner)
            state.wireActive = gameObject.activeSelf;

        state.AddCallback("wireActive", ActiveChanged);
    }

    void ActiveChanged()
    {
        if (!entity.IsOwner)
            gameObject.SetActive(state.wireActive);
    }

    void WireStart()
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
        if (player.FighterStatus == Fighter.Status.Disconnecting)
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
        player.FighterStatus = Fighter.Status.Disconnecting;
        StartCoroutine(Retract());
    }

    void StartConnecting()
    {
        player.FighterStatus = Fighter.Status.Connecting;
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

    void ResetExtension()
    {
        wireHead.ResetLocalPosition();
        wireBody.ResetLocalScale();
    }

    IEnumerator ConnectWhenDisconnected()
    {
        connectWhenDisconnected = true;
        yield return new WaitUntil(() => player.FighterStatus != Fighter.Status.Disconnecting);

        connectWhenDisconnected = false;
        StartConnecting();
    }

    IEnumerator Extend()
    {
        while (player.FighterStatus == Fighter.Status.Connecting)
        {
            if (Length >= player.AbilityRange)
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
        while (player.FighterStatus == Fighter.Status.Waiting ||
            player.FighterStatus == Fighter.Status.Using)
        {
            ApplyExtension(player.TargetDistance - Length);

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
                ResetExtension();
            else
            {
                ApplyExtension(- retraction);

                yield return null;
            }
        }

        transform.localRotation = defaultLocalRotation;

        player.FighterStatus = Fighter.Status.Free;

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
        if (entity.IsAttached && entity.IsOwner)
            state.wireActive = false;
    }
}
