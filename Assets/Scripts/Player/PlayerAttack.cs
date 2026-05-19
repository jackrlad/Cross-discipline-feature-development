using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Collider AttackCollision;
    public float PlayerDamage = 10f;

    private InputReader ir;
    private SwapSystem swap;
    private KnifeCount knifeCount = KnifeCount.Both;
    private float MaxAttackCooldown = 0.3f;
    private float AttackCooldown;
    private bool _throwHeld = false;

    void Start()
    {
        ir = GetComponent<InputReader>();
        swap = GetComponent<SwapSystem>();
    }

    void Update()
    {
        // Melee attack — hold to swing, cooldown between swings
        if (ir.Attack)
        {
            if (AttackCooldown > 0)
                AttackCooldown -= Time.deltaTime;
            else
            {
                Attack();
                AttackCooldown = MaxAttackCooldown;
            }
        }
        else
        {
            AttackCooldown = MaxAttackCooldown;
        }

        // Knife throw — fires once per press
        if (ir.Throw && !_throwHeld)
        {
            knifeCount = swap.SwapHit();
            _throwHeld = true;
        }
        else if (!ir.Throw)
        {
            _throwHeld = false;
        }

        // Execute the swap
        if (ir.Swap)
        {
            knifeCount = swap.SwapTrigger();
        }
    }

    void Attack()
    {
        Debug.Log("I'm attacking!!");
        Collider[] hits = Physics.OverlapBox
        (
            AttackCollision.transform.position, 
            AttackCollision.transform.localScale / 2, 
            AttackCollision.transform.rotation
        );

        foreach (Collider col in hits)
        {
            IDamageable damageable = col.GetComponent<IDamageable>();

            if(col.gameObject != gameObject && damageable != null)
            {
                float damageConst;
                if (knifeCount == KnifeCount.Both)
                    damageConst = 1;
                else if(knifeCount == KnifeCount.One)
                    damageConst = 0.75f;
                else
                    damageConst = 0.5f;
                damageable.TakeDamage(PlayerDamage * damageConst);
            }
        }
    }
}
