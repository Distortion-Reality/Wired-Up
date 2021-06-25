using UnityEngine;

public class FighterRangedStatistic : FighterStatistic
{
    public FighterRangedStatistic(int baseValue) : base(baseValue) { }

    public override int ApplyChange(int change)
    {
        currentValue = Mathf.Clamp(currentValue + change, 0, baseValue);
        return currentValue;
    }
}
