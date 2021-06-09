using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Player))]
public class MovementManager : MonoBehaviour
{
    Player player;
    Rigidbody rb;
    Transform cam;

    readonly float moveSpeed = 4f;
    float moveSpeedMultiplier = 1f;

    readonly int dashCostEnergy = 5;
    readonly float dashMultiplier = 2f,
        dashDurationSeconds = 0.5f,
        dashCooldownSeconds = 5f;
    float nextDashTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Dash") && Time.time > nextDashTime &&
            player.CheckAndUseEnergy(dashCostEnergy))
            StartCoroutine(Dash());
    }

    void FixedUpdate()
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
