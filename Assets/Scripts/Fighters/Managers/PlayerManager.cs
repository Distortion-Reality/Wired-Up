using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

[RequireComponent(typeof(Player))]
public class PlayerManager : FighterManager<IPlayerState>
{
    static int alliesIndex = 0;
    
    Transform cam;
    Canvas gui;
    public GameObject allyInfoPrefab;

    float moveSpeedMultiplier = 1f;

    readonly int dashEnergy = 5;
    readonly float dashMultiplier = 2f,
        dashDuration = 0.25f,
        dashCooldown = 5f;
    float nextDashTime = 0f;

    protected override Quaternion DefaultRotation =>
        new Quaternion(transform.rotation.x, cam.rotation.y, transform.rotation.z, cam.rotation.w);

    protected override void OnAttached()
    {
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

            allyInfo.transform.Find("AllyPortrait").GetComponent<Image>().color = allyColor;

            healthBar = allyInfo.GetComponentInChildren<Slider>();
            healthBar.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color = allyColor;

            alliesIndex++;
        }
    }

    protected override void OnUpdate()
    {
        if (Input.GetButtonDown("Dash") && Time.time > nextDashTime &&
            fighter.FighterStatus == Fighter.Status.Free &&
            fighter.CheckAndUseEnergy(dashEnergy))
            StartCoroutine(Dash());
    }

    protected override void OnSimulateOwner()
    {
        UpdateMovement();
    }

    void UpdateMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = cam.right * x + cam.forward * z;
        dir.Normalize();
        dir *= moveSpeedMultiplier * MoveSpeed;
        dir.y = rb.velocity.y;
        rb.velocity = dir;
    }

    IEnumerator Dash()
    {
        moveSpeedMultiplier = dashMultiplier;
        nextDashTime = Time.time + dashDuration + dashCooldown;
        yield return new WaitForSeconds(dashDuration);

        moveSpeedMultiplier = 1f;
    }
}
