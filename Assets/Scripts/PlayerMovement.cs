using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject SelectionPrefab;
    public Transform PlayerModel;
    public Transform CameraArm;
    public Transform Camera;
    public Transform HealthDisplay;
    public BoxCollider KnifeCollider;

    private Transform SelectedObject1 = null;
    private Transform SelectedObject2 = null;
    private KnifeCount knifeCount = KnifeCount.Both;
    private Rigidbody rb;
    
    public float SPEED = 2000;
    public float JUMPFORCE = 100;

    private Vector3 SpawnPos = new Vector3(-5, 1, 0);
    private float MaxHealth = 100;
    private float Health = 100;
    
    private Vector3 forward = new Vector3(1, 0, -1);
    private Vector3 right = new Vector3(-1, 0, -1);
    private float cameraRot = 0;
    private float SprintConst = 1.8f;
    private float yaw = 0.0f;
    private bool OnGround = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Movement();
        SwapControls();
        RotCamera();

        CameraArm.position = transform.position;

        if(Health <= 0)
        {
            Health = MaxHealth;
            transform.position = SpawnPos;
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            TakeDamage(15);
        }

        HealthDisplay.localScale = new Vector3
        (
            Mathf.Lerp(HealthDisplay.localScale.x, Health / 100 * 3, 0.1f), 
            HealthDisplay.localScale.y, 
            HealthDisplay.localScale.z
        );
    }

    void TakeDamage(float damage)
    {
        Health -= damage;
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

        if (Input.GetKeyDown(KeyCode.Space) && OnGround)
        {
            rb.AddForce(PlayerModel.up * JUMPFORCE, ForceMode.Impulse);
        }



        Ray groundRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(groundRay.origin, groundRay.direction, Color.green);
        if(Physics.Raycast(groundRay, out hit, 2f))
        {
            OnGround = true;
            transform.rotation = Quaternion.Euler
            (
                hit.collider.gameObject.transform.rotation.eulerAngles.x, 
                transform.rotation.eulerAngles.y, 
                hit.collider.gameObject.transform.rotation.eulerAngles.z
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



        if(Input.GetKey(KeyCode.LeftShift))
        {
            velocity = velocity * SprintConst;
        }
        velocity = velocity * Time.deltaTime * SPEED;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }

    void RotCamera()
    {
        cameraRot = 0;

        if(Input.GetKey(KeyCode.Q))
        {
            cameraRot -= 90;
        }
        if(Input.GetKey(KeyCode.E))
        {
            cameraRot += 90;
        }

        CameraArm.rotation = Quaternion.Lerp
        (
            CameraArm.rotation, 
            Quaternion.Euler(CameraArm.eulerAngles.x, cameraRot, CameraArm.eulerAngles.z), 
            0.1f
        );
    }

    void SwapControls()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Collider[] hits = Physics.OverlapBox
            (
                KnifeCollider.transform.position,
                KnifeCollider.size / 2,
                KnifeCollider.transform.rotation,
                LayerMask.GetMask("Swappable")
            );

            if(hits.Length == 0)
            {
                Debug.Log("no collider found");
                return;
            }

            Collider closestCol = hits[0];
            float disFromOther = (transform.position - closestCol.transform.position).magnitude;

            foreach(Collider col in hits)
            {
                Debug.DrawRay(col.transform.position, Vector3.up*100, Color.red, 10);
                float disFromCol = (transform.position - col.transform.position).magnitude;

                if(disFromCol < disFromOther)
                {
                    closestCol = col;
                }
            }

            Ray KnifeRay = new Ray(transform.position, PlayerModel.forward);
            RaycastHit hit;
            Vector3 dir = (closestCol.transform.position - KnifeRay.origin).normalized;
            Debug.DrawRay(KnifeRay.origin, dir, Color.red);
            if(Physics.Raycast(KnifeRay.origin, dir, out hit))
            {
                if(hit.collider == closestCol) {
                    if(knifeCount == KnifeCount.Both)
                    {
                        SelectedObject1 = hit.collider.gameObject.transform;
                        SelectedObject2 = transform;
                        knifeCount = KnifeCount.One;

                        var prefab = Instantiate(SelectionPrefab);
                        prefab.transform.parent = SelectedObject1;
                        prefab.transform.localPosition = new Vector3(0, 1.5f, 0);
                    }
                    else if(knifeCount == KnifeCount.One && SelectedObject1 != hit.collider.gameObject.transform)
                    {
                        SelectedObject2 = hit.collider.gameObject.transform;
                        knifeCount = KnifeCount.Neither;

                        var prefab = Instantiate(SelectionPrefab);
                        prefab.transform.parent = SelectedObject2;
                        prefab.transform.localPosition = new Vector3(0, 1.5f, 0);
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if(knifeCount == KnifeCount.One)
            {
                Swap(SelectedObject1, SelectedObject1, transform, PlayerModel);
                SelectedObject1 = null;
                SelectedObject2 = null;
            }
            else if(knifeCount == KnifeCount.Neither)
            {
                Swap(SelectedObject1, SelectedObject1, SelectedObject2, SelectedObject2);
                SelectedObject1 = null;
                SelectedObject2 = null;
            }
        }
    }

    void Swap(Transform Obj1, Transform Obj1Model, Transform Obj2, Transform Obj2Model)
    {
        if(Obj1 && Obj2)
        {
            Vector3 tempPos = Obj1.position;
            Quaternion tempRot = Obj1Model.rotation;
            Obj1.position = Obj2.position;
            Obj1Model.rotation = Obj2Model.rotation;
            Obj2.position = tempPos;
            Obj2Model.rotation = tempRot;
            if(Obj2 == transform)
            {
                yaw = tempRot.y;
            }
            knifeCount = KnifeCount.Both;

            try {Destroy(Obj1.GetComponentInChildren<ParticleSystem>().gameObject);} catch {}
            try {Destroy(Obj2.GetComponentInChildren<ParticleSystem>().gameObject);} catch {}
        }
    }
}

public enum KnifeCount
{
    Both,
    One,
    Neither
}