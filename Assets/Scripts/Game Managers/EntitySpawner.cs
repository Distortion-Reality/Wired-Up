using System;
using UnityEngine;
using Photon.Bolt;

public class EntitySpawner : MonoBehaviour {

    public GameObject prefab;

    void Start()
    {
        if (BoltNetwork.IsServer)
        {
            FighterInfo info = new FighterInfo();
            info.guid = Guid.NewGuid();
            BoltNetwork.Instantiate(prefab, info, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}
