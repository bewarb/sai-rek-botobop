using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class ThirdPersonShooterController : MonoBehaviour
{
    public CinemachineVirtualCamera aimVirtualCamera;
    public LayerMask aimColliderMask = new LayerMask();

    public bool isDebugging = false;
    public Transform debug;

    public float normalSensitivity = 1f;
    public float aimSensitivity = 0.6f;

    private Animator anim;
    
    private CameraController mouseLook;
    private ThirdPersonMovementController controller;
    
    public Vector3 mouseWorldPosition;
    public bool isAiming;
    
    private int weaponType;

    void Start()
    {
        mouseLook = GetComponent<CameraController>();
        controller = GetComponent<ThirdPersonMovementController>();
        anim = GetComponent<Animator>();

        normalSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 5) / 5f;
        isAiming = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (LevelManager.isGameOver) return;
        
        anim.SetBool("attackRanged", false);
        anim.SetBool("attackMelee", false);
        mouseWorldPosition = Vector3.zero;

        UpdatePointer();
        Aiming();
    }

    void UpdatePointer()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
        {
            if (isDebugging)
            {
                debug.position = raycastHit.point;
                Debug.Log(raycastHit.collider.name);
            }
            mouseWorldPosition = raycastHit.point;
        }
    }

    void Aiming()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            aimVirtualCamera.gameObject.SetActive(true);
            mouseLook.SetSensitivity(aimSensitivity);
            controller.SetRotateOnMove(false);
            isAiming = true;

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 10f);
        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
            mouseLook.SetSensitivity(normalSensitivity);
            controller.SetRotateOnMove(true);
            isAiming = false;
        }
    }
}
