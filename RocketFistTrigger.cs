using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketFistTrigger : MonoBehaviour
{
    
    public CombatAttributes attri;
    public bool canDoDamage = true;
    public bool hasHitEnemy;

    public void OnEnable()
    {
        canDoDamage = true;
        hasHitEnemy = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && canDoDamage)
        {
            other.GetComponent<EnemyGetDamaged>().GetDamaged(transform, attri.strenght, attri.dmg, attri.type, attri.statusDuration);
            hasHitEnemy = true;
        }
    }
}
