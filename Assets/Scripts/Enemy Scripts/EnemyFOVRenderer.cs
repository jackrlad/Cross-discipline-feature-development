using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EnemyFOVRenderer : MonoBehaviour
{
    [Header("Mesh Settings")]
    [SerializeField] private int _rayCount = 40;
    [SerializeField] private float _groundOffset = 0.05f;

    [Header("State Colours")]
    [SerializeField] private Color _patrolColour      = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color _investigateColour = new Color(1f, 0.9f, 0f, 0.4f);
    [SerializeField] private Color _chaseColour       = new Color(1f, 0.1f, 0.1f, 0.5f);
    [SerializeField] private Color _confusedColour    = new Color(0.5f, 0f, 1f, 0.4f);

    private Mesh _mesh;
    private MeshRenderer _meshRenderer;
    private EnemyPerception _perception;
    private EnemyAI _enemyAI;

    private float _outerAngle;
    private float _innerAngle;
    private float _detectionRange;
    private float _peripheralRange;

    void Awake()
    {
        _perception   = GetComponentInParent<EnemyPerception>();
        _enemyAI      = GetComponentInParent<EnemyAI>();
        _meshRenderer = GetComponent<MeshRenderer>();

        Material fovMat = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        fovMat.color = _patrolColour;
        _meshRenderer.material = fovMat;

        _mesh      = new Mesh();
        _mesh.name = "FOV Mesh";
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    void Start()
    {
        _outerAngle      = _perception.OuterAngle;
        _innerAngle      = _perception.InnerAngle;
        _detectionRange  = _perception.DetectionRange;
        _peripheralRange = _perception.PeripheralRange;
    }

    void LateUpdate()
    {
        UpdateColour();
        DrawFOVMesh();
    }

    void UpdateColour()
    {
        Color target = _enemyAI.CurrentState switch
        {
            EnemyAI.AIState.Patrol      => _patrolColour,
            EnemyAI.AIState.Chase       => _chaseColour,
            EnemyAI.AIState.Investigate => _investigateColour,
            EnemyAI.AIState.Confused    => _confusedColour,
            _                           => _patrolColour
        };
        _meshRenderer.material.color = target;
    }

    void DrawFOVMesh()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y + _groundOffset, transform.position.z);
        float angleStep = (_outerAngle * 2f) / _rayCount;

        Vector3[] vertices  = new Vector3[_rayCount + 2];
        int[]     triangles = new int[_rayCount * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= _rayCount; i++)
        {
            float currentAngle    = -_outerAngle + angleStep * i;
            float angleFromCenter = Mathf.Abs(currentAngle);

            float effectiveRange = angleFromCenter <= _innerAngle
                ? _detectionRange
                : Mathf.Lerp(_detectionRange, _peripheralRange,
                    Mathf.InverseLerp(_innerAngle, _outerAngle, angleFromCenter));

            Vector3 rayDir = Quaternion.Euler(0, currentAngle, 0) * transform.forward;
            Vector3 worldHit;

            if (Physics.Raycast(origin, rayDir, out RaycastHit hit, effectiveRange))
                worldHit = hit.point;
            else
                worldHit = origin + rayDir * effectiveRange;

            worldHit.y = transform.position.y + _groundOffset;
            vertices[i + 1] = transform.InverseTransformPoint(worldHit);
        }

        for (int i = 0; i < _rayCount; i++)
        {
            triangles[i * 3]     = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        _mesh.Clear();
        _mesh.vertices  = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
    }
}