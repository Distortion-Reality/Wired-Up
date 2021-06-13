using UnityEngine;
using Photon.Bolt;

public class EntitySpawner : MonoBehaviour {

    public GameObject prefab;

    void Start()
    {
        if (BoltNetwork.IsServer)
            BoltNetwork.Instantiate(prefab, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
