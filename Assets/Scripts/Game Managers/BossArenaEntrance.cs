using System.Collections.Generic;
using UnityEngine;

public class BossArenaEntrance : MonoBehaviour
{
    readonly List<Player> players = new List<Player>();

    public static BossArenaEntrance Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (players.Count == 3)
        {
            foreach (Player player in players)
                if (player.transform.position.z < transform.position.z)
                    return;
            GetComponent<Collider>().enabled = true;
            enabled = false;
        }
    }

    public void RegisterPlayer(Player player)
    {
        players.Add(player);
    }
}
