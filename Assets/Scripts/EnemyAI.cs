using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, ITeleportable
{
    [Header("Detection")]
    [SerializeField] private float _detectionRange = 10f;
    [SerializeField] private float _attackRange = 2f;

    [Header("Investigate")]
    [SerializeField] private float _investigateDuration = 5f;

    private enum AIState { Patrol, Chase, Investigate }
    private AIState _currentState = AIState.Patrol;

    private NavMeshAgent _agent;
    private Transform _player;
    private Vector3? _lastKnownPosition;
    private float _investigateTimer;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
        else
            Debug.LogError("[EnemyAI] No GameObject tagged 'Player' found!");
    }

    void Update()
    {
        if (_player == null) return;

        switch (_currentState)
        {
            case AIState.Patrol:      UpdatePatrol();      break;
            case AIState.Chase:       UpdateChase();       break;
            case AIState.Investigate: UpdateInvestigate(); break;
        }
    }

    void UpdatePatrol()
    {
        // Patrol points can be added here later — for now the enemy stands still
        _agent.ResetPath();

        if (CanSeePlayer())
        {
            _lastKnownPosition = _player.position;
            TransitionTo(AIState.Chase);
        }
    }

    void UpdateChase()
    {
        if (!CanSeePlayer())
        {
            TransitionTo(AIState.Investigate);
            return;
        }

        _lastKnownPosition = _player.position;
        _agent.SetDestination(_player.position);

        if (Vector3.Distance(transform.position, _player.position) <= _attackRange)
            Debug.Log("[EnemyAI] In attack range!"); // Attack logic goes here
    }

    void UpdateInvestigate()
    {
        if (!_lastKnownPosition.HasValue)
        {
            Debug.Log("[EnemyAI] No last known position (teleported). Returning to Patrol.");
            TransitionTo(AIState.Patrol);
            return;
        }

        _agent.SetDestination(_lastKnownPosition.Value);

        if (CanSeePlayer())
        {
            _lastKnownPosition = _player.position;
            TransitionTo(AIState.Chase);
            return;
        }

        _investigateTimer += Time.deltaTime;
        if (_investigateTimer >= _investigateDuration)
        {
            _investigateTimer = 0f;
            TransitionTo(AIState.Patrol);
        }
    }

    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position, _player.position) > _detectionRange) return false;

        Vector3 dir = (_player.position - transform.position).normalized;
        return Physics.Raycast(transform.position, dir, out RaycastHit hit, _detectionRange) && hit.transform == _player;
    }

    public void OnTeleported()
    {
        _lastKnownPosition = null;
        _investigateTimer = 0f;
        TransitionTo(AIState.Patrol);
        Debug.Log($"[EnemyAI] {gameObject.name} teleported — state reset.");
    }

    void TransitionTo(AIState newState)
    {
        if (_currentState == newState) return;
        Debug.Log($"[EnemyAI] {gameObject.name}: {_currentState} → {newState}");
        _currentState = newState;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
