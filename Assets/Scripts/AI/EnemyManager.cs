using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    protected Player[] players = new Player[3];
    protected Enemy[] enemies = new Enemy[3];

    // Start is called before the first frame update
    void Start()
    {
        players = FindObjectsOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(FindObjectsOfType<Player>().Length);
        if (FindObjectsOfType<Player>().Length > 0)
        {
            players = FindObjectsOfType<Player>();
        }
        enemies = FindObjectsOfType<Enemy>();
        for (int i = 0; i < players.Length; i++)
        {
            if (enemies[i].Target == null)
            {
                enemies[i].Target = players[i];
            }
        }
    }
}
