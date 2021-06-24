using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(Enemy))]
public class EnemyManager : FighterManager<IEnemyState>
{
    protected override Quaternion DefaultRotation => transform.rotation;

    protected override void OnAttached()
    {
        
    }

    protected override void OnUpdate()
    {

    }

    protected override void OnSimulateOwner()
    {
        
    }
}
