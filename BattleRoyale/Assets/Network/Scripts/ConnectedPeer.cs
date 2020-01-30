using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ConnectedPeer : MonoBehaviour
{
    private int peerIndex = -1;
    [SerializeField] private BodyParts[] bodyParts;
    [SerializeField] private Transform spine;
    [SerializeField] private Animator animator;
    [SerializeField] private bool debug;

    private Rigidbody[] rigidbodies;
    private bool alive = true;

    [Serializable]
    private struct BodyParts
    {
        public GameObject bodyPart;
        [Range(0,1)]public float damagePercentage;
    }

    public void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public bool GetAlive() { return alive; }

    public void SetAnimation(float speed, float direction)
    {
        animator.speed = 2;
        animator.SetFloat("Speed", speed);
        animator.SetFloat("Direction", direction);
    }

    public void ActivateRagdoll()
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }
    }



    public void Die(Vector3 shotFrom, float pushbackForce)
    {
        alive = false;
        animator.enabled = false;
        ActivateRagdoll();
        enabled = false;

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.AddForce((transform.position - shotFrom).normalized * pushbackForce);
        }

        if (!debug)
            StartCoroutine(Respawn());
    }

    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5);
        ConnectedPeer clone = Instantiate(NetCient.instance.GetPeerPrefab(), transform.position, transform.rotation).GetComponent<ConnectedPeer>();
        clone.peerIndex = peerIndex;
        NetCient.instance.SetPeerClone(clone.gameObject, peerIndex);
        Debug.Log(gameObject.name + "Destroyed after respawn coroutine was called in connectedPeer");
        Destroy(gameObject);
    }


    public float GetHitDamageFallOff(GameObject hitObject) {

        for (int i = 0; i < bodyParts.Length; i++)
        {
            if (bodyParts[i].bodyPart == hitObject)
            {
                return bodyParts[i].damagePercentage;
            }
        }

        return 0;
    }

    public int GetPeerIndex() { return peerIndex; }
    public void SetPeerIndex(int peerIndex) { this.peerIndex = peerIndex; }

    public void RotateSpine(Vector3 spineRotation) {
        spine.eulerAngles = spineRotation;
    }
}
