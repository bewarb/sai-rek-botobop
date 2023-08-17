using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{
    public GameObject[] projectiles;
    public Transform projectileOrigin;
    
    public Image selectionImage;
    
    public float damageScale = 1.0f;
    public float speedScale = 1.0f;
    public float sizeScale = 1.0f;
    public float growScale = 1.0f;
    public float distanceScale = 1.0f;

    public float shootingAlertRadius = 10f;
    
    private Animator anim;
    private Transform projectileParent;
    private ThirdPersonShooterController shooterController;

    private int weaponType;
    private int currentWeapon;
    
    private bool isAiming;
    private Vector3 mouseWorldPosition;
    
    void Start()
    {
        shooterController = GetComponent<ThirdPersonShooterController>();
        anim = GetComponent<Animator>();
        projectileParent = GameObject.FindGameObjectWithTag("ProjectileParent").transform;
        
        currentWeapon = 0;
        ProjectileBehavior properties = projectiles[currentWeapon].GetComponent<ProjectileBehavior>();
        weaponType = properties.GetTypeInt();
        selectionImage.sprite = properties.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        isAiming = shooterController.isAiming;
        mouseWorldPosition = shooterController.mouseWorldPosition;
        
        SwitchWeapon();
        Attack();
    }
    
    void SwitchWeapon()
    {
        int prevWeapon = currentWeapon;
        
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            currentWeapon = (currentWeapon + 1) % projectiles.Length;
        if (Input.GetAxis("Mouse ScrollWheel") < 0) 
            currentWeapon = (currentWeapon + projectiles.Length - 1) % projectiles.Length;
        
        if (prevWeapon != currentWeapon)
        {
            ProjectileBehavior properties = projectiles[currentWeapon].GetComponent<ProjectileBehavior>();
            weaponType = properties.GetTypeInt();
            selectionImage.sprite = properties.sprite;
        }
    }
    
    void Attack()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle_Walk_Run") && Input.GetButtonDown("Fire1"))
        {
            if (weaponType == 0)
            {
                anim.SetBool("attackRanged", true);
                Invoke("FireProjectile", 0.2f);
                AlertNearby();
            }
            else if (weaponType == 1)
            {
                anim.SetBool("attackMelee", true);
                Invoke("FireMelee", 0.2f);
            }
        }
    }
    
    void FireProjectile()
    {
        Quaternion bulletDirection = Quaternion.identity;
        if (isAiming)
        {
            Vector3 aimDirection = (mouseWorldPosition - projectileOrigin.position).normalized;
            bulletDirection = Quaternion.LookRotation(aimDirection, Vector3.up);
        }
        else
        {
            bulletDirection = transform.rotation;
        }

        GameObject curProjectilePrefab = projectiles[currentWeapon];
        
        GameObject projectile = Instantiate
            (curProjectilePrefab, projectileOrigin.position, bulletDirection) as GameObject;

        ConfigureProjectile(projectile);

        projectile.transform.SetParent(projectileParent);
    }

    void FireMelee()
    {
        GameObject curProjectilePrefab = projectiles[currentWeapon];
        
        GameObject projectile = Instantiate
            (curProjectilePrefab, projectileOrigin.position + transform.forward * 2, transform.rotation) as GameObject;

        ConfigureProjectile(projectile);

        projectile.transform.SetParent(projectileParent);
    }

    void ConfigureProjectile(GameObject projectile)
    {
        ProjectileBehavior projectileScript = projectile.GetComponent<ProjectileBehavior>();
        
        projectileScript.SetProperties(damageScale, speedScale, sizeScale, growScale, distanceScale);
        
        projectileScript.SetReady(true);
    }
    
    void AlertNearby()
    {
        Collider[] others =  Physics.OverlapSphere(transform.position, shootingAlertRadius);

        foreach(Collider other in others)
        {
            if (other.gameObject.CompareTag("Astronaut"))
            {
                AstronautAI astronaut = other.gameObject.GetComponent<AstronautAI>();
                if (astronaut.currentState == AstronautAI.FSMStates.Idle)
                    astronaut.Alert();
            }
            if (other.gameObject.CompareTag("Robot"))
            {
                RobotAI robot = other.gameObject.GetComponent<RobotAI>();
                if (robot.currentState == RobotAI.FSMStates.Idle) 
                    robot.Alert();
            }
            if (other.gameObject.CompareTag("Soldier"))
            {
                SoldierAI soldier = other.gameObject.GetComponent<SoldierAI>();
                if (soldier.currentState == SoldierAI.FSMStates.Idle 
                    || soldier.currentState == SoldierAI.FSMStates.Patrol) 
                    soldier.Alert();
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, shootingAlertRadius);
    }
    
}
