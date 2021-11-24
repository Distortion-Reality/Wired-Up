using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(BoltEntity))]
public class FighterEntity : EntityBehaviour<IFighterState>
{
    Fighter fighter;
    GameOver gameOver;

    public override void Attached()
    {
        gameOver = GameOver.FindObjectOfType<GameOver>();

        fighter = GetComponent<Fighter>();
        fighter.EntityStart();
    }

    public override void Detached()
    {
        fighter.EntityDestroyed();
    }

    // Update is called once per frame
    void Update()
    {
        if (!entity.IsAttached || gameOver.IsGameOver)
        {
            fighter.EntityDestroyed();
            return;
        }
        
        if (entity.IsOwner)
            fighter.OwnerUpdate();
        
        fighter.EntityUpdate();
    }

    // SimulateOwner is a FixedUpdate run only if entity.IsOwner
    public override void SimulateOwner()
    {
        if (!entity.IsAttached || gameOver.IsGameOver)
            return;
        
        fighter.OwnerFixedUpdate();
    }
}
