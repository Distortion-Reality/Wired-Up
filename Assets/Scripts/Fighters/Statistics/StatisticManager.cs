using System.Collections.Generic;

public static class StatisticManager
{
    public enum StatisticId
    {
        HP,
        Arm,
        Lng,
        Int,
        Spd,
        Nrg
    }

    public static List<Statistic> stats = new List<Statistic>()
    {
        new Statistic(StatisticId.HP, "Hit Points", StatisticId.HP.ToString(),
            "The remaining life value of the character and how many damages he can sustain."),
        new Statistic(StatisticId.Arm, "Armor", StatisticId.Arm.ToString(),
            "The capacity of the character to absorb and reduce attacks� damages."),
        new Statistic(StatisticId.Lng, "Length", StatisticId.Lng.ToString(),
            "How far the character can extend his wire."),
        new Statistic(StatisticId.Int, "Intensity", StatisticId.Int.ToString(),
            "How effective character�s attacks are."),
        new Statistic(StatisticId.Nrg, "Energy", StatisticId.Nrg.ToString(),
            "How many actions the character can do at a specific moment."),
        new Statistic(StatisticId.Spd, "Speed", StatisticId.Spd.ToString(),
            "How fast the character can move and how fast he regains Energy.")
    };
}
