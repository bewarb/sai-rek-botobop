using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletBehavior : MonoBehaviour
{
    public int damageAmount = 20;
    public float duration = 5f;
    
    void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            var playerHealth = other.collider.GetComponent<PlayerHealth>();
            playerHealth.TakeDamage(damageAmount);
        }
        Destroy(gameObject);
    }
}
