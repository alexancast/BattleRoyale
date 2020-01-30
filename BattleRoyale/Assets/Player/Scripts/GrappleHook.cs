using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrappleHook : Special
{

    [SerializeField] private float grappleDistance = 10;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float stoppingDistance = 0.5f;

    private LineRenderer line;
    private Vector3 grapplePoint;
    private bool grappling;

    public override void CacheComponents() {
        
        base.CacheComponents();
        line = GetComponent<LineRenderer>();
    }

    public override void RunAttack()
    {
        base.RunAttack();

        if (onCooldown)
            return;

        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, grappleDistance))
        {
            if (hit.collider.CompareTag("Walkable"))
            {
                line.enabled = true;
                grappling = true;
                line.startWidth = lineWidth;
                line.endWidth = lineWidth;
                grapplePoint = hit.point;
                player.GetComponent<CharacterController>().enabled = false;
                StartCoroutine(Cooldown());
            }
        }


    }

    public void Update()
    {
        if (grappling)
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, grapplePoint);

            player.transform.position = Vector3.Lerp(player.transform.position, grapplePoint, 0.1f);
        }

        if(Vector3.Distance(player.transform.position, grapplePoint) < stoppingDistance)
        {
            player.GetComponent<CharacterController>().enabled = true;
            grappling = false;
            line.enabled = false;
            player.GetComponent<PlayerMovement>().Jump();
        }
    }


   
}
