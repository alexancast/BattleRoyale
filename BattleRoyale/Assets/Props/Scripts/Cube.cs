using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour, IShatterable
{
    public void Shatter(float shatterForce)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;

        for (int i = 0; i < transform.childCount; i++)
        {
            rb = transform.GetChild(i).GetComponent<Rigidbody>();
            rb.GetComponent<Collider>().enabled = true;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(Random.Range(-shatterForce, shatterForce), Random.Range(-shatterForce, shatterForce), Random.Range(-shatterForce, shatterForce));

        }
    }
}
