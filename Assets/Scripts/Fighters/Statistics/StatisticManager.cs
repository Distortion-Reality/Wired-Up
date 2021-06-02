using System.Collections.Generic;

public static class StatisticManager
{
    public enum StatisticId
    {
        HP,
        Arm,
        Spd,
        Lng,
        Eng,
        Int
    }

    public static List<Statistic> stats = new List<Statistic>()
    {
        new Statistic(StatisticId.HP, "Hit Points", "HP",
            "The remaining life value of the character and how many damages he can sustain."),
        new Statistic(StatisticId.Arm, "Armor", "Arm",
            "The capacity of the character to absorb and reduce attacks’ damages."),
        new Statistic(StatisticId.Spd, "Speed", "Spd",
            "How fast the character can move and how fast he regains Energy."),
        new Statistic(StatisticId.Lng, "Length", "Lng",
            "How far the character can extend his wire."),
        new Statistic(StatisticId.Eng, "Energy", "Eng",
            "How many actions the character can do at a specific moment."),
        new Statistic(StatisticId.Int, "Intensity", "Int",
            "How effective character’s attacks are.")
    };
}
