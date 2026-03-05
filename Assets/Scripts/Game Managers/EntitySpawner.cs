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

    protected virtual FighterToken CreateToken()
    {
        return new FighterToken();
    }

    protected virtual FighterToken BuildToken()
    {
        FighterToken token = CreateToken();
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
