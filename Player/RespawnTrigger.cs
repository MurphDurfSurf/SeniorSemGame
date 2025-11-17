using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RespawnTrigger : MonoBehaviour
{
    public Transform respawnPoint;

    void OnTriggerEnter(Collider other)
    {
        // Works even if the collider is on a child of the player
        var pr = other.GetComponentInParent<PlayerRespawn>();
        if (pr != null && respawnPoint != null)
        {
            pr.RespawnAt(respawnPoint);
        }
    }

}
