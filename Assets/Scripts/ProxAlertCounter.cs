using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProxAlertCounter : MonoBehaviour
{
    public static int alertedCounter;
    
    private GameObject player;
    private ThirdPersonMovementController controller;
    private bool detected;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        controller = player.GetComponent<ThirdPersonMovementController>();
        detected = false;
    }

    // Update is called once per frame
    void Update()
    {
        var distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        var alertThreshold = controller.currentAlertRadius + 5;
        
        if (distanceToPlayer <= alertThreshold && !detected)
        {
            detected = true;
            alertedCounter++;
        }
        else if (distanceToPlayer > alertThreshold && detected)
        {
            detected = false;
            alertedCounter--;
        }
    }

    void OnDestroy()
    {
        if (detected) alertedCounter--;
    }
}