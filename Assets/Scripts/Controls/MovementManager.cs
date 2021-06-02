using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementManager : MonoBehaviour
{
    Rigidbody rb;
    Transform cam;
    readonly float moveSpeed = 4f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
    }

    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = cam.right * x + cam.forward * z;
        dir.Normalize();
        dir *= moveSpeed;
        dir.y = rb.velocity.y;
        rb.velocity = dir;
    }
}
