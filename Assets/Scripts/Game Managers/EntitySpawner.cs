using System;
using UnityEngine;
using Photon.Bolt;

public class EntitySpawner : MonoBehaviour
{
    public GameObject prefab;

    void Start()
    {
        if (BoltNetwork.IsServer)
            InstantiateBoltEntity();

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

    protected virtual BoltEntity InstantiateBoltEntity()
    {
        BoltEntity entity = BoltNetwork.Instantiate(prefab, BuildToken(), transform.position, transform.rotation);
        entity.transform.SetParent(transform.parent);
        return entity;
    }
}
