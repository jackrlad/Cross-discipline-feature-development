using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
 
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject SelectionPrefab;
    public Transform PlayerModel;
    public Transform CameraArm;
    public Transform Camera;
 
    [Header("Movement")]
    public float SPEED = 3000;
    public float JUMPFORCE = 100;
    public float SprintConst = 1.8f;
 
    private Rigidbody rb;
    private PlayerHealth _playerHealth;
    private Vector3 SpawnPos = new Vector3(-5, 1, 0);
 
    private Vector3 forward = new Vector3(1, 0, -1);
    private Vector3 right = new Vector3(-1, 0, -1);
    private Vector3 aimDirection = Vector3.forward;
    private float cameraRot = 0;
    private bool _onGround = false;
 
    private Transform SelectedObject1 = null;
    private Transform SelectedObject2 = null;
    private KnifeCount knifeCount = KnifeCount.Both;
 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _playerHealth = GetComponent<PlayerHealth>();
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
    }
 
    void UpdateAim()
    {
        Ray ray = Camera.GetComponent<UnityEngine.Camera>().ScreenPointToRay(Input.mousePosition);
 
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
 
        if (Input.GetKey(KeyCode.W)) velocity += forward;
        if (Input.GetKey(KeyCode.S)) velocity -= forward;
        if (Input.GetKey(KeyCode.D)) velocity += right;
        if (Input.GetKey(KeyCode.A)) velocity -= right;
 
        velocity = velocity.normalized;
 
        // Ground check — also aligns player to surface normal
        Ray groundRay = new Ray(transform.position, Vector3.down);
        Debug.DrawRay(groundRay.origin, groundRay.direction, Color.green);
 
        if (Physics.Raycast(groundRay, out RaycastHit hit, 2f))
        {
            _onGround = true;
            Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Quaternion targetRotation = surfaceRotation * Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
        }
        else
        {
            _onGround = false;
            transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
 
        if (Input.GetKeyDown(KeyCode.Space) && _onGround)
            rb.AddForce(PlayerModel.up * JUMPFORCE, ForceMode.Impulse);
 
        if (Input.GetKey(KeyCode.LeftShift))
            velocity *= SprintConst;
 
        rb.AddForce(velocity * Time.deltaTime * SPEED, ForceMode.Force);
    }
 
    void RotCamera()
    {
        cameraRot = 0;
 
        if (Input.GetKey(KeyCode.Q)) cameraRot -= 90;
        if (Input.GetKey(KeyCode.E)) cameraRot += 90;
 
        CameraArm.rotation = Quaternion.Lerp(
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
 
            if (!Physics.Raycast(knifeRay, out RaycastHit cameraHit, 100f)) return;
            if (cameraHit.collider.gameObject.layer != 3) return;
 
            // Second LOS ray from player to confirm no obstruction
            Vector3 dirToTarget = (cameraHit.point - transform.position).normalized;
            bool hasLOS = !Physics.Raycast(
                transform.position,
                dirToTarget,
                out RaycastHit losHit,
                Vector3.Distance(transform.position, cameraHit.point)
            ) || losHit.collider == cameraHit.collider;
 
            if (!hasLOS)
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
    if (!Obj1 || !Obj2) return;

    Vector3 tempPos = Obj1.position;
    Quaternion tempRot = Obj1Model.rotation;

    MoveSwappable(Obj1, Obj2.position);
    Obj1Model.rotation = Obj2Model.rotation;

    MoveSwappable(Obj2, tempPos);
    Obj2Model.rotation = tempRot;

    knifeCount = KnifeCount.Both;
    rb.velocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;

    try { Destroy(Obj1.GetComponentInChildren<ParticleSystem>().gameObject); } catch { }
    try { Destroy(Obj2.GetComponentInChildren<ParticleSystem>().gameObject); } catch { }

    Obj1.GetComponent<ITeleportable>()?.OnTeleported();
    Obj2.GetComponent<ITeleportable>()?.OnTeleported();
}

void MoveSwappable(Transform obj, Vector3 targetPos)
{
    NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();

    if (agent != null)
    {
        // Check if the destination actually has a NavMesh nearby
        bool hasNavMesh = NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 2f, NavMesh.AllAreas);

        if (hasNavMesh)
        {
            // Valid NavMesh at destination — Warp keeps the agent happy on cross-floor swaps
            agent.Warp(hit.position);
        }
        else
        {
            // No NavMesh (void, off map) — disable agent and drop them with physics
            agent.enabled = false;
            obj.position = targetPos;

            Rigidbody agentRb = obj.GetComponent<Rigidbody>();
            if (agentRb != null)
            {
                agentRb.isKinematic = false;
                agentRb.velocity = Vector3.zero;
            }
        }
    }
    else
    {
        obj.position = targetPos;
    }
}
}
 
public enum KnifeCount
{
    Both,
    One,
    Neither
}