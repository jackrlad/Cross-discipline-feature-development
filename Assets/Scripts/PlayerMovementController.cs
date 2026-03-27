using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    public Transform PlayerModel;
    public Transform Camera;

    private Rigidbody rb;
    public float SPEED = 2000;
    public float JUMPFORCE = 10;
    
    private Vector3 forward = new Vector3(1, 0, -1);
    private Vector3 right = new Vector3(-1, 0, -1);
    private float SprintConst = 1.8f;
    public float yaw = 0.0f;
    private bool OnGround = false;
    private bool CanJump = true;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Movement();
    }

    public void setYaw(float newYaw)
    {
        yaw = newYaw;
    }

    void Movement()
    {
        Vector3 movement = Vector3.zero;

        if(Input.GetKey(KeyCode.W))
        {
            movement += forward;
        }
        if(Input.GetKey(KeyCode.S))
        {
            movement -= forward;
        }
        if(Input.GetKey(KeyCode.D))
        {
            movement += right;
        }
        if(Input.GetKey(KeyCode.A))
        {
            movement -= right;
        }

        movement = movement.normalized;

        if(movement != Vector3.zero)
        {
            yaw = Mathf.Atan2(movement.x, movement.z);
            yaw = yaw * (180f / Mathf.PI);

            PlayerModel.localRotation = Quaternion.Lerp
            (
                Quaternion.Euler(0.0f, PlayerModel.localRotation.eulerAngles.y, 0.0f), 
                Quaternion.Euler(0.0f, yaw, 0.0f), 
                0.1f
            );
        }

        if (Input.GetKeyDown(KeyCode.Space) && CanJump && OnGround)
        {
            rb.AddForce(PlayerModel.up * JUMPFORCE, ForceMode.Impulse);
            CanJump = false;
        }



        Ray groundRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(groundRay.origin, groundRay.direction, Color.green);
        if(Physics.Raycast(groundRay, out hit, 2f))
        {
            OnGround = true;
            transform.rotation = Quaternion.Euler
            (
                hit.normal.x, 
                transform.rotation.eulerAngles.y, 
                hit.normal.z
            );
            Debug.Log(hit.collider.gameObject.transform.rotation.eulerAngles.x);
            Debug.Log(hit.collider.gameObject.transform.rotation.eulerAngles.z);
            Debug.Log("-----");
        }
        else
        {
            OnGround = false;
            transform.rotation = Quaternion.Euler
            (
                0, 
                transform.rotation.eulerAngles.y, 
                0
            );
        }

        if(!OnGround) CanJump = true;

        Vector3 velocity = Vector3.zero;

        if(movement.magnitude > 0)
            velocity = PlayerModel.forward;



        if(Input.GetKey(KeyCode.LeftShift))
        {
            velocity = velocity * SprintConst;
        }
        velocity = velocity * Time.deltaTime * SPEED;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }
}