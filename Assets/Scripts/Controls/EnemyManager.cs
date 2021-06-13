using System.Collections;
using UnityEngine;
using Photon.Bolt;

[RequireComponent(typeof(Enemy))]
public class EnemyManager : FighterManager<IEnemyState>
{
    public override void Attached()
    {
        base.Attached();
    }

    protected override void Update()
    {
        base.Update();
    }
}
