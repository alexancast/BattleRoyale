using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    [SerializeField] private float runSpeed = 10;
    [SerializeField] private float aimSpeed = 2;
    [SerializeField] private float mouseSensitivity = 40;
    [SerializeField] private float jumpForce = 3;
    [SerializeField] private float gravityMultiplier = 5;
    [SerializeField] private Transform spine;
    [SerializeField] private CharacterController characterController;

    private Rigidbody playerRigidbody;
    private Rigidbody[] rigidbodies;
    private Player player;
    private float velocity;
    private bool grounded;
    private Vector3 direction = new Vector3(0,0,0);
    private Vector3 inputDir;

    public void SetGrounded(bool toggle) { grounded = toggle; }
    public void Start()
    {
        inputDir = new Vector3(0,0,0);
        player = GetComponent<Player>();
        playerRigidbody = GetComponent<Rigidbody>();
        rigidbodies = GetComponentsInChildren<Rigidbody>();

        DeactivateRagdoll();
    }

    public Vector3 GetDirection() { return inputDir; }

    public void ActivateRagdoll()
    {
        characterController.enabled = false;

        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb != playerRigidbody)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
            }
        }
    }

    public void DeactivateRagdoll()
    {
        characterController.enabled = true;

        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb != playerRigidbody)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }
    }


    public void Update()
    {
        if (!player.GetAlive())
            return;

        float activeSpeed;

        if (Input.GetMouseButton(1))
        {
            activeSpeed = aimSpeed;

        }else if(Input.GetKey(KeyCode.LeftShift))
        {
            activeSpeed = runSpeed;
        }
        else
        {
            activeSpeed = speed;
        }


        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputDir = new Vector3(horizontal, 0, vertical);

        float hMouse = Input.GetAxis("Mouse X");
        float vMouse = Input.GetAxis("Mouse Y");

        direction = transform.TransformDirection(horizontal, 0, vertical).normalized * activeSpeed + Vector3.up * velocity;

        characterController.Move(direction * Time.deltaTime);

        transform.Rotate(new Vector3(0, hMouse, 0) * mouseSensitivity * Time.deltaTime);
        spine.Rotate(new Vector3(vMouse, 0, 0) * -mouseSensitivity * Time.deltaTime);


    }

    public void Jump() {
        velocity = jumpForce * 5;
    }

    public void FixedUpdate()
    {

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Jump();   
        } else if(grounded && velocity < 0)
        {
            velocity = 0;
        }
        else
        {
            velocity += Physics.gravity.y * Time.deltaTime * gravityMultiplier;
        }

    }
}
