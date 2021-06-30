using UnityEngine;

public class FighterBuffableStatistic : FighterStatistic
{
    readonly float stageBuff = 0.25f;
    readonly int maxStage = 3;
    int stage = 0;

    public FighterBuffableStatistic(int baseValue) : base(baseValue) { }

    public override int ApplyChange(int change)
    {
        stage += change;
        currentValue = baseValue + Mathf.RoundToInt(Mathf.Clamp(stage, - maxStage, maxStage) * stageBuff * baseValue);
        return currentValue;
    }
}
