using UnityEngine;
using UnityEngine.UI;
using Photon.Bolt;

public abstract class FighterManager<T> : EntityBehaviour<T> where T : IFighterState
{
    protected Fighter fighter;

    protected Slider healthBar;

    public override void Attached()
    {
        fighter = GetComponent<Fighter>();
        fighter.Init();

        state.SetTransforms(state.transform, transform);
        state.hp = fighter.Stats[StatisticManager.StatisticId.HP].CurrentValue;
        state.AddCallback("hp", HpChanged);
    }

    void HpChanged()
    {
        fighter.Stats[StatisticManager.StatisticId.HP].CurrentValue = state.hp;
        if (healthBar != null)
        {
            healthBar.value = fighter.Stats[StatisticManager.StatisticId.HP].PercentageValue;
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        fighter.UpdateFrame();
    }
}
