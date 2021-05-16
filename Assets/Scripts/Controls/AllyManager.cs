using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyManager : FighterManager
{
    // Start is called before the first frame update
    protected override void Start()
    {
        TargetPlayerAbilityManager targetPlayerAbilityManager = GetComponent<TargetPlayerAbilityManager>();
        Wire wire = transform.Find("WireParent").transform.Find("Wire").GetComponent<Wire>();
        fighter = new Player(null, null, null, targetPlayerAbilityManager, wire);
    }

    // Update is called once per frame
    protected override void Update()
    {
        
    }
}
