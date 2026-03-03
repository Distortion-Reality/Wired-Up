using Fusion;
using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    public GameObject prefab;

    void Start()
    {
        if (NetworkManager.runner.IsServer)
            InstantiateNetworkObject();

        Destroy(gameObject);
    }

    protected virtual NetworkObject InstantiateNetworkObject()
    {
        NetworkObject entity = NetworkRunner.Instantiate(prefab, BuildToken(), transform.position, transform.rotation);
        entity.transform.SetParent(transform.parent);
        return entity;
    }
}
