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
        
        if (entity.IsControllerOrOwner)
            fighter.OwnerUpdate();
        
        fighter.EntityUpdate();
    }

    // SimulateOwner is a FixedUpdate run only if entity.IsOwner
    void FixedUpdate()
    {
        if (!entity.IsAttached || gameOver.IsGameOver)
            return;
        
        if (entity.IsControllerOrOwner)
            fighter.OwnerFixedUpdate();
    }

    // SimulateOwner is a FixedUpdate run only if entity.HasControl
    public override void SimulateController()
    {
        fighter.ControllerFixedUpdate();
    }

    public override void ExecuteCommand(Command command, bool resetState)
    {
        fighter.ExecuteCommand(command, resetState);
    }
}
