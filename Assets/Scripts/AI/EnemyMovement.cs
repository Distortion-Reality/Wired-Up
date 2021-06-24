using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{

    Enemy enemy;
    NavMeshAgent agent;
    Vector3 destination;
    float distance;

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
            agent.destination = Vector3.ProjectOnPlane(enemy.Target.transform.position, Vector3.up);
            //distance = Vector3.Distance(this.transform.position, );
            agent.stoppingDistance = enemy.Target.AbilityRange + 3;
            if (distance > 3)
            {
                distance = 3;
                //transform.position = (transform.position - )
            }
        }
    }
}
