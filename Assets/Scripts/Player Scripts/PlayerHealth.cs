using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _iFrameDuration = 1f;

    [Header("References")]
    [SerializeField] private Transform _healthDisplay;
    [SerializeField] private Transform _spawnPoint;

    private float _currentHealth;
    private float _iFrameTimer = 0f;
    private bool _isInvincible = false;

    public float HealthPercent => _currentHealth / _maxHealth;
    public bool IsDead => _currentHealth <= 0f;

    void Start()
    {
        _currentHealth = _maxHealth;
    }

    void Update()
    {
        if (_isInvincible)
        {
            _iFrameTimer += Time.deltaTime;
            if (_iFrameTimer >= _iFrameDuration)
            {
                _iFrameTimer = 0f;
                _isInvincible = false;
            }
        }

        if (_healthDisplay != null)
        {
            _healthDisplay.localScale = new Vector3
            (
                Mathf.Lerp(_healthDisplay.localScale.x, HealthPercent * 3f, 0.1f),
                _healthDisplay.localScale.y,
                _healthDisplay.localScale.z
            );
        }

        if (IsDead) Respawn();
    }

    public void TakeDamage(float damage)
    {
        if (_isInvincible) return;

        _currentHealth = Mathf.Max(_currentHealth - damage, 0f);
        _isInvincible = true;
        _iFrameTimer = 0f;
        Debug.Log($"[PlayerHealth] Took {damage} damage. Health: {_currentHealth}/{_maxHealth}");
    }

    void Respawn()
    {
        _currentHealth = _maxHealth;
        Vector3 spawnPos = _spawnPoint != null ? _spawnPoint.position : new Vector3(-5, 1, 0);
        transform.position = spawnPos;
        _isInvincible = false;
        Debug.Log("[PlayerHealth] Player respawned.");
    }
}