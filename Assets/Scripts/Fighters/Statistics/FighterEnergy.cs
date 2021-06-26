using UnityEngine;

public class FighterEnergy : FighterRangedStatistic
{
    new float currentValue;

    public FighterEnergy(int baseValue) : base(baseValue)
    {
        currentValue = baseValue;
    }

    public new float CurrentValue { get => currentValue; }

    public new float PercentageValue => currentValue / baseValue;

    public override int ApplyChange(int change)
    {
        ApplyChange(change);
        return base.currentValue;
    }

    public float ApplyChange(float change)
    {
        currentValue = Mathf.Clamp(currentValue + change, 0f, baseValue);
        base.currentValue = Mathf.FloorToInt(currentValue);
        return currentValue;
    }
}
