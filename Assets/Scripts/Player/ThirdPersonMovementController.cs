using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThirdPersonMovementController : MonoBehaviour
{
    public Text powerUpTimer;
    public Image powerUpImage;

    public float moveSpeed = 5f;
    public float jumpHeight = 3f;
    public float gravity = 9.81f;
    public float airControl = 1f;
    public float acceleration = 5f;
    public float alertRadius = 5f;

    private CharacterController controller;
    private Animator anim;
    
    private float speedBoost = 1f;
    private float jumpBoost = 1f;
    private float countDown;
    
    private float angVelocity = 0f;
    private float speed = 0f;
    private bool rotateOnMove = true;
    public AudioClip jumpSFX;

    private bool isGrounded;
    private bool isRunning;
    public float currentAlertRadius;
    private Vector3 moveDirection;
    private float rotateAngle;
    private Color clean = new Color();

    void Start()
    {
        //if (PlayerPrefs.HasKey("PlayerPositionX") &&
        //    PlayerPrefs.HasKey("PlayerPositionY") &&
        //    PlayerPrefs.HasKey("PlayerPositionZ"))
        //{
        //    float posX = PlayerPrefs.GetFloat("PlayerPositionX");
        //    float posY = PlayerPrefs.GetFloat("PlayerPositionY");
        //    float posZ = PlayerPrefs.GetFloat("PlayerPositionZ");
        //    transform.position = new Vector3(posX, posY, posZ);
        //}
        
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();

        clean.a = 0f;

        powerUpImage.color = clean;
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelManager.isGameOver) return;
        
        Setup();
        Jump();
        Move();

        if (countDown > 0)
        {
            countDown -= Time.deltaTime;

            powerUpTimer.text = countDown.ToString("f2");
        } else
        {
            powerUpTimer.text = "";
        }
    }
    
    void Setup()
    {
        moveDirection = new Vector3(0, moveDirection.y, 0);
        isGrounded = controller.isGrounded;
        anim.SetBool("isJumping", false);
        anim.SetBool("isGrounded", isGrounded);
    }
    
    void Jump()
    {
        // on the ground start jumping
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            AudioSource.PlayClipAtPoint(jumpSFX,transform.position);
            moveDirection.y = Mathf.Sqrt(2 * jumpHeight * jumpBoost * gravity);
        } 
        // on the ground
        else if (isGrounded)
        {
            moveDirection.y = 0.0f;
        }
        // gravity as constant force
        moveDirection.y -= gravity * Time.deltaTime;
    }
    
    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        
        isRunning = Input.GetKey(KeyCode.LeftShift);
        currentAlertRadius = isRunning ? alertRadius * 1.5f : alertRadius;
        float targetSpeed = isRunning ? moveSpeed * 2 : moveSpeed;
        targetSpeed *= speedBoost;

        float currentSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
        Vector3 input = new Vector3(moveHorizontal, 0, moveVertical);
        
        bool isStopped = input.magnitude == 0;
        
        if (isStopped)
        {
            speed = 0.0f;
        }
        else if (currentSpeed > targetSpeed + 0.1 || currentSpeed < targetSpeed - 0.1)
        {
            speed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            speed = targetSpeed;
        }

        anim.SetFloat("Speed", speed);
        input.Normalize();
        
        if (!isStopped)
        {
            rotateAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + 
                          Camera.main.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle
                (transform.eulerAngles.y, rotateAngle, ref angVelocity, Time.deltaTime * 0.05f);
            
            if (rotateOnMove)
            {
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
        }

        Vector3 targetDirection = (Quaternion.Euler(0.0f, rotateAngle, 0.0f) * Vector3.forward).normalized;
        
        if (!isGrounded)
        {
            targetDirection *= airControl;
        }

        moveDirection += targetDirection * speed;
        controller.Move(moveDirection * Time.deltaTime);

        AlertNearby();
    }

    public void SetRotateOnMove(bool newRotateOnMove)
    {
        rotateOnMove = newRotateOnMove;
    }

    public void PowerUp(string pickup, float duration, Sprite sprite)
    {
        PowerDown();
        Invoke("PowerDown", duration);

        powerUpImage.sprite = sprite;
        powerUpImage.color = new Color(255,255,255,1);
        countDown = duration;

        if(pickup == "speed")
        {
            speedBoost = 1.5f;
            jumpBoost = 1;
        } else if (pickup == "jump")
        {
            speedBoost = 1;
            jumpBoost = 2f;
        }
    }

    void PowerDown()
    {
        powerUpImage.color = clean;
        speedBoost = 1f;
        jumpBoost = 1f;
    }
    void AlertNearby()
    {
        Collider[] others =  Physics.OverlapSphere(transform.position, currentAlertRadius);

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
                Debug.Log(other.gameObject.name);
                SoldierAI soldier = other.gameObject.GetComponent<SoldierAI>();
                if (soldier.currentState == SoldierAI.FSMStates.Idle ||
                    soldier.currentState == SoldierAI.FSMStates.Patrol) 
                    soldier.Alert();
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentAlertRadius);
    }
    
    private void OnApplicationQuit()
    {
        // Save the player position to PlayerPrefs when the application quits
        PlayerPrefs.SetFloat("PlayerPositionX", transform.position.x);
        PlayerPrefs.SetFloat("PlayerPositionY", transform.position.y);
        PlayerPrefs.SetFloat("PlayerPositionZ", transform.position.z);
    }
}
