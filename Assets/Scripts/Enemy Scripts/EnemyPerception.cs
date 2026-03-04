using UnityEngine;

public class EnemyPerception : MonoBehaviour
{
    [Header("Field of View")]
    [SerializeField, Range(0f, 180f)] private float _innerAngle = 40f;
    [SerializeField, Range(0f, 180f)] private float _outerAngle = 80f;
    [SerializeField, Min(0f)] private float _detectionRange = 10f;
    [SerializeField, Min(0f)] private float _peripheralRange = 4f;

    [Header("Line of Sight")]
    [Tooltip("Include the player's layer AND any layers that should block sight (walls, environment, etc.)")]
    [SerializeField] private LayerMask _sightBlockMask = -1;

    public float OuterAngle      => _outerAngle;
    public float InnerAngle      => _innerAngle;
    public float DetectionRange  => _detectionRange;
    public float PeripheralRange => _peripheralRange;

    private const int ArcSegments = 20;

    public bool CanSeePlayer(Transform origin, Transform player)
    {
        if (player == null) return false;

        Vector3 rawDirection = player.position - origin.position;
        float distanceToPlayer = rawDirection.magnitude;

        if (distanceToPlayer < Mathf.Epsilon) return true;

        
        Vector3 dirToPlayer = rawDirection / distanceToPlayer;
        float angleToPlayer = Vector3.Angle(origin.forward, dirToPlayer);

        if (angleToPlayer > _outerAngle) return false;

        // Full range inside inner cone, lerp down to peripheral range toward the outer edge
        float effectiveRange = angleToPlayer <= _innerAngle
            ? _detectionRange
            : Mathf.Lerp(_detectionRange, _peripheralRange,
                Mathf.InverseLerp(_innerAngle, _outerAngle, angleToPlayer));

        if (distanceToPlayer > effectiveRange) return false;

        Physics.Raycast(origin.position, dirToPlayer, out RaycastHit hit, effectiveRange, _sightBlockMask);
        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    private void OnValidate()
    {
        // Prevent the lerp inverting if these are set the wrong way round in the inspector
        _innerAngle = Mathf.Min(_innerAngle, _outerAngle);
        _peripheralRange = Mathf.Min(_peripheralRange, _detectionRange);
    }

    private void OnDrawGizmosSelected()
    {
        DrawConeGizmo(Color.green, _innerAngle, _detectionRange);
        DrawConeGizmo(new Color(1f, 0.5f, 0f), _outerAngle, _peripheralRange);
    }

    private void DrawConeGizmo(Color color, float halfAngle, float range)
    {
        Gizmos.color = color;

        Gizmos.DrawRay(transform.position, transform.forward * range);

        Vector3 left = Quaternion.Euler(0f, -halfAngle, 0f) * transform.forward * range;
        Vector3 right = Quaternion.Euler(0f, halfAngle, 0f) * transform.forward * range;
        Gizmos.DrawRay(transform.position, left);
        Gizmos.DrawRay(transform.position, right);

        float step = halfAngle * 2f / ArcSegments;
        Vector3 prev = transform.position + Quaternion.Euler(0f, -halfAngle, 0f) * transform.forward * range;

        for (int i = 1; i <= ArcSegments; i++)
        {
            Vector3 next = transform.position + Quaternion.Euler(0f, -halfAngle + step * i, 0f) * transform.forward * range;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
