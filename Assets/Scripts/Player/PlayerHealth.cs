using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IHealth
{    
    public Transform HealthDisplay;
    
    private Vector3 SpawnPos = new Vector3(-5, 1, 0);
    private float MaxHealth = 100;
    private float Health = 100;
    
    void Start()
    {
        
    }

    void Update()
    {
        if(Health <= 0)
        {
            Health = MaxHealth;
            transform.position = SpawnPos;
        }

        HealthDisplay.localScale = new Vector3
        (
            Mathf.Lerp(HealthDisplay.localScale.x, Health / 100 * 3, 0.1f), 
            HealthDisplay.localScale.y, 
            HealthDisplay.localScale.z
        );
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;
    }
}
