using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensitivity = 1f;
    
    public GameObject cameraTarget;

    private float yaw;
    private float pitch;
    
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void LateUpdate()
    {
        float moveX = Input.GetAxis("Mouse X") * sensitivity;
        float moveY = Input.GetAxis("Mouse Y") * sensitivity;

        yaw += moveX;
        pitch -= moveY;

        yaw = ClampAngle(yaw, float.MinValue, float.MaxValue);
        pitch = ClampAngle(pitch, -30f, 70f);

        cameraTarget.transform.rotation = Quaternion.Euler(pitch, yaw, 0.0f);
    }
    
    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        
        return Mathf.Clamp(angle, min, max);
    }

    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = newSensitivity;
    }
}
