using System.Collections;
using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(Player))]
public class PlayerManager : FighterManager<IPlayerState>
{
    Rigidbody rb;
    Transform cam;

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
