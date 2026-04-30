using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour, IDamageable
{
    public float Health = 1f;
    public GameObject HitParticle;
    public void TakeDamage(float damage)
    {
        Health -= damage;
        var particle = Instantiate(HitParticle);
        particle.transform.position = gameObject.transform.position;

        Destroy(particle, 1f);

        if(Health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
