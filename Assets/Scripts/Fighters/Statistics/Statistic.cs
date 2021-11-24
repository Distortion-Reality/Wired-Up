public class Statistic
{
    readonly StatisticManager.StatisticId id;
    readonly string name, abbr, description;

    public Statistic(StatisticManager.StatisticId id, string name, string abbr, string description)
    {
        this.id = id;
        this.name = name;
        this.abbr = abbr;
        this.description = description;
    }

    public StatisticManager.StatisticId Id => id;
    public string Name => name;
    public string Abbr => abbr;
    public string Description => description;
}
