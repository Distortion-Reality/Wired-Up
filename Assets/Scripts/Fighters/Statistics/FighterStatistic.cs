using System.Collections;
using System.Collections.Generic;

public class FighterStatistic
{
    int baseValue, currentValue;

    public FighterStatistic() : this(0) { }

    public FighterStatistic(int baseValue)
    {
        this.baseValue = baseValue;
        this.currentValue = this.baseValue;
    }

    public int CurrentValue { get => currentValue; set => currentValue = value; }
}
