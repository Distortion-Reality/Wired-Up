using System;
using UnityEngine;
using Photon.Bolt;

public class EntitySpawner : MonoBehaviour {

    public GameObject prefab;

    void Start()
    {
        if (BoltNetwork.IsServer)
        {
            FighterInfo info = new FighterInfo
            {
                guid = Guid.NewGuid()
            };
            InstantiatePrefab(info);
        }

        Destroy(gameObject);
    }

    protected virtual BoltEntity InstantiatePrefab(FighterInfo info)
    {
        return BoltNetwork.Instantiate(prefab, info, transform.position, transform.rotation);
    }
}
