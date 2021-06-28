using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Bolt;

[RequireComponent(typeof(BoltEntity))]
public class FighterEntity : EntityBehaviour<IFighterState>
{
    Fighter fighter;

    public override void Attached()
    {
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BoltLauncher.Shutdown();
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }

        if (!entity.IsAttached)
            return;
        
        if (entity.IsOwner)
            fighter.OwnerUpdate();
        
        fighter.EntityUpdate();
    }

    // SimulateOwner is a FixedUpdate run only if entity.IsOwner
    public override void SimulateOwner()
    {
        fighter.OwnerFixedUpdate();
    }
}
