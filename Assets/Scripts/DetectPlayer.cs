using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectPlayer : MonoBehaviour
{
    private LightSwitchBehavior switchBehavior;

    void Start()
    {
        GameObject parentObj = transform.parent.gameObject;
        switchBehavior = parentObj.GetComponent<LightSwitchBehavior>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switchBehavior.SetPlayerDetected(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switchBehavior.SetPlayerDetected(false);
        }
    }
}
