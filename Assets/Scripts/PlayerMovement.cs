using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject SelectionPrefab;
    public Transform PlayerModel;
    public Transform CameraArm;
    public Transform Camera;
    public Transform HealthDisplay;
    public Transform CrosshairMarker;

    private Transform SelectedObject1 = null;
    private Transform SelectedObject2 = null;
    private KnifeCount knifeCount = KnifeCount.Both;
    private Rigidbody rb;

    private Vector3 SpawnPos = new Vector3(-5, 1, 0);
    private float MaxHealth = 100;
    private float Health = 100;
    
    private Vector3 forward = new Vector3(1, 0, -1);
    private Vector3 right = new Vector3(-1, 0, -1);
    private float cameraRot = 0;
    private float SPEED = 3000;
    private float SprintConst = 1.8f;
    private Vector3 aimDirection = Vector3.forward;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    void Update()
    {
        Movement();
        UpdateAim();
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

    void UpdateAim()
    {
        Ray ray = Camera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            worldPoint.y = transform.position.y;

            if (CrosshairMarker != null)
                CrosshairMarker.position = worldPoint;

            Vector3 toTarget = worldPoint - transform.position;
            if (toTarget.sqrMagnitude > 0.01f)
            {
                aimDirection = toTarget.normalized;
                float targetYaw = Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg;
                PlayerModel.localRotation = Quaternion.Euler(0f, targetYaw, 0f);
            }
        }
    }

    void Movement()
    {
        Vector3 velocity = Vector3.zero;

        if(Input.GetKey(KeyCode.W))
        {
            velocity += forward;
        }
        if(Input.GetKey(KeyCode.S))
        {
            velocity -= forward;
        }
        if(Input.GetKey(KeyCode.D))
        {
            velocity += right;
        }
        if(Input.GetKey(KeyCode.A))
        {
            velocity -= right;
        }

        velocity = velocity.normalized;



        Ray groundRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Debug.DrawRay(groundRay.origin, groundRay.direction, Color.green);
        if(Physics.Raycast(groundRay, out hit, 2f))
        {
            transform.rotation = Quaternion.Euler
            (
                hit.collider.gameObject.transform.rotation.eulerAngles.x, 
                transform.rotation.eulerAngles.y, 
                hit.collider.gameObject.transform.rotation.eulerAngles.z
            );
            
        }



        if(Input.GetKey(KeyCode.LeftShift))
        {
            velocity = velocity * SprintConst;
        }
        rb.AddForce(velocity * Time.deltaTime * SPEED, ForceMode.Force);
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
            Ray KnifeRay = new Ray(transform.position, aimDirection);
            RaycastHit hit;
            Debug.DrawRay(KnifeRay.origin, KnifeRay.direction, Color.red);
            if(Physics.Raycast(KnifeRay.origin, KnifeRay.direction, out hit))
            {
                if(hit.collider.gameObject.layer == 3) {
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
            knifeCount = KnifeCount.Both;

            
            rb.velocity = Vector3.zero;

            try {Destroy(Obj1.GetComponentInChildren<ParticleSystem>().gameObject);} catch {}
            try {Destroy(Obj2.GetComponentInChildren<ParticleSystem>().gameObject);} catch {}

            Obj1.GetComponent<ITeleportable>()?.OnTeleported();
            Obj2.GetComponent<ITeleportable>()?.OnTeleported();
        }
    }
}

public enum KnifeCount
{
    Both,
    One,
    Neither
}