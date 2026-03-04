using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    public GameObject SelectionPrefab;
    public Transform PlayerModel;
    public Transform CameraArm;
    public Transform Camera;
    
    

    private Transform SelectedObject1 = null;
    private Transform SelectedObject2 = null;
    private KnifeCount knifeCount = KnifeCount.Both;
    private Rigidbody rb;

    private Vector3 SpawnPos = new Vector3(-5, 1, 0);
    private PlayerHealth _playerHealth;
    
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
        _playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        Movement();
        UpdateAim();
        SwapControls();
        RotCamera();

        CameraArm.position = transform.position;

        
        
       
    }

    

    void UpdateAim()
{
    Ray ray = Camera.GetComponent<UnityEngine.Camera>().ScreenPointToRay(Input.mousePosition);

    // Raycast against everything to find what the mouse is pointing at
    if (Physics.Raycast(ray, out RaycastHit hit, 100f))
    {
        Vector3 targetPoint = hit.point;
        targetPoint.y = transform.position.y;

        Vector3 toTarget = targetPoint - transform.position;
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
        if (Physics.Raycast(groundRay, out hit, 2f))
            {
            // Build target rotation from surface normal while preserving Y rotation
                Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Quaternion targetRotation = surfaceRotation * Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
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
    Ray knifeRay = Camera.GetComponent<UnityEngine.Camera>().ScreenPointToRay(Input.mousePosition);

    if (Physics.Raycast(knifeRay, out RaycastHit cameraHit, 100f))
    {
        if (cameraHit.collider.gameObject.layer == 3)
        {
            // Check the player has a clear line of sight to the target
            // Fire a second ray from the player to the hit object
            Vector3 directionToTarget = (cameraHit.point - transform.position).normalized;
            bool hasLineOfSight = !Physics.Raycast(
                transform.position,
                directionToTarget,
                out RaycastHit losHit,
                Vector3.Distance(transform.position, cameraHit.point)
            ) || losHit.collider == cameraHit.collider;

            if (!hasLineOfSight)
            {
                Debug.Log("[PlayerMovement] No line of sight to target — knife blocked.");
                return;
            }

            if (knifeCount == KnifeCount.Both)
            {
                SelectedObject1 = cameraHit.collider.gameObject.transform;
                SelectedObject2 = transform;
                knifeCount = KnifeCount.One;

                var prefab = Instantiate(SelectionPrefab);
                prefab.transform.parent = SelectedObject1;
                prefab.transform.localPosition = new Vector3(0, 1.5f, 0);
            }
            else if (knifeCount == KnifeCount.One && SelectedObject1 != cameraHit.collider.gameObject.transform)
            {
                SelectedObject2 = cameraHit.collider.gameObject.transform;
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
        if (knifeCount == KnifeCount.One)
        {
            Swap(SelectedObject1, SelectedObject1, transform, PlayerModel);
            SelectedObject1 = null;
            SelectedObject2 = null;
        }
        else if (knifeCount == KnifeCount.Neither)
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

        // Warp any NavMeshAgent to the new position so the agent
        // doesn't reject the position change on a different floor
        NavMeshAgent obj1Agent = Obj1.GetComponent<NavMeshAgent>();
        NavMeshAgent obj2Agent = Obj2.GetComponent<NavMeshAgent>();

        if (obj1Agent != null) obj1Agent.Warp(Obj2.position);
        else Obj1.position = Obj2.position;
        Obj1Model.rotation = Obj2Model.rotation;

        if (obj2Agent != null) obj2Agent.Warp(tempPos);
        else Obj2.position = tempPos;
        Obj2Model.rotation = tempRot;

        knifeCount = KnifeCount.Both;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        

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