using System;
using UnityEngine;
using Photon.Bolt;

public class EntitySpawner : MonoBehaviour {
    public GameObject prefab;

    void Start()
    {
        if (BoltNetwork.IsServer)
            BoltNetwork.Instantiate(prefab, BuildToken(), transform.position, transform.rotation);

        Destroy(gameObject);
    }

    protected virtual FighterInfo CreateToken()
    {
        return new FighterInfo();
    }

    protected virtual FighterInfo BuildToken()
    {
        FighterInfo token = CreateToken();
        token.guid = Guid.NewGuid();
        return token; 
    }
}
