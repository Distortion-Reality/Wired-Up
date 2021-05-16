using System.Collections;
using System.Collections.Generic;

public class Statistic
{
    StatisticManager.StatisticId id;
    string name, description;

    public Statistic(StatisticManager.StatisticId id, string name, string description)
    {
        this.id = id;
        this.name = name;
        this.description = description;
    }

    public string Name { get => name; }
    public string Description { get => description; }
}
