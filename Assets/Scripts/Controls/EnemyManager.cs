using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : FighterManager
{
    // Start is called before the first frame update
    protected override void Start()
    {
        TargetEnemyAbilityManager targetEnemyAbilityManager = GetComponent<TargetEnemyAbilityManager>();
        fighter = new Enemy(null, null, null, targetEnemyAbilityManager);
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}
