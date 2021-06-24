using UnityEngine;

public class FighterEnergy : FighterRangedStatistic
{
    new float currentValue;

    public FighterEnergy(int baseValue) : base(baseValue)
    {
        currentValue = baseValue;
    }

    public new float CurrentValue { get => currentValue; }

    public override void ApplyChange(int change)
    {
        ApplyChange(change);
    }

    public void ApplyChange(float change)
    {
        currentValue = Mathf.Clamp(currentValue + change, 0f, baseValue);
        base.currentValue = Mathf.FloorToInt(currentValue);
    }
}
