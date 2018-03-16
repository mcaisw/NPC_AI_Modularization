using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class EnemyEye : MonoBehaviour {
    EnemyFunc enemy;

    private void Start()
    {
        enemy = this.transform.parent.GetChild(0).GetComponent<EnemyFunc>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="Player")
        {
            enemy.SeePlayer();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            enemy.PlayerLeave();
        }
    }
}
