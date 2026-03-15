using System.Collections.Generic;
using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    readonly List<Player> players = new List<Player>();
    readonly List<Enemy> enemies = new List<Enemy>();

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < Mathf.Min(players.Count, enemies.Count); i++)
            if (!enemies[i].Target)
                enemies[i].Target = players[i];
    }

    public void RegisterPlayer(Player player)
    {
        players.Add(player);
    }

    public void UnregisterPlayer(Player player)
    {
        players.Remove(player);
    }

    public void AddEnemy(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    public void RemoveEnemy(Enemy enemy)
    {
        if (enemies.Contains(enemy))
            enemies.Remove(enemy);
    }
}
