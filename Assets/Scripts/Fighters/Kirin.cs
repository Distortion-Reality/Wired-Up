using UnityEngine;

public class Kirin : MonoBehaviour
{
    public Transform foreArmGunFire;

    private Transform root;
    private Vector3 defaultRootPosition;

    // Start is called before the first frame update
    void Start()
    {
        root = transform.Find("Root");
        defaultRootPosition = root.localPosition;
    }

    void LateUpdate()
    {
        root.localPosition = defaultRootPosition;
    }
}
