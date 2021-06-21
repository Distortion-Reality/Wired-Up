public class FighterStatistic
{
    readonly int baseValue;
    int currentValue;

    public FighterStatistic() : this(0) { }

    public FighterStatistic(int baseValue)
    {
        this.baseValue = baseValue;
        currentValue = baseValue;
    }

    public int BaseValue { get => baseValue;}

    public int CurrentValue { get => currentValue; set => currentValue = value; }

    public float PercentageValue { get => ((float) currentValue) / baseValue;}

    public void ResetValue()
    {
        currentValue = baseValue;
    }
}
