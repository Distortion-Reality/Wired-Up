using System.Collections;
using System.Collections.Generic;

public static class StatisticManager
{
    public enum StatisticId
    {
        HP,
        Int,
        Arm,
        Prc,
        Lng,
        Spd
    }

    public static List<Statistic> Stats = new List<Statistic>()
    {
        new Statistic(StatisticId.HP, "HP",
            ""),
        new Statistic(StatisticId.Int, "Intensity",
            ""),
        new Statistic(StatisticId.Arm, "Armor",
            ""),
        new Statistic(StatisticId.Prc, "Perception",
            ""),
        new Statistic(StatisticId.Lng, "Length",
            ""),
        new Statistic(StatisticId.Spd, "Speed",
            "")
    };
}
