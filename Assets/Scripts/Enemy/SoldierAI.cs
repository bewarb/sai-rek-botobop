using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierAI : MonoBehaviour
{
    public enum FSMStates
    {
        Idle,
        Patrol,
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
    
    public Transform muzzle;
    
    public GameObject[] patrolPoints;
    private int currentDestinationIndex = 0;
    private Vector3 nextDestination;

    private float distanceToPlayer;
    private bool playerInFOV;
    private float elapsedTime = 0;
    private Vector3 alertPosition;

    private EnemyHealth enemyHealth;
    private int health;
    private bool isDead;

    public Transform enemyEyes;
    public float fieldOfView = 45f;

    private float curAttackDistance = 15.0f;
    private float curChaseDistance = 20.0f;
    private float curFOV = 45f;
    
    Animator anim;
    private UnityEngine.AI.NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerTarget = GameObject.FindGameObjectWithTag("PlayerTarget");
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        enemyHealth = GetComponent<EnemyHealth>();
        health = enemyHealth.currentHealth;
        isDead = false;

        ReturnToNeutral();
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelManager.isGameOver) return;
        
        UpdateValues();

        print(gameObject.name + " " + currentState);
        switch (currentState)
        {   
            case FSMStates.Idle:
                UpdateIdleState();
                break;
            case FSMStates.Patrol:
                UpdatePatrolState();
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
        if (currentState == FSMStates.Idle || currentState == FSMStates.Patrol)
        {
            this.currentState = FSMStates.Alert;
            elapsedTime = 0;
            alertPosition = player.transform.position;
        }
    }

    void UpdateIdleState()
    {
        anim.SetInteger("animState", 0);
        if (IsPlayerInClearFOV())
        {
            currentState = FSMStates.Chase;
        }
    }

    void UpdatePatrolState()
    {
        anim.SetInteger("animState", 1);

        agent.stoppingDistance = 0f;
        agent.speed = 3.5f;

        if (Vector3.Distance(transform.position, nextDestination) < 2)
        {
            FindNextPoint();
        }
        else if (IsPlayerInClearFOV())
        {
            currentState = FSMStates.Chase;
        }

        FaceTarget(nextDestination);

        agent.SetDestination(nextDestination);
    }

    void UpdateAlertState()
    {
        anim.SetInteger("animState", 2);
        
        agent.isStopped = true;
        
        if (IsPlayerInClearFOV())
        {
            agent.isStopped = false;
            currentState = FSMStates.Chase;
        }

        if (elapsedTime > alertTimer)
        {
            agent.isStopped = false;
            ReturnToNeutral();
        }

        FaceTarget(alertPosition);
    }

    void UpdateChaseState()
    {
        anim.SetInteger("animState", 3);

        agent.stoppingDistance = curAttackDistance * 0.75f;
        agent.speed = enemySpeed;

        nextDestination = player.transform.position;

        if (distanceToPlayer <= curAttackDistance + .5f)
        {
            currentState = FSMStates.Attack;
            anim.SetInteger("strafeState", 1);
        }
        else if (distanceToPlayer > curChaseDistance + .5f)
        {
            ReturnToNeutral();
        }

        FaceTarget(nextDestination);
        agent.SetDestination(nextDestination);
    }

    void UpdateAttackState()
    {
        anim.SetInteger("animState", 4);
        var strafeSpeed = enemySpeed * 0.5f;
        
        agent.stoppingDistance = attackDistance * 0.75f;
        agent.speed = strafeSpeed;

        nextDestination = player.transform.position;

        if (distanceToPlayer > curAttackDistance + .5f && distanceToPlayer <= curChaseDistance)
        {
            currentState = FSMStates.Chase;
        }
        else if (distanceToPlayer > curChaseDistance)
        {
            ReturnToNeutral();
        }
        
        if (distanceToPlayer < curAttackDistance * 0.67f)
        {
            anim.SetInteger("strafeState", 2);
            agent.isStopped = true;
            
            transform.position = Vector3.Lerp
                (transform.position, transform.position - transform.forward, Time.deltaTime * strafeSpeed);
        }
        else
        {
            agent.isStopped = false;
            if (agent.velocity != Vector3.zero) anim.SetInteger("strafeState", 1);
            else anim.SetInteger("strafeState", 0);
        }
        
        FaceTarget(nextDestination);
        ShootProjectile();
    }

    void UpdateDeadState()
    {
        if (isDead) return;
        
        isDead = true;
        agent.isStopped = true;
        anim.SetInteger("animState", 5);

        AudioSource.PlayClipAtPoint(deadSFX, transform.position);
        Destroy(gameObject, 3f);
    }

    void ReturnToNeutral()
    {
        if (patrolPoints.Length > 0)
        {
            currentState = FSMStates.Patrol;
            FindNextPoint();
        }
        else
        {
            currentState = FSMStates.Idle;
            agent.SetDestination(transform.position);
        }
    }
    void FindNextPoint()
    {
        nextDestination = patrolPoints[currentDestinationIndex].transform.position;
        currentDestinationIndex = (currentDestinationIndex + 1) % patrolPoints.Length;

        agent.SetDestination(nextDestination);
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
            GameObject bullet = Instantiate
                (bulletPrefab, muzzle.position + muzzle.forward, muzzle.rotation) as GameObject;

            bullet.transform.LookAt(playerTarget.transform.position);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(bullet.transform.forward * bulletSpeed, ForceMode.VelocityChange);

            bullet.transform.SetParent(GameObject.FindGameObjectWithTag("ProjectileParent").transform);

            AudioSource.PlayClipAtPoint(shootSFX, muzzle.position);

            elapsedTime = 0f;
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
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
