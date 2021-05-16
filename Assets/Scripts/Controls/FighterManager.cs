using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class FighterManager : MonoBehaviour
{
    protected Fighter fighter;

    public Fighter Fighter { get => fighter; }

    // Start is called before the first frame update
    protected abstract void Start();

    // Update is called once per frame
    protected abstract void Update();
}
