public abstract class FighterStatistic
{
    protected int baseValue;
    protected int currentValue;

    public FighterStatistic(int baseValue)
    {
        this.baseValue = baseValue;
        currentValue = baseValue;
    }

    public int BaseValue { get => baseValue; set => baseValue = value; }
    public int CurrentValue { get => currentValue; set => currentValue = value; }

    public float PercentageValue => ((float) currentValue) / baseValue;

    public abstract int ApplyChange(int change);
}
