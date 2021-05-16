using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Wire : MonoBehaviour
{
    Fighter user;
    bool isExtending = false, isConnected = false;
    Ability ability = null;

    // Start is called before the first frame update
    void Start()
    {
        user = transform.parent.transform.parent.GetComponent<FighterManager>().Fighter;
    }

    public void Connect(Ability ability)
    {
        this.ability = ability;
        StartCoroutine(Extend());
    }

    IEnumerator Extend()
    {
        isExtending = true;

        while(isExtending)
        {
            transform.parent.transform.localScale += new Vector3(0, 0, 0.2f);
            yield return new WaitForSeconds(0.01f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        Fighter collisionFighter = collision.gameObject.GetComponent<FighterManager>().Fighter;
        string collisionTag = collision.gameObject.tag;

        if(collisionFighter != user && collisionTag != "Terrain")
            if(!isConnected && (collisionTag == "Enemy" || collisionTag == "Ally"))
            {
                isExtending = false;
                isConnected = true;
                user.Target = collisionFighter;
                user.UseAbility(ability);
            }
            else
                user.InterruptAbility();
    }
}
