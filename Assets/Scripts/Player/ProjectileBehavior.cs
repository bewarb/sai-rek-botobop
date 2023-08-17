using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public enum AttackType
    {
        Melee,
        Ranged
    }
    public int baseDamage = 10;
    public float baseSpeed = 25f;
    public float baseGrowRate = 1f;
    public float baseDistance = 125f;
    public AttackType type;
    public AudioClip fireSFX;
    public Sprite sprite;
    
    public Vector3 origin;
    
    private float damageScale = 1.0f;
    private float speedScale = 1.0f;
    private float sizeScale = 1.0f;
    private float growScale = 1.0f;
    private float distanceScale = 1.0f;

    private int finalDamage;
    private float finalSpeed;
    private float finalGrow;
    private float finalDistance;
    private float duration;

    private Vector3 startingScale;
    
    private bool isReady = false;
    private bool isFired = false;
    void Start()
    {
        origin = transform.position;
        startingScale = transform.localScale;
    }

    private void Update()
    {
        if (isFired)
        {
            // grows by finalGrow% of starting scale every frame
            // i.e. 1.1 will grow the projectile 10% of base scale every frame
            transform.localScale += (startingScale * (finalGrow - 1));
        }
        else if (isReady)
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            duration  = finalDistance / finalSpeed;

            rb.AddForce(transform.forward * finalSpeed, ForceMode.VelocityChange);
            
            AudioSource.PlayClipAtPoint(fireSFX, origin);

            Destroy(gameObject, duration);
            isFired = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Astronaut") || collision.gameObject.CompareTag("Robot") || collision.gameObject.CompareTag("Soldier"))
        {
            collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(finalDamage);
        }
        if (!(collision.gameObject.CompareTag("Player") 
              || collision.gameObject.CompareTag("EnemyVision")) 
              || collision.gameObject.CompareTag("PlayerDetector"))
        {
            Destroy(gameObject);
        }
    }
    

    public void SetProperties
    (float newDamageScale, float newSpeedScale, float newSizeScale, float newGrowScale,float newDistance)
    {
        damageScale = newDamageScale;
        speedScale = newSpeedScale;
        sizeScale = newSizeScale;
        growScale = newGrowScale;
        distanceScale = newDistance;

        UpdateProjectile();
    }

    private void UpdateProjectile()
    {
        finalDamage = Mathf.RoundToInt(baseDamage * damageScale);
        finalSpeed = baseSpeed * speedScale;
        finalGrow = baseGrowRate * growScale;
        finalDistance = baseDistance * distanceScale;
        
        // damage is damage, no action needed
        // speed is initial velocity, no action needed
        // size modifies scale
        transform.localScale *= sizeScale;
        // growth is updated per frame
        // distance determines duration of projectile in air
        duration = finalDistance / finalSpeed;

    }

    public void SetReady(bool status)
    {
        isReady = status;
    }

    public int GetTypeInt()
    {
        if (type == AttackType.Ranged) return 0;
        else return 1;
    }
}
