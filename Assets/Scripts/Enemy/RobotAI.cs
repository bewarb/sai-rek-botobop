using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class RobotAI : MonoBehaviour
{
    public enum FSMStates
    {
        Idle,
        Alert,
        Chase,
        Attack,
        Dead
    }

    public FSMStates currentState;

    public float attackDistance = 15.0f;
    public float chaseDistance = 20.0f;
    public float enemySpeed = 3.0f;
    public float shootRate = 1.0f;
    public float alertTimer = 5.0f;
    public float bulletSpeed = 25f;

    public GameObject player;
    public GameObject playerTarget;
    public GameObject bulletPrefab;

    public AudioClip shootSFX;
    public AudioClip deadSFX;

    public Transform barrel1;
    public Transform barrel2;

    private Vector3 nextDestination;

    private float distanceToPlayer;
    private bool playerInFOV;
    private float elapsedTime = 0;
    private Vector3 alertPosition;

    private EnemyHealth enemyHealth;
    private int health;

    public Transform enemyEyes;
    public float fieldOfView = 45f;

    private float curAttackDistance = 15.0f;
    private float curChaseDistance = 20.0f;
    private float curFOV = 45f;
    
    private NavMeshAgent agent;

    private int barrel;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerTarget = GameObject.FindGameObjectWithTag("PlayerTarget");
        agent = GetComponent<NavMeshAgent>();

        enemyHealth = GetComponent<EnemyHealth>();
        health = enemyHealth.currentHealth;

        currentState = FSMStates.Idle;
    }

    void Update()
    {
        if (LevelManager.isGameOver) return;

        UpdateValues();

        switch (currentState)
        {
            case FSMStates.Idle:
                UpdateIdleState();
                break;
            case FSMStates.Alert:
                UpdateAlertState();
                break;
            case FSMStates.Chase:
                UpdateChaseState();
                break;
            case FSMStates.Attack:
                UpdateAttackState();
                break;
            case FSMStates.Dead:
                UpdateDeadState();
                break;
        }

        elapsedTime += Time.deltaTime;

        if (health <= 0)
        {
            currentState = FSMStates.Dead;
        }
    }

    void UpdateValues()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        int prevHealth = health;
        health = enemyHealth.currentHealth;

        if (health < prevHealth)
        {
            Alert();
        }
        
        if (LevelManager.isLightOn)
        {
            curAttackDistance = attackDistance;
            curChaseDistance = chaseDistance;
            curFOV = fieldOfView;
        }
        else
        {
            curAttackDistance = attackDistance * 0.5f;
            curChaseDistance = chaseDistance * 0.5f;
            curFOV = fieldOfView * 1.25f;
        }
    }

    public void Alert()
    {
        if (currentState == FSMStates.Idle)
        {
            this.currentState = FSMStates.Alert;
            elapsedTime = 0;
            alertPosition = player.transform.position;
        }
    }

    void UpdateIdleState()
    {
        if (IsPlayerInClearFOV())
        {
            currentState = FSMStates.Chase;
        }
    }

    void UpdateAlertState()
    {
        if (IsPlayerInClearFOV())
        {
            currentState = FSMStates.Chase;
        }
        else if (elapsedTime > alertTimer)
        {
            currentState = FSMStates.Idle;
        }

        FaceTarget(alertPosition);
    }

    void UpdateChaseState()
    {
        agent.stoppingDistance = attackDistance;
        agent.speed = enemySpeed;

        nextDestination = player.transform.position;

        if (distanceToPlayer <= curAttackDistance + .5f)
        {
            currentState = FSMStates.Attack;
        }
        else if (distanceToPlayer > curChaseDistance)
        {
            currentState = FSMStates.Idle;
        }

        FaceTarget(nextDestination);
        agent.SetDestination(nextDestination);
    }

    void UpdateAttackState()
    {
        agent.stoppingDistance = attackDistance;
        
        nextDestination = player.transform.position;

        if (distanceToPlayer <= curAttackDistance)
        {
            currentState = FSMStates.Attack;
        }
        else if (distanceToPlayer > curAttackDistance + .5f && distanceToPlayer <= curChaseDistance)
        {
            currentState = FSMStates.Chase;
        }
        else if (distanceToPlayer > curChaseDistance)
        {
            currentState = FSMStates.Idle;
        }

        FaceTarget(nextDestination);

        ShootProjectile();
    }

    void UpdateDeadState()
    {
        agent.isStopped = true;
        AudioSource.PlayClipAtPoint(deadSFX, transform.position);
        Destroy(gameObject);
    }

    public void FaceTarget(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;
        directionToTarget.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10 * Time.deltaTime);
    }

    void ShootProjectile()
    {
        if (elapsedTime > shootRate)
        {
            Transform bulletSource = barrel % 2 == 0 ? barrel1 : barrel2;
            barrel = (barrel + 1) % 2;

            GameObject bullet = Instantiate
                (bulletPrefab, bulletSource.position + bulletSource.forward, bulletSource.rotation) as GameObject;

            bullet.transform.LookAt(playerTarget.transform.position);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(bullet.transform.forward * bulletSpeed, ForceMode.VelocityChange);

            bullet.transform.SetParent(GameObject.FindGameObjectWithTag("ProjectileParent").transform);

            AudioSource.PlayClipAtPoint(shootSFX, bulletSource.position);

            elapsedTime = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        // attack
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, curAttackDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, curChaseDistance);
    }

    // courtesy of Caglar Yildrim
    bool IsPlayerInClearFOV()
    {
        RaycastHit hit;

        Vector3 directionToPlayer = player.transform.position - enemyEyes.position;

        if (Vector3.Angle(directionToPlayer, enemyEyes.forward) <= curFOV)
        {
            if (Physics.Raycast(enemyEyes.position, directionToPlayer, out hit, curChaseDistance))
            {
                if (hit.collider.CompareTag("PlayerDetector"))
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        return false;
    }
}