using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Walkable"))
            player.SetGrounded(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Walkable"))
            player.SetGrounded(false);
    }
}
