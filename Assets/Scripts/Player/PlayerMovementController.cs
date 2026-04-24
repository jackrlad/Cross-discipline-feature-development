using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    public Transform PlayerModel;
    public Transform Camera;

    private Rigidbody rb;
    public float SPEED = 10;
    public float JUMPFORCE = 10;
    
    private InputReader ir;
    public float yaw = 0.0f;
    private bool OnGround = false;
    private bool CanJump = true;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ir = GetComponent<InputReader>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        Movement();
    }

    public void setYaw(float newYaw)
    {
        yaw = newYaw;
    }

    void Movement()
    {
        Vector2 rotatedMovement = RotateVector2(ir.Move, 45);
        Vector3 movement = new Vector3(-rotatedMovement.x, 0, -rotatedMovement.y);

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

        
        if(ir != null && ir.Jump)
        {
            if (CanJump && OnGround)
            {
                rb.AddForce(PlayerModel.up * JUMPFORCE, ForceMode.Impulse);
                CanJump = false;
            }
        }
        else CanJump = true;


        Ray groundRay = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        Debug.DrawRay(groundRay.origin, groundRay.direction * 1.8f, Color.green);
        if(Physics.Raycast(groundRay, out hit, 2f))
        {
            OnGround = true;
            
            transform.rotation = Quaternion.Euler
            (
                hit.normal.z * Mathf.Rad2Deg, 
                transform.rotation.eulerAngles.y, 
                -1 * hit.normal.x * Mathf.Rad2Deg
            );
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

        Vector3 velocity = Vector3.zero;

        if(movement.magnitude > 0)
            velocity = PlayerModel.forward;
        velocity = velocity * SPEED;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }

    Vector2 RotateVector2(Vector2 vector, float degrees)
    {
        float newX = Mathf.Cos(degrees) * vector.x - Mathf.Sin(degrees) * vector.y;
        float newY = Mathf.Sin(degrees) * vector.x + Mathf.Cos(degrees) * vector.y;
        
        return new Vector2(newX, newY);
    }
}