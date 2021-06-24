using System.Collections;
using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(Player))]
public class PlayerManager : FighterManager<IPlayerState>
{
    Transform cam;

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
    }

    protected override void OnUpdate()
    {
        if (!entity.IsOwner)
            return;

        if (Input.GetButtonDown("Dash") && Time.time > nextDashTime &&
            fighter.FighterStatus == Fighter.Status.FREE &&
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
