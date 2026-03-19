using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class EntitySpawner : MonoBehaviour
{
    public NetworkPrefabRef prefab;

    public void StartEntitySpawner()
    {
        if (NetworkRunnerManager.Instance.Runner.IsServer)
            SpawnEntity();

        Destroy(gameObject);
    }

    protected virtual FighterToken CreateToken()
    {
        return new FighterToken();
    }

    protected virtual FighterToken BuildToken()
    {
        FighterToken token = CreateToken();
        return token;
    }

    protected virtual NetworkObject SpawnEntity()
    {
        NetworkObject entity = NetworkRunnerManager.Instance.Runner.Spawn(
            prefab,
            transform.position,
            transform.rotation,
            NetworkManager.Instance.Runner.LocalPlayer,
            (runner, obj) =>
            {
                FighterEntity fighterEntity = obj.GetComponent<FighterEntity>();
                fighterEntity.Token = BuildToken();
            });

        entity.transform.SetParent(transform.parent);
        return entity;
    }
}
