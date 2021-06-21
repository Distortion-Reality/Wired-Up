using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

[RequireComponent(typeof(Player))]
public class PlayerManager : FighterManager<IPlayerState>
{
    private static int alliesIndex = 0;
    
    private Rigidbody rb;
    private Transform cam;
    private Canvas gui;
    public GameObject allyInfoPrefab;

    readonly float moveSpeed = 8f;
    float moveSpeedMultiplier = 1f;

    readonly int dashCostEnergy = 5;
    readonly float dashMultiplier = 2f,
        dashDurationSeconds = 0.5f,
        dashCooldownSeconds = 5f;
    float nextDashTime = 0f;

    public override void Attached()
    {
        base.Attached();

        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        gui = FindObjectOfType<Canvas>();

        if (entity.IsOwner)
        {
            healthBar = GameObject.Find("PlayerHealthBar").GetComponent<Slider>();
        }
        else // Ally
        {
            // Create ally UI
            Color allyColor = Color.green;
            GameObject allyInfo = Instantiate(allyInfoPrefab, allyInfoPrefab.transform.position, allyInfoPrefab.transform.rotation);
            Vector3 pos = allyInfo.transform.position;
            pos.Set(pos.x, pos.y + alliesIndex * 60 , pos.z);
            allyInfo.transform.SetParent(gui.transform, false);

            TMPro.TextMeshProUGUI allyName = allyInfo.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            allyName.text = entity.Source.RemoteEndPoint.SteamId.Id.ToString();

            allyInfo.gameObject.transform.Find("AllyPortrait").GetComponent<Image>().color = allyColor;

            healthBar = allyInfo.GetComponentInChildren<Slider>();
            healthBar.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = allyColor;

            alliesIndex++;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (!entity.IsOwner)
        {
            return;
        }

        if (Input.GetButtonDown("Dash") && Time.time > nextDashTime &&
            fighter.CheckAndUseEnergy(dashCostEnergy))
            StartCoroutine(Dash());

        base.Update();
    }

    public override void SimulateOwner()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = cam.right * x + cam.forward * z;
        dir.Normalize();
        dir *= moveSpeedMultiplier * moveSpeed;
        dir.y = rb.velocity.y;
        rb.velocity = dir;
    }

    IEnumerator Dash()
    {
        moveSpeedMultiplier = dashMultiplier;
        nextDashTime = Time.time + dashDurationSeconds + dashCooldownSeconds;
        yield return new WaitForSeconds(dashDurationSeconds);

        moveSpeedMultiplier = 1f;
    }
}
