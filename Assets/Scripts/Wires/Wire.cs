using Fusion;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Wire : NetworkBehaviour
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

    public override void Spawned()
    {
        WireStart();
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
        if (player.FighterStatusLocal == Fighter.Status.Disconnecting)
            StartCoroutine(ConnectWhenDisconnected());
        else
        {
            gameObject.SetActive(true);
            StartConnecting();
        }
    }

    public void StayConnected()
    {
        player.FighterStatusLocal = Fighter.Status.Waiting;
        StartCoroutine(AdjustExtension());
    }

    public void Disconnect()
    {
        if (!gameObject.activeSelf)
            return;
        if (!Object.HasInputAuthority)
            player.FighterStatusLocal = Fighter.Status.Disconnecting;
        StartCoroutine(Retract());
    }

    void StartConnecting()
    {
        player.FighterStatusLocal = Fighter.Status.Connecting;
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
        yield return new WaitUntil(() => player.FighterStatusLocal != Fighter.Status.Disconnecting);

        connectWhenDisconnected = false;
        StartConnecting();
    }

    IEnumerator Extend()
    {
        while (player.FighterStatusLocal == Fighter.Status.Connecting)
        {
            if (player.PlayerEntity.HasInputAuthority && Length >= player.AbilityRange)
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
        while (player.FighterStatusLocal == Fighter.Status.Waiting ||
            player.FighterStatusLocal == Fighter.Status.Using)
        {
            ApplyExtension(player.TargetDistance - Length);
            transform.rotation = player.LookAtTargetRotation() * defaultLocalRotation;

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
                ApplyExtension(-retraction);

                yield return null;
            }
        }

        transform.localRotation = defaultLocalRotation;

        player.FighterStatusLocal = Fighter.Status.Free;

        if (!connectWhenDisconnected)
            gameObject.SetActive(false);
    }
}
