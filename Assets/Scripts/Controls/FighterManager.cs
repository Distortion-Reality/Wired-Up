using System.Collections;
using UnityEngine;
using Photon.Bolt;

public abstract class FighterManager<T> : EntityBehaviour<T> where T : IFighterState
{
    protected Fighter fighter;

    public override void Attached()
    {
        fighter = GetComponent<Fighter>();
        fighter.Init();

        state.SetTransforms(state.transform, transform);
    }

    protected virtual void Update()
    {
        fighter.BaseTick();
    }
}
