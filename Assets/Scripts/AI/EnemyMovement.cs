using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{

    Enemy enemy;
    NavMeshAgent agent;
    Vector3 destination;
    Enemy[] enemies;

    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<Enemy>();
        agent = GetComponent<NavMeshAgent>();
        destination = agent.destination;
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.Target)
        {
            agent.destination = enemy.Target.BasePosition;
            agent.stoppingDistance = enemy.Target.AbilityRange + 3;
            /*
            destination = Vector3.ProjectOnPlane(enemy.Target.transform.position, Vector3.up);
                //Debug.Log(enemy.Target.transform.position);
                agent.destination = destination;
                //Debug.Log(agent.destination);
                agent.stoppingDistance = enemy.Target.AbilityRange + 3;
                //Debug.Log(agent.remainingDistance);
                enemies = FindObjectsOfType<Enemy>();
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i] != enemy)
                    {
                        Vector3 vector = Vector3.ProjectOnPlane(enemies[i].transform.position - transform.position, transform.up);
                        float distance = Vector3.Magnitude(vector);
                        if (distance < 10)
                        {
                            destination -= vector.normalized*2;
                            agent.destination = agent.destination + destination * (agent.stoppingDistance + 1);
                    }
                    }
                }

            
            Vector3 playerDirection = Vector3.ProjectOnPlane(enemy.Target.transform.position-transform.position, Vector3.up).normalized;
            Vector3 direction = playerDirection;
            enemies = FindObjectsOfType<Enemy>();
            for (int i = 0; i < enemies.Length; i++) {
                if (enemies[i] != enemy) {
                    Vector3 vector = Vector3.ProjectOnPlane(enemies[i].transform.position - transform.position, transform.up);
                    float distance = Vector3.Magnitude(vector);
                    if (distance < 10 && Vector3.Angle(playerDirection, vector) < 90)
                    {
                        direction -= vector.normalized;
                    }
                }
            }
            agent.destination = transform.position + direction * (agent.stoppingDistance+1);
            agent.stoppingDistance = enemy.Target.AbilityRange + 3;
        */
        }
    }
}
