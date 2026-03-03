using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class FighterEntity : NetworkBehaviour
{
    Fighter fighter;
    GameOver gameOver;

    public override void Spawned()
    {
        gameOver = FindObjectOfType<GameOver>();

        fighter = GetComponent<Fighter>();
        fighter.EntityStart();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        fighter.EntityDestroyed();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Object.HasStateAuthority || gameOver.IsGameOver)
        {
            fighter.EntityDestroyed();
            return;
        }
        
        if (Object.HasStateAuthority)
            fighter.OwnerUpdate();
        
        fighter.EntityUpdate();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || gameOver.IsGameOver)
            return;
        
        fighter.OwnerFixedUpdate();
    }
}
